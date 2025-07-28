using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class HediffInjuryWithStacks : Hediff_Injury, IStackableHediff
    {
        protected int _CurrentStackLevel = 1;
        public int StackLevel => _CurrentStackLevel;
        public int MaxStackLevel
        {
            get
            {
                if (Def != null)
                {
                    if (Def.useStagesAsStackCount)
                    {
                        return Def.stages.Count;
                    }
                    return Def.maxStacks;
                }
                return def.stages.Count;
            }
        }

        public override string Label => base.Label + $" [{StackLevel + 1}]";

        public override string Description => base.Description + $"\r\n[{StackLevel + 1}] stacks.";
        public override HediffStage CurStage => GetStageForStackLevel(StackLevel);
        protected int stackLossTicker = 0;
        private StackingHediffDef Def => (StackingHediffDef)def;

        public override void Tick()
        {
            base.Tick();
            if (Def.losesStacksPerInterval)
            {
                stackLossTicker++;
                if (stackLossTicker >= Def.ticksBetweenStackLoss)
                {
                    RemoveStack(Def.stacksLostPerTickInterval);
                    stackLossTicker = 0;
                }
            }
        }

        public HediffStage GetStageForStackLevel(int Level)
        {
            if (def.stages == null || def.stages.Count == 0 || Level <= 0)
            {
                return null;
            }
            if (Level > def.stages.Count)
            {
                return def.stages[def.stages.Count - 1];
            }
            return def.stages[Level - 1];
        }

        public void SetStack(int stackLevel)
        {
            _CurrentStackLevel = stackLevel;
            OnStacksChange(stackLevel);

            if (_CurrentStackLevel >= MaxStackLevel)
            {
                OnMaxStacks();
            }
        }

        public void AddStack(int stacksToAdd = 1)
        {
            _CurrentStackLevel += stacksToAdd;
            if (_CurrentStackLevel > MaxStackLevel)
            {
                _CurrentStackLevel = MaxStackLevel;
            }

            if (this.TryGetComp(out HediffComp_Disappears _Disappears) && Def.stackGainRefreshesDisappearsDuration)
            {
                _Disappears.SetDuration(_Disappears.Props.disappearsAfterTicks.RandomInRange);
            }

            if (_CurrentStackLevel >= MaxStackLevel)
            {
                OnMaxStacks();
            }

            OnStacksChange(_CurrentStackLevel);
        }

        public void RemoveStack(int stacksToRemove = 1)
        {
            _CurrentStackLevel -= stacksToRemove;
            OnStacksChange(_CurrentStackLevel);

            if (_CurrentStackLevel <= 0)
            {
                _CurrentStackLevel = 0;
                if (Def != null && Def.removeOnZeroStacks)
                {
                    this.pawn.health.RemoveHediff(this);
                }
            }
        }

        protected virtual void OnStacksChange(int newStackLevel)
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnStacksChanged(newStackLevel);
                }
            }
        }

        protected virtual void OnMaxStacks()
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnMaxStacks();
                }
            }
        }
        public override bool TryMergeWith(Hediff other)
        {
            if (other is IStackableHediff stackableHediff && other.def == this.def)
            {
                AddStack(stackableHediff.StackLevel);

                if (this.TryGetComp(out HediffComp_Disappears disappears) &&
                    other.TryGetComp(out HediffComp_Disappears otherDisappears))
                {
                    if (Def.stackGainRefreshesDisappearsDuration)
                    {
                        disappears.SetDuration(disappears.Props.disappearsAfterTicks.RandomInRange);
                    }
                    else if (Def.stackGainAddsDisappearsDuration)
                    {
                        int remainingTicks = disappears.ticksToDisappear;
                        int addTicks = Def.stackGainDurationAdd;
                        disappears.SetDuration(remainingTicks + addTicks);
                    }
                }

                return true;
            }

            return base.TryMergeWith(other);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _CurrentStackLevel, "CurrentStackLevel", 0);
            Scribe_Values.Look(ref stackLossTicker, "StackLossTicker", 0);
        }
    }
    public class StackingHediffDef : HediffDef
    {
        public int initialStacks = 1;
        public int maxStacks = 10;
        public bool useStagesAsStackCount = false;
        public bool stackGainRefreshesDisappearsDuration = false;
        public bool stackGainAddsDisappearsDuration = false;
        public int stackGainDurationAdd = 1250;
        public bool losesStacksPerInterval = false;
        public int ticksBetweenStackLoss = 300;
        public int stacksLostPerTickInterval = 1;
        public bool removeOnZeroStacks = true;

        public StackingHediffDef()
        {
            hediffClass = typeof(HediffWithStacks);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (this.stages != null)
            {
                //hiding the severity not in order error since it doesnt use it at all.
                if (!typeof(Hediff_Addiction).IsAssignableFrom(this.hediffClass))
                {
                    yield break;
                }
            }

            foreach (var item in base.ConfigErrors())
            {
                yield return item;
            }
        }
    }

    public class HediffWithStacks : HediffWithComps, IStackableHediff
    {
        protected int _CurrentStackLevel = 1;
        public int StackLevel => _CurrentStackLevel;
        public int MaxStackLevel
        {
            get
            {
                if (Def != null)
                {
                    if (Def.useStagesAsStackCount)
                    {
                        return Def.stages.Count;
                    }
                    return Def.maxStacks;
                }
                return def.stages.Count;
            }
        }

        public override string Label => base.Label + $" [{StackLevel + 1}]";

        public override string Description => base.Description + $"\r\n[{StackLevel + 1}] stacks.";
        public override HediffStage CurStage => GetStageForStackLevel(StackLevel);
        protected int stackLossTicker = 0;
        private StackingHediffDef Def => (StackingHediffDef)def;
        public override void Tick()
        {
            base.Tick();
            if (Def.losesStacksPerInterval)
            {
                stackLossTicker++;
                if (stackLossTicker >= Def.ticksBetweenStackLoss)
                {
                    RemoveStack(Def.stacksLostPerTickInterval);
                    stackLossTicker = 0;
                }
            }
        }
        public HediffStage GetStageForStackLevel(int Level)
        {
            if (def.stages == null || def.stages.Count == 0 || Level <= 0)
            {
                return null;
            }
            if (Level > def.stages.Count)
            {
                return def.stages[def.stages.Count - 1];
            }
            return def.stages[Level - 1];
        }

        public void SetStack(int stackLevel)
        {
            _CurrentStackLevel = stackLevel;
            OnStacksChange(stackLevel);

            if (_CurrentStackLevel >= MaxStackLevel)
            {
                OnMaxStacks();
            }
        }

        public void AddStack(int stacksToAdd = 1)
        {
            _CurrentStackLevel += stacksToAdd;
            if (_CurrentStackLevel > MaxStackLevel)
            {
                _CurrentStackLevel = MaxStackLevel;

            }
            if (this.TryGetComp(out HediffComp_Disappears _Disappears) && Def.stackGainRefreshesDisappearsDuration)
            {
                _Disappears.SetDuration(_Disappears.Props.disappearsAfterTicks.RandomInRange);
            }

            if (_CurrentStackLevel >= MaxStackLevel)
            {
                OnMaxStacks();
            }

            OnStacksChange(_CurrentStackLevel);
        }

        public void RemoveStack(int stacksToRemove = 1)
        {
            _CurrentStackLevel -= stacksToRemove;
            OnStacksChange(_CurrentStackLevel);

            if (_CurrentStackLevel <= 0)
            {
                _CurrentStackLevel = 0;
                if (Def != null && Def.removeOnZeroStacks)
                {
                    this.pawn.health.RemoveHediff(this);
                }
            }

        }

        protected virtual void OnStacksChange(int newStackLevel)
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnStacksChanged(newStackLevel);
                }
            }
        }

        protected virtual void OnMaxStacks()
        {
            foreach (var item in comps)
            {
                if (item is HediffComp_BaseStack baseStack)
                {
                    baseStack.OnMaxStacks();
                }
            }
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (other is IStackableHediff stackableHediff && other.def == this.def)
            {
                AddStack(stackableHediff.StackLevel);

                if (this.TryGetComp(out HediffComp_Disappears disappears) &&
                    other.TryGetComp(out HediffComp_Disappears otherDisappears))
                {
                    if (Def.stackGainRefreshesDisappearsDuration)
                    {
                        disappears.SetDuration(disappears.Props.disappearsAfterTicks.RandomInRange);
                    }
                    else if (Def.stackGainAddsDisappearsDuration)
                    {
                        int remainingTicks = disappears.ticksToDisappear;
                        int addTicks = Def.stackGainDurationAdd;
                        disappears.SetDuration(remainingTicks + addTicks);
                    }
                }

                return true;
            }

            return base.TryMergeWith(other);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _CurrentStackLevel, "CurrentStackLevel", 0);
            Scribe_Values.Look(ref stackLossTicker, "StackLossTicker", 0);
        }
    }
    public abstract class HediffComp_OnMeleeAttackEffect : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EventManager.Instance.OnBeforeMeleeDamageInfo += Instance_OnBeforeMeleeDamageInfo;
            EventManager.Instance.OnVerbUsed += Instance_OnVerbUsed;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            EventManager.Instance.OnBeforeMeleeDamageInfo -= Instance_OnBeforeMeleeDamageInfo;
            EventManager.Instance.OnVerbUsed -= Instance_OnVerbUsed;
        }

        protected bool Instance_OnBeforeMeleeDamageInfo(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {
            if (attacker != this.parent.pawn)
            {
                return false;
            }

            OnBeforeMeleeDamage(attacker, target, ref damageInfo);
            return true;
        }

  

        private void Instance_OnVerbUsed(Pawn arg1, Verb arg2)
        {
            if (arg1 == this.parent.pawn)
            {
                if (arg2 is Verb_MeleeAttackDamage meleeAttackVerb)
                {
                    if (meleeAttackVerb.CurrentTarget != null)
                    {
                        OnMeleeAttack(meleeAttackVerb, meleeAttackVerb.CurrentTarget);
                    }
                }
          
            }
        }


        protected virtual void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {

        }

        protected virtual void OnBeforeMeleeDamage(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {

        }
    }
}
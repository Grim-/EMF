using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_LightningArmour : HediffCompProperties
    {
        public int ticksPerStackGain = 2500;
        public int stacksToRemoveOnHit = 1;
        public List<DamageDef> triggerDamageTypes = new List<DamageDef>();
        public float minimumDamageToTrigger = 0f;
        public HediffDef buffToGrantOnHit;
        public int buffDurationTicks = 300;
        public int stunDurationTicks = 60;
        public float stunChance = 1f;

        public HediffCompProperties_LightningArmour()
        {
            compClass = typeof(HediffComp_LightningArmour);
        }
    }


    public class HediffComp_LightningArmour : HediffComp_OnDamageTakenEffect
    {
        public new HediffCompProperties_LightningArmour Props => (HediffCompProperties_LightningArmour)props;

        private int ticksSinceLastStackGain = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticksSinceLastStackGain++;

            if (ticksSinceLastStackGain >= Props.ticksPerStackGain)
            {
                if (parent is IStackableHediff stackedHediff)
                {
                    stackedHediff.AddStack(1);
                    ticksSinceLastStackGain = 0;
                }
            }
        }

        protected override void OnDamageTaken(DamageInfo dinfo)
        {
            if (ShouldTriggerEffects(dinfo))
            {
                if (parent is IStackableHediff stackedHediff)
                {
                    stackedHediff.RemoveStack(Props.stacksToRemoveOnHit);
                }

                TryStunAttacker(dinfo);
                GrantSpeedBuff();
            }
        }

        private bool ShouldTriggerEffects(DamageInfo dinfo)
        {
            if (dinfo.Amount < Props.minimumDamageToTrigger)
            {
                return false;
            }

            if (Props.triggerDamageTypes.NullOrEmpty())
            {
                return true;
            }

            return Props.triggerDamageTypes.Contains(dinfo.Def);
        }

        private void TryStunAttacker(DamageInfo dinfo)
        {
            if (dinfo.Instigator is Pawn attacker && Rand.Chance(Props.stunChance))
            {
                attacker.stances.stunner.StunFor(Props.stunDurationTicks, parent.pawn);
            }
        }

        private void GrantSpeedBuff()
        {
            if (Props.buffToGrantOnHit != null && parent.pawn != null)
            {
                parent.pawn.health.GetOrAddHediff(Props.buffToGrantOnHit);
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksSinceLastStackGain, "ticksSinceLastStackGain", 0);
        }
    }
}
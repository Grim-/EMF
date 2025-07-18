using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace EMF
{
    public delegate bool BeforeThingDamageTakenHandler(Thing target, ref DamageInfo dinfo);
    public delegate bool BeforeMeleeDamageInfoHandler(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo);

    public class EventManager
    {
        private static readonly EventManager instance = new EventManager();

        public static EventManager Instance => instance;

        public event Func<Thing, Thing, DamageInfo, DamageWorker.DamageResult, DamageWorker.DamageResult> OnDamageDealt;
        public event Action<Pawn, DamageInfo> OnPawnDamageTaken;
        public event Action<Pawn, DamageInfo?, Hediff> OnPawnHediffGained;
        public event Action<Pawn, Hediff> OnPawnHediffRemoved;
        public event BeforeThingDamageTakenHandler OnBeforeThingDamageTaken;
        public event BeforeMeleeDamageInfoHandler OnBeforeMeleeDamageInfo;
        public event Action<Thing, DamageInfo> OnThingDamageTaken;
        public event Action<Pawn, DamageInfo, Hediff> OnThingKilled;

        public event Action<Pawn, WorkTypeDef, float> OnWorkCompleted;
        public event Action<Pawn, SkillDef, float> OnSkillGained;

        public event Action<Pawn, Verb> OnVerbUsed;
        public event Action<Pawn, Ability> OnAbilityCompleted;

        public event Action<Pawn, Job> OnJobStarted;
        public event Action<Pawn, Job, int> OnJobProgress;
        public event Action<Pawn, Job, JobCondition> OnJobEnded;
        public event Action<Pawn, Job, JobCondition> OnJobCleanedUp;

        public event Action<Pawn, IntVec3> OnCellEntered;
        public event Action<Pawn, IntVec3, IntVec3> OnPawnMoved;
        public event Func<Pawn, IntVec3, bool> OnPerceptionCheck;

        private EventManager()
        {

        }

        public DamageWorker.DamageResult RaiseDamageDealt(Thing target, Thing attacker, DamageInfo dinfo, DamageWorker.DamageResult baseResult)
        {
            return OnDamageDealt?.Invoke(target, attacker, dinfo, baseResult) ?? baseResult;
        }

        public void RaiseThingDamageTaken(Thing target, DamageInfo info)
        {
            OnThingDamageTaken?.Invoke(target, info);
        }

        public bool RaiseOnBeforeThingDamageTaken(Thing target, ref DamageInfo info)
        {
            if (OnBeforeThingDamageTaken != null)
            {
                foreach (BeforeThingDamageTakenHandler handler in OnBeforeThingDamageTaken.GetInvocationList())
                {
                    if (handler(target, ref info))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RaisePawnDamageTaken(Pawn target, DamageInfo info)
        {
            OnPawnDamageTaken?.Invoke(target, info);
        }

        public void RaiseOnPawnHediffGained(Pawn target, DamageInfo? info, Hediff hediff)
        {
            OnPawnHediffGained?.Invoke(target, info, hediff);
        }

        public void RaiseOnPawnHediffRemoved(Pawn target, Hediff hediff)
        {
            OnPawnHediffRemoved?.Invoke(target, hediff);
        }

        public void RaiseOnKilled(Pawn target, DamageInfo info, Hediff culprit = null)
        {
            OnThingKilled?.Invoke(target, info, culprit);
        }

        public void RaiseWorkCompleted(Pawn pawn, WorkTypeDef workType, float value)
        {
            OnWorkCompleted?.Invoke(pawn, workType, value);
        }

        public void RaiseSkillGained(Pawn pawn, SkillDef skill, float xp)
        {
            OnSkillGained?.Invoke(pawn, skill, xp);
        }

        public void RaiseVerbUsed(Pawn pawn, Verb verb)
        {
            OnVerbUsed?.Invoke(pawn, verb);
        }

        public bool RaiseBeforeMeleeDamageInfo(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {
            if (OnBeforeMeleeDamageInfo != null)
            {
                foreach (BeforeMeleeDamageInfoHandler handler in OnBeforeMeleeDamageInfo.GetInvocationList())
                {
                    if (handler(attacker, target, ref damageInfo))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RaiseAbilityCompleted(Pawn pawn, Ability ability)
        {
            Log.Message($"Raising ability {ability.def.label} completed for {pawn.LabelCap}");
            OnAbilityCompleted?.Invoke(pawn, ability);
        }

        public void RaiseJobStarted(Pawn pawn, Job job)
        {
            OnJobStarted?.Invoke(pawn, job);
        }

        public void RaiseJobProgress(Pawn pawn, Job job, int toilIndex)
        {
            OnJobProgress?.Invoke(pawn, job, toilIndex);
        }

        public void RaiseJobEnded(Pawn pawn, Job job, JobCondition condition)
        {
            OnJobEnded?.Invoke(pawn, job, condition);
        }

        public void RaiseJobCleanedUp(Pawn pawn, Job job, JobCondition condition)
        {
            OnJobCleanedUp?.Invoke(pawn, job, condition);
        }

        private int LastPatherArrivedEventTick = -1;

        public void PawnArrivedAtPathDestination(Pawn pawn, IntVec3 cell)
        {
            if (Current.Game.tickManager.TicksGame > LastPatherArrivedEventTick + 1)
            {
                OnCellEntered?.Invoke(pawn, cell);
                LastPatherArrivedEventTick = Current.Game.tickManager.TicksGame;
            }
        }

        public void RaisePawnMoved(Pawn pawn, IntVec3 fromCell, IntVec3 toCell)
        {
            OnPawnMoved?.Invoke(pawn, fromCell, toCell);
        }
    }
}

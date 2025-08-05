using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class CompProperties_BasePawnComp : CompProperties
    {
        public CompProperties_BasePawnComp()
        {
            compClass = typeof(Comp_BasePawnComp);
        }
    }

    public class Comp_BasePawnComp : ThingComp
    {
        protected Pawn EquippedPawn = null;


        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            EquippedPawn = pawn;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            EquippedPawn = null;
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
        }

        public virtual void Notify_OwnerKilled()
        {

        }

        public virtual void Notify_ProjectileImpact(Pawn attacker, Thing target, Projectile projectile)
        {

        }

        public virtual bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            return false;
        }

        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo damage, Pawn attacker, Thing target, Projectile projectile)
        {
            return damage;
        }

        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn attacker, ref DamageWorker.DamageResult damageWorkerResult)
        {
            return damageWorkerResult;
        }

        public virtual void Notify_OwnerThoughtGained(Thought thought, Pawn otherPawn)
        {

        }

        public virtual void Notify_OwnerThoughtLost(Thought thought)
        {

        }

        public virtual void Notify_OwnerHediffGained(Hediff hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
        }

        public virtual void Notify_OwnerHediffRemoved(Hediff hediff)
        {

        }

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }

        #region IStatProvider Implementation
        public virtual IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
        {
            yield break;
        }

        public virtual IEnumerable<StatModifier> GetStatFactors(StatDef stat)
        {
            yield break;
        }

        public virtual string GetExplanation(StatDef stat)
        {
            return string.Empty;
        }
        #endregion

        public override string CompInspectStringExtra()
        {
            return string.Empty;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }
    }
}

using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_ApplyHediffToAttacker : HediffCompProperties
    {
        public HediffDef hediffToApply;
        public float chance = 1f;
        public FloatRange severity = new FloatRange(1f, 1f);
        public bool meleeRangeOnly = true;
        public List<DamageDef> triggerDamageTypes = new List<DamageDef>();
        public float minimumDamageToTrigger = 0f;

        public HediffCompProperties_ApplyHediffToAttacker()
        {
            compClass = typeof(HediffComp_ApplyHediffToAttacker);
        }
    }

    public class HediffComp_ApplyHediffToAttacker : HediffComp_OnDamageTakenEffect
    {
        public HediffCompProperties_ApplyHediffToAttacker Props => (HediffCompProperties_ApplyHediffToAttacker)props;

        protected override void OnDamageTaken(DamageInfo dinfo)
        {
            if (ShouldApplyHediff(dinfo) && dinfo.Instigator is Pawn attackerPawn && Rand.Chance(Props.chance))
            {
                if (!Props.meleeRangeOnly || IsInMeleeRange(attackerPawn))
                {
                    Hediff newHediff = HediffMaker.MakeHediff(Props.hediffToApply, attackerPawn);
                    newHediff.Severity = Props.severity.RandomInRange;
                    attackerPawn.health.AddHediff(newHediff);
                }
            }
        }

        private bool ShouldApplyHediff(DamageInfo dinfo)
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

        private bool IsInMeleeRange(Thing attacker)
        {
            if (parent.pawn?.Position == null || attacker?.Position == null)
            {
                return false;
            }

            return parent.pawn.Position.AdjacentTo8WayOrInside(attacker.Position);
        }
    }
}
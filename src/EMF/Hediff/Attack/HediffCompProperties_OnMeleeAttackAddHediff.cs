using RimWorld;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackAddHediff : HediffCompProperties
    {
        public HediffDef hediffDef;
        public FloatRange intialSeverity = new FloatRange(0.1f, 0.1f);
        public FloatRange severityIncrease = new FloatRange(0.1f, 0.1f);
        public float chanceToApply = 0.5f;

        public HediffCompProperties_OnMeleeAttackAddHediff()
        {
            compClass = typeof(HediffComp_OnMeleeAttackAddHediff);
        }
    }

    public class HediffComp_OnMeleeAttackAddHediff : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_OnMeleeAttackAddHediff Props => (HediffCompProperties_OnMeleeAttackAddHediff)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);
            if (Target.Pawn != null)
            {
                if (Rand.Value <= Props.chanceToApply)
                {
                    Hediff hediff = null;
                    if (Target.Pawn.health.hediffSet.HasHediff(Props.hediffDef))
                    {
                        hediff = Target.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                        hediff.Severity += Props.severityIncrease.RandomInRange;
                    }
                    else
                    {
                        hediff = Target.Pawn.health.AddHediff(Props.hediffDef);
                        hediff.Severity = Props.intialSeverity.RandomInRange;
                    }
                }
            }
        }
    }
}
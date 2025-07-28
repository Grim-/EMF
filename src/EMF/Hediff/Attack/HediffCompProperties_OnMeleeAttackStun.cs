using RimWorld;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackStun : HediffCompProperties
    {
        public FloatRange chance;
        public IntRange stunTicks = new IntRange(200, 200);

        public HediffCompProperties_OnMeleeAttackStun()
        {
            compClass = typeof(HediffComp_OnMeleeAttackStun);
        }
    }

    public class HediffComp_OnMeleeAttackStun : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_OnMeleeAttackStun Props => (HediffCompProperties_OnMeleeAttackStun)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            if (Target.Pawn != null && Target.Pawn.stances?.stunner != null)
            {
                Target.Pawn.stances?.stunner.StunFor(Props.stunTicks.RandomInRange, this.parent.pawn);
            }
        }
    }
}
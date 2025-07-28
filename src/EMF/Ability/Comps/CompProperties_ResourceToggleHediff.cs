using RimWorld;
using Verse;

namespace EMF
{
    public class CompProperties_ResourceToggleHediff : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;

        public CompProperties_ResourceToggleHediff()
        {
            compClass = typeof(ToggleAbilityComp_ToggleHediff);
        }
    }

    public class ToggleAbilityComp_ToggleHediff : BaseToggleAbilityComp
    {
        public CompProperties_ResourceToggleHediff Props => (CompProperties_ResourceToggleHediff)props;

        public override void OnParentActivated()
        {
            parent.pawn.health.AddHediff(Props.hediffDef, null);
        }

        public override void OnParentDeactivated()
        {
            if (parent.pawn.health.hediffSet.HasHediff(Props.hediffDef))
            {
                parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef));
            }
        }
    }
}

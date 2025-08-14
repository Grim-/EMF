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

    public class ToggleAbilityComp_ToggleHediff : CompAbilityEffect_Toggleable
    {
        public CompProperties_ResourceToggleHediff Props => (CompProperties_ResourceToggleHediff)props;

        public override bool CanStart()
        {
            return true;
        }

        public override void OnToggleOff()
        {
            if (parent.pawn.health.hediffSet.HasHediff(Props.hediffDef))
            {
                parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef));
            }
        }

        public override void OnToggleOn()
        {
            parent.pawn.health.AddHediff(Props.hediffDef, null);
        }
    }
}

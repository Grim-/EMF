using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_HediffImpact : CompProperties_BaseJumpEffect
    {
        public HediffDef hediffDef;
        public float severity = 1f;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public CompProperties_HediffImpact()
        {
            compClass = typeof(CompAbilityEffect_HediffImpact);
        }
    }


    public class CompAbilityEffect_HediffImpact : CompAbilityEffect_BaseJumpEffect
    {
        CompProperties_HediffImpact Props => (CompProperties_HediffImpact)props;

        protected override void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            base.OnLand(arg1, arg2, arg3);

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.landingRadius, true).ToList();

            foreach (IntVec3 cell in cells)
            {
                Pawn pawn = cell.GetFirstPawn(arg3.Map);
                if (pawn != null && Props.friendlyFireSettings.CanTargetThing(pawn, this.parent.pawn))
                {
                    ApplyHediff(pawn);
                }
            }
        }

        private void ApplyHediff(Pawn target)
        {
            if (Props.hediffDef == null)
                return;

            Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                hediff.Severity += Props.severity;
            }
            else
            {
                hediff = HediffMaker.MakeHediff(Props.hediffDef, target);
                hediff.Severity = Props.severity;
                target.health.AddHediff(hediff);
            }
        }
    }
}

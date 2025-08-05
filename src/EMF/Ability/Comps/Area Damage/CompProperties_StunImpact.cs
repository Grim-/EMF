using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_StunImpact : CompProperties_BaseJumpEffect
    {
        public int stunTicks = 300;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();
        public EffecterDef stunEffecterDef;

        public CompProperties_StunImpact()
        {
            compClass = typeof(CompAbilityEffect_StunImpact);
        }
    }


    public class CompAbilityEffect_StunImpact : CompAbilityEffect_BaseJumpEffect
    {
        CompProperties_StunImpact Props => (CompProperties_StunImpact)props;

        protected override void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            base.OnLand(arg1, arg2, arg3);

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.landingRadius, true).ToList();

            foreach (IntVec3 cell in cells)
            {
                Pawn pawn = cell.GetFirstPawn(arg3.Map);
                if (pawn != null && Props.friendlyFireSettings.CanTargetThing(pawn, this.parent.pawn))
                {
                    pawn.stances.stunner.StunFor(Props.stunTicks, this.parent.pawn);

                    if (Props.stunEffecterDef != null)
                    {
                        Props.stunEffecterDef.Spawn(pawn.Position, pawn.Map);
                    }
                }
            }
        }
    }
}

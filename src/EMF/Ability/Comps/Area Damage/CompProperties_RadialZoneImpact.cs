using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_RadialZoneImpact : CompProperties_BaseJumpEffect
    {
        public ActiveZoneDef zoneDef;

        public CompProperties_RadialZoneImpact()
        {
            compClass = typeof(CompAbilityEffect_RadialZoneImpact);
        }
    }

    public class CompAbilityEffect_RadialZoneImpact : CompAbilityEffect_BaseJumpEffect
    {
        CompProperties_RadialZoneImpact Props => (CompProperties_RadialZoneImpact)props;

        protected override void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            base.OnLand(arg1, arg2, arg3);

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.landingRadius, true).ToList();

            if (Props.zoneDef != null)
            {
                ActiveZone activeZone = ActiveZone.SpawnZone(Props.zoneDef, this.parent.pawn.Position, cells, this.parent.pawn.Map, this.parent.pawn);
            }
        }
    }
}

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityDamageInRect : CompProperties_AreaDamageBase
    {
        public int length = 10;
        public int width = 3;
        public ThingDef effectMote = null;

        public CompProperties_AbilityDamageInRect()
        {
            compClass = typeof(CompAbilityEffect_AbilityDamageInRect);
        }
    }

    public class CompAbilityEffect_AbilityDamageInRect : CompAbilityEffect_AreaDamageBase
    {
        private CompProperties_AbilityDamageInRect Props => (CompProperties_AbilityDamageInRect)props;

        protected override IEnumerable<IntVec3> GetAffectedCells(IntVec3 origin, IntVec3 target, Map map)
        {
            return TargetUtil.GetAllCellsInRect(origin, target, Props.width, Props.length);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = parent.pawn.Map;

            if (Props.effectMote != null)
            {
                var mote = (MoteDualAttached)ThingMaker.MakeThing(Props.effectMote);
                GenSpawn.Spawn(mote, parent.pawn.Position, map, WipeMode.Vanish);
                mote.Attach(new TargetInfo(parent.pawn.Position, map), new TargetInfo(target.Cell, map));
                mote.linearScale = new Vector3(Props.width, 1f, (parent.pawn.DrawPos - target.Cell.ToVector3Shifted()).MagnitudeHorizontal());
            }

            StageVisualEffect.CreateStageEffect(GetAffectedCells(this.parent.pawn.Position, target.Cell, this.parent.pawn.Map).ToList(), map, Random.Range(8, 15), (cell, targetMap, sectionIndex) =>
            {
                foreach (var things in cell.GetThingList(targetMap))
                {
                    if (this.Props.friendlyFireParms.CanTargetThing(this.parent.pawn, things) && this.Props.targetParms.CanTarget(things))
                    {
                        DealDamageToThingsInCell(cell, targetMap);
                    }
                }
            });
        }
    }
}

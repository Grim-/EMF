using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityDamageInCone : CompProperties_AreaDamageBase
    {
        public int range = 10;
        public float angle = 45f;

        public CompProperties_AbilityDamageInCone()
        {
            compClass = typeof(CompAbilityEffect_AbilityDamageInCone);
        }
    }

    public class CompAbilityEffect_AbilityDamageInCone : CompAbilityEffect_AreaDamageBase
    {
       new private CompProperties_AbilityDamageInCone Props => (CompProperties_AbilityDamageInCone)props;

        protected override IEnumerable<IntVec3> GetAffectedCells(IntVec3 origin, IntVec3 target, Map map)
        {
            int range = (int)parent.verb.EffectiveRange;
            return TargetUtil.GetCellsInCone(origin, target, range, Props.angle);
        }



        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = parent.pawn.Map;
            var cells = GetAffectedCells(parent.pawn.Position, target.Cell, map).OrderBy(c => c.DistanceTo(parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, Random.Range(8, 15), (cell, targetMap, sectionIndex) =>
            {
                SpawnCellImpactEffect(cell, targetMap);
                DealDamageToThingsInCell(cell, targetMap);
            });
        }
    }
}

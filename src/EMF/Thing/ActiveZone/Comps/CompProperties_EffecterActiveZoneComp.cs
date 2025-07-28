using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_EffecterActiveZoneComp : CompProperties
    {
        public int ticksBetweenSpawns = 60;
        public float chancePerCell = 0.1f;
        public float totalCoverage = 1f;

        public EffecterDef effecterDef;

        public CompProperties_EffecterActiveZoneComp()
        {
            compClass = typeof(Effecter_ActiveZoneComp);
        }
    }

    public class Effecter_ActiveZoneComp : ActiveZoneComp
    {
        CompProperties_EffecterActiveZoneComp Props => (CompProperties_EffecterActiveZoneComp)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);

            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenSpawns))
            {
                if (Props.effecterDef == null || cells.NullOrEmpty())
                {
                    return;
                }

                int targetCellCount = Mathf.RoundToInt(cells.Count * Props.totalCoverage);
                int spawnedCount = 0;

                List<IntVec3> shuffledCells = cells.InRandomOrder().ToList();

                foreach (var cell in shuffledCells)
                {
                    if (spawnedCount >= targetCellCount)
                    {
                        break;
                    }

                    if (Rand.Chance(Props.chancePerCell))
                    {
                        Props.effecterDef.Spawn(cell, ParentZone.Map);
                        spawnedCount++;
                    }
                }
            }
        }
    }
}

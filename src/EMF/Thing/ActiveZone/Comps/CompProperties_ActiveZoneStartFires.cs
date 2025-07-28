using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_ActiveZoneStartFires : CompProperties
    {
        public int ticksBetweenFires = 500;
        public FloatRange fireAmount = new FloatRange(1, 1);
        public FloatRange coverage = new FloatRange(1, 1);
        public bool extinguishAllFiresOnDestroy = true;

        public CompProperties_ActiveZoneStartFires()
        {
            compClass = typeof(ActiveZoneComp_ActiveZoneStartFires);
        }
    }

    public class ActiveZoneComp_ActiveZoneStartFires : ActiveZoneComp
    {
        CompProperties_ActiveZoneStartFires Props => (CompProperties_ActiveZoneStartFires)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);
        }

        public override void OnZoneSpawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneSpawned(ParentZone, ref cells);


            Map map = parent.Map;

            foreach (var item in cells.Take((int)(cells.Count * Props.coverage.RandomInRange)))
            {
                FireUtility.TryStartFireIn(item, map, Props.fireAmount.RandomInRange, null);
            }
        }

        public override void OnZoneDespawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            Map map = parent.Map;
            foreach (var item in ParentZone.ZoneCells)
            {
                EMFUtil.TryExtinguishFireAt(item, map, Props.fireAmount.max + 500);
            }

            base.OnZoneDespawned(ParentZone, ref cells);
        }
    }
}

using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_SpawnsThing : ProjectileCompProperties
    {
        public ThingDef spawnsThingDef;
        public ThingDef stuff;
        public bool tryAdjacentFreeSpaces = false;

        public ProjectileCompProperties_SpawnsThing()
        {
            compClass = typeof(ProjectileComp_SpawnsThing);
        }
    }

    public class ProjectileComp_SpawnsThing : ProjectileComp
    {
        public ProjectileCompProperties_SpawnsThing Props => (ProjectileCompProperties_SpawnsThing)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield) 
                return;

            Map map = parent.Map;
            IntVec3 loc = parent.Position;

            SpawnThingAt(Props.spawnsThingDef, Props.stuff, loc, map);
        }


        protected void SpawnThingAt(ThingDef thingDef, ThingDef stuffDef, IntVec3 cell, Map map)
        {
            IntVec3 loc = cell;
            if (Props.tryAdjacentFreeSpaces && parent.Position.GetFirstBuilding(map) != null)
            {
                foreach (IntVec3 intVec in GenAdjFast.AdjacentCells8Way(parent.Position))
                {
                    if (intVec.GetFirstBuilding(map) == null && intVec.Standable(map))
                    {
                        loc = intVec;
                        break;
                    }
                }
            }

            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(thingDef, stuffDef), loc, map, WipeMode.Vanish);
            if (thing.def.CanHaveFaction && ParentAsProjectile.Launcher?.Faction != null)
            {
                thing.SetFaction(ParentAsProjectile.Launcher.Faction, null);
            }
        }
    }


}

using System.Linq;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_SpawnsActiveZoneRadius : ProjectileCompProperties
    {
        public ActiveZoneDef spawnsThingDef;
        public float radius = 5f;

        public ProjectileCompProperties_SpawnsActiveZoneRadius()
        {
            compClass = typeof(ProjectileComp_SpawnsActiveZoneRadius);
        }
    }

    public class ProjectileComp_SpawnsActiveZoneRadius : ProjectileComp
    {
        public ProjectileCompProperties_SpawnsActiveZoneRadius Props => (ProjectileCompProperties_SpawnsActiveZoneRadius)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            Map map = parent.Map;
            IntVec3 loc = parent.Position;
            ActiveZone activeZone = ActiveZone.SpawnZone(Props.spawnsThingDef, loc, GenRadial.RadialCellsAround(loc, Props.radius, true).ToList(), map, this.Launcher);
        }
    }
}

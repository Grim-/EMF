using Verse;

namespace EMF
{
    public class ProjectileCompProperties_SpawnsThingRadius : ProjectileCompProperties_SpawnsThing
    {
        public float radius = 3f;
        public int maxSpawnCount = -1;
        public float areaCoverage = 1f;
        public bool requiresLineOfSight = false;
        public bool onlyOnPassableTerrain = true;

        public ProjectileCompProperties_SpawnsThingRadius()
        {
            compClass = typeof(ProjectileComp_SpawnsThingRadius);
        }
    }

    public class ProjectileComp_SpawnsThingRadius : ProjectileComp_SpawnsThing
    {
        new public ProjectileCompProperties_SpawnsThingRadius Props => (ProjectileCompProperties_SpawnsThingRadius)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            Map map = parent.Map;
            if (map == null)
                return;

            SplashUtility.SplashThingsAround(
                center: parent.Position,
                map: map,
                radius: Props.radius,
                thingDef: Props.spawnsThingDef,
                stuffDef: Props.stuff,
                maxSpawnCount: Props.maxSpawnCount,
                areaCoverage: Props.areaCoverage,
                onlyOnPassableTerrain: Props.onlyOnPassableTerrain,
                requiresLineOfSight: Props.requiresLineOfSight,
                avoidPawns: false,
                avoidBuildings: true
            );
        }
    }
}

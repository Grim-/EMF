using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_AOEDamageAlongPath : ProjectileCompProperties
    {
        public DamageParameters damageParms;
        public float radius = 2.5f;
        public int damageInterval = 2;
        public bool growRadius = false;
        public float radiusGrowthRate = 0.1f;
        public float maxRadius = 10f;
        public EffecterDef flightEffecter = null;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.All();
        public ProjectileCompProperties_AOEDamageAlongPath()
        {
            compClass = typeof(ProjectileComp_AOEDamageAlongPath);
        }
    }
    public class ProjectileComp_AOEDamageAlongPath : ProjectileComp
    {
        private int initialFlightTime = 0;
        private Dictionary<Thing, int> lastDamageTick = new Dictionary<Thing, int>();

        public ProjectileCompProperties_AOEDamageAlongPath Props => (ProjectileCompProperties_AOEDamageAlongPath)props;

        public override void PostLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            initialFlightTime = 0;
            lastDamageTick.Clear();
        }

        public override void PostFlightTick()
        {
            CheckAndDamageTargetsInRadius();
        }

        private float GetCurrentRadius()
        {
            if (!Props.growRadius || initialFlightTime <= 0)
            {
                return Props.radius;
            }
            float flightProgress = 1f - ((float)ParentAsProjectile.TicksUntilImpact / initialFlightTime);
            float radiusIncrease = flightProgress * Props.radiusGrowthRate * initialFlightTime;
            float currentRadius = Props.radius + radiusIncrease;
            return UnityEngine.Mathf.Min(currentRadius, Props.maxRadius);
        }

        private void CheckAndDamageTargetsInRadius()
        {
            Map map = parent.Map;
            if (map == null)
                return;

            IntVec3 center = parent.Position;
            float currentRadius = GetCurrentRadius();
            int currentTick = Find.TickManager.TicksGame;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, currentRadius, true))
            {
                if (!cell.InBounds(map))
                    continue;

                foreach (Thing thing in map.thingGrid.ThingsListAt(cell).ToArray())
                {
                    if (thing is Pawn pawn && ShouldDamageThing(pawn))
                    {
                        if (!lastDamageTick.ContainsKey(pawn) || currentTick - lastDamageTick[pawn] >= Props.damageInterval)
                        {
                            lastDamageTick[pawn] = currentTick;

                            if (Props.flightEffecter != null)
                            {
                                Props.flightEffecter.Spawn(center, map);
                            }

                            if (Props.damageParms != null)
                            {
                                Props.damageParms.DealDamageTo(this.ParentAsProjectile.Launcher, pawn);
                            }
                        }
                    }
                }
            }
        }

        private bool ShouldDamageThing(Thing thing)
        {
            return thing != null && !thing.Destroyed && Props.friendlyFireSettings.CanTargetThing(thing, this.ParentAsProjectile.Launcher);
        }
    }
}

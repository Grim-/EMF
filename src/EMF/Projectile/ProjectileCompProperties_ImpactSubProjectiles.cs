using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactSubProjectiles : ProjectileCompProperties
    {
        public FloatRange targetSeekRadius = new FloatRange(2, 3);
        public List<ThingDef> projectileDefs;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.All();
        public int maxProjectileCount = 5;
        public bool requireUniqueTargets = true;
        public ProjectileCompProperties_ImpactSubProjectiles()
        {
            compClass = typeof(ProjectileComp_ImpactSubProjectiles);
        }
    }
    public class ProjectileComp_ImpactSubProjectiles : ProjectileComp
    {
        public ProjectileCompProperties_ImpactSubProjectiles Props => (ProjectileCompProperties_ImpactSubProjectiles)props;

        public override void PreImpactSomething()
        {
            base.PreImpactSomething();

            Map map = parent.Map;
            IntVec3 loc = parent.DrawPos.ToIntVec3();
            List<Pawn> targets = TargetUtil.GetPawnsInRadius(loc, map, Props.targetSeekRadius.RandomInRange, parent.Faction, Props.friendlyFireSettings, true);

            Log.Message(targets.Count);

            if (targets.NullOrEmpty() || Props.projectileDefs.NullOrEmpty())
                return;

            List<Pawn> selectedTargets = SelectTargets(targets);
            LaunchSubProjectiles(selectedTargets, loc, map);
        }

        private List<Pawn> SelectTargets(List<Pawn> availableTargets)
        {
            List<Pawn> selectedTargets = new List<Pawn>();

            if (Props.requireUniqueTargets)
            {
                selectedTargets = availableTargets.Take(Props.maxProjectileCount).ToList();
            }
            else
            {
                for (int i = 0; i < Props.maxProjectileCount && availableTargets.Any(); i++)
                {
                    selectedTargets.Add(availableTargets.RandomElement());
                }
            }

            return selectedTargets;
        }

        private void LaunchSubProjectiles(List<Pawn> targets, IntVec3 origin, Map map)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                Pawn target = targets[i];
                ThingDef projectileDef = Props.projectileDefs[i % Props.projectileDefs.Count];

                if (HasSubProjectileComponent(projectileDef))
                {
                    Log.Warning($"Skipping sub-projectile {projectileDef.defName} to prevent recursive spawning");
                    continue;
                }


                Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, origin, map, WipeMode.Vanish);
                projectile.Launch(this.ParentAsProjectile.Launcher, this.parent.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, false, null, null);
            }
        }

        private bool HasSubProjectileComponent(ThingDef projectileDef)
        {
            if (projectileDef?.comps == null)
                return false;

            return projectileDef.comps.Any(comp => comp is ProjectileCompProperties_ImpactSubProjectiles);
        }
    }
}

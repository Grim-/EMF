using UnityEngine;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties : CompProperties
    {
        public ProjectileCompProperties()
        {
            compClass = typeof(ProjectileComp);
        }
    }

    public abstract class ProjectileComp : ThingComp
    {
        public ProjectileWithComps ParentAsProjectile => (ProjectileWithComps)parent;

        //protected Thing Launcher;

        public virtual void PostLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            if (ParentAsProjectile == null)
            {
                return;
            }

            //Launcher = launcher;
        }

        public virtual void PostFlightTick()
        {
            if (ParentAsProjectile == null)
            {
                return;
            }

        }

        public virtual void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }

        public virtual void PostImpact(Thing hitThing, bool blockedByShield)
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }
        public virtual void PreImpactSomething()
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }

        public virtual void PostImpactSomething()
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }
        public override void PostDraw()
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }

        public virtual bool PreCheckForFreeIntercept(IntVec3 cell, ref bool shouldIntercept)
        {
            if (ParentAsProjectile == null)
            {
                return true;
            }

            return true;
        }

        public virtual void PostDestroy(DestroyMode mode)
        {
            if (ParentAsProjectile == null)
            {
                return;
            }
        }
    }
}

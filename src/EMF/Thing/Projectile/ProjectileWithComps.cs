using RimWorld;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ProjectileWithComps : Bullet
    {
        public int TicksUntilImpact => ticksToImpact;
        public int OverrideDamageAmount = -1;
        public override int DamageAmount => OverrideDamageAmount > 0 ? OverrideDamageAmount : base.DamageAmount;

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostLaunch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                }
            }
        }

        protected override void Tick()
        {
            base.Tick();

            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostFlightTick();
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PreImpact(hitThing, blockedByShield);
                }
            }

            base.Impact(hitThing, blockedByShield);

            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostImpact(hitThing, blockedByShield);
                }
            }
        }

        protected override void ImpactSomething()
        {
            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PreImpactSomething();
                }
            }

            base.ImpactSomething();

            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostImpactSomething();
                }
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);

            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostDraw();
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = 0; i < AllComps.Count; i++)
            {
                if (AllComps[i] is ProjectileComp comp)
                {
                    comp.PostDestroy(mode);
                }
            }

            base.Destroy(mode);
        }
    }

}

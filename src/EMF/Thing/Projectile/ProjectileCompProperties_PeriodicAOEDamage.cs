using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_PeriodicAOEDamage : ProjectileCompProperties
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(5f, 5f);
        public EffecterDef damageEffecterDef = null;
        public float radius = 1.9f;
        public int tickInterval = 60;
        public EffecterDef effecterDef = null;
        public int damageCooldownTicks = 30;
        public int effecterSpawnTicks = 8;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public ProjectileCompProperties_PeriodicAOEDamage()
        {
            compClass = typeof(ProjectileComp_PeriodicAOEDamage);
        }
    }

    public class ProjectileComp_PeriodicAOEDamage : ProjectileComp
    {
        private Thing launcher;
        private Dictionary<Thing, int> lastDamagedTicks = new Dictionary<Thing, int>();

        public ProjectileCompProperties_PeriodicAOEDamage Props => (ProjectileCompProperties_PeriodicAOEDamage)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            lastDamagedTicks = new Dictionary<Thing, int>();
        }

        public override void PostFlightTick()
        {
            base.PostFlightTick();
            //ApplyAreaDamage();

            if (this.parent.IsHashIntervalTick(Props.tickInterval))
            {
                ApplyAreaDamage();
            }

            if (this.parent.IsHashIntervalTick(Props.effecterSpawnTicks))
            {
                if (Props.effecterDef != null)
                {
                    Props.effecterDef.Spawn(this.parent.DrawPos.ToIntVec3(), this.parent.Map);
                }
                else
                {
                    EffecterDefOf.ImpactSmallDustCloud.Spawn(this.parent.DrawPos.ToIntVec3(), this.parent.Map);
                }
            }
        }

        private void ApplyAreaDamage()
        {
            Map map = this.parent.Map;
            if (map == null) 
                return;

            if (Props.effecterDef != null)
            {
                Effecter effect = Props.effecterDef.Spawn();
            }

            IntVec3 currentPosition = this.parent.DrawPos.ToIntVec3();
            if (currentPosition.IsValid)
            {
                foreach (var thing in currentPosition.GetThingList(map).ToList())
                {
                    if (Props.friendlyFireSettings.CanTargetThing(thing, this.Launcher))
                    {

                        if (Props.damageEffecterDef != null)
                        {
                            Props.damageEffecterDef.Spawn(thing.Position, map);
                        }


                        DamageInfo damage = new DamageInfo(Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb, Props.damageAmount.RandomInRange);
                        thing.TakeDamage(damage);
                        lastDamagedTicks[thing] = Find.TickManager.TicksGame;
                    }
                }
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref launcher, "launcher");
            Scribe_Collections.Look(ref lastDamagedTicks, "lastDamagedTicks", LookMode.Reference, LookMode.Value);
        }
    }


    public class CompProperties_LaunchProjectileExtended : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public IntRange launchAmount = new IntRange(1, 1);

        public CompProperties_LaunchProjectileExtended()
        {
            compClass = typeof(CompAbilityEffect_LaunchProjectileExtended);
        }
    }
    public class CompAbilityEffect_LaunchProjectileExtended : CompAbilityEffect
    {
        public new CompProperties_LaunchProjectileExtended Props => (CompProperties_LaunchProjectileExtended)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            this.LaunchProjectile(target);
        }

        private void LaunchProjectile(LocalTargetInfo target)
        {
            if (this.Props.projectileDef != null)
            {
                Pawn pawn = this.parent.pawn;

                for (int i = 0; i < Props.launchAmount.RandomInRange; i++)
                {
                    IntVec3 spawnCell = pawn.Position.RandomAdjacentCell8Way();

                    Projectile projectile = (Projectile)GenSpawn.Spawn(this.Props.projectileDef, spawnCell, pawn.Map, WipeMode.Vanish);

                    //if (projectile is ProjectileWithComps projectile_Extended)
                    //{
                    //    projectile_Extended.OverrideDamageAmount = (int)(projectile.DamageAmount * resourceAbility.GetDamageScalingMultiplier());
                    //}

                    projectile.Launch(pawn, spawnCell.ToVector3Shifted(), target, target, ProjectileHitFlags.IntendedTarget, false, null, null);
                }
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }
    }
}

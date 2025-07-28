using System.Linq;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactLightningStrike : ProjectileCompProperties
    {
        public float strikeRadius = 3f;
        public FloatRange strikeDamage = new FloatRange(5,5);
        public DamageDef strikeDamageDef;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public ProjectileCompProperties_ImpactLightningStrike()
        {
            compClass = typeof(ProjectileComp_ImpactLightningStrike);
        }
    }

    public class ProjectileComp_ImpactLightningStrike : ProjectileComp
    {
        public ProjectileCompProperties_ImpactLightningStrike Props => (ProjectileCompProperties_ImpactLightningStrike)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            Map map = parent.Map;
            IntVec3 loc = parent.Position;

            LightningStrike.GenerateLightningStrike(map, loc, Props.strikeRadius, (IntVec3 cell, Map cellMap) =>
            {
                foreach (var item in cell.GetThingList(cellMap).Where(x => x.CanTargetThing(this.ParentAsProjectile.Launcher.Faction, Props.friendlyFireSettings)).ToList())
                {
                    item.TakeDamage(new DamageInfo(Props.strikeDamageDef, Props.strikeDamage.RandomInRange, 0, -1, this.ParentAsProjectile.Launcher));
                }
            });
        }
    }
}

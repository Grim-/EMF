using RimWorld;
using System.Linq;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactAOEDamage : ProjectileCompProperties
    {
        public DamageParameters damageParms;
        public FloatRange radius = new FloatRange(3f, 3f);
        public IntRange sections = new IntRange(4,4);
        public EffecterDef effecterDef = null;
        public int ticksBetweenSections = 15;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.All();

        public ProjectileCompProperties_ImpactAOEDamage()
        {
            compClass = typeof(ProjectileComp_ImpactAOEDamage);
        }
    }

    public class ProjectileComp_ImpactAOEDamage : ProjectileComp
    {
        public ProjectileCompProperties_ImpactAOEDamage Props => (ProjectileCompProperties_ImpactAOEDamage)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            StageVisualEffect.CreateRadialStageEffect(this.parent.Position, Props.radius.RandomInRange, this.parent.Map, Props.sections.RandomInRange, (IntVec3 cell, Map map, int currentSection) =>
            {
                foreach (var item in cell.GetThingList(map).ToList())
                {
                    if (Props.friendlyFireSettings.CanTargetThing(item, this.ParentAsProjectile.Launcher))
                    {
                        Props.damageParms.DealDamageTo(this.ParentAsProjectile.Launcher, item);
                    }
              
                    if (Props.effecterDef != null)
                    {
                        Props.effecterDef.Spawn(item.Position, map);
                    }
                }
            }, Props.ticksBetweenSections);
        }
    }


}

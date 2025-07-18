using RimWorld;
using System.Linq;
using Verse;

namespace EMF
{



    public class ProjectileCompProperties_ImpactAOEDamage : ProjectileCompProperties
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10f, 10f);
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
                    DamageInfo damage = new DamageInfo(Props.damageDef != null ? Props.damageDef : DamageDefOf.Bomb, Props.damageAmount.RandomInRange);
                    item.TakeDamage(damage);

                    if (Props.effecterDef != null)
                    {
                        Props.effecterDef.Spawn(item.Position, map);
                    }
                }
            }, Props.ticksBetweenSections);
        }
    }


}

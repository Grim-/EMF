using RimWorld;
using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactStartOrStopFires : ProjectileCompProperties
    {
        public FloatRange radius = new FloatRange(3f, 3f);
        public FloatRange fireAmount = new FloatRange(1, 1);
        public IntRange sections = new IntRange(4, 4);
        public EffecterDef effecterDef = null;
        public int ticksBetweenSections = 15;

        public bool extinguishFires = false;

        public ProjectileCompProperties_ImpactStartOrStopFires()
        {
            compClass = typeof(ProjectileComp_ImpactStartOrStopFires);
        }
    }

    public class ProjectileComp_ImpactStartOrStopFires : ProjectileComp
    {
        public ProjectileCompProperties_ImpactStartOrStopFires Props => (ProjectileCompProperties_ImpactStartOrStopFires)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            if (this.parent.Map == null)
            {
                return;
            }

            StageVisualEffect.CreateRadialStageEffect(this.parent.Position, Props.radius.RandomInRange, this.parent.Map, Props.sections.RandomInRange, (IntVec3 cell, Map map, int currentSection) =>
            {
                if (cell.IsValid)
                {
                    if (Props.extinguishFires)
                    {
                        if (EMFUtil.TryExtinguishFireAt(cell, map, Props.fireAmount.RandomInRange))
                        {
                            if (Props.effecterDef != null)
                            {
                                Props.effecterDef.Spawn(cell, map);
                            }
                        }
                       
                    }
                    else
                    {
                        if (FireUtility.TryStartFireIn(cell, map, Props.fireAmount.RandomInRange, null))
                        {
                            if (Props.effecterDef != null)
                            {
                                Props.effecterDef.Spawn(cell, map);
                            }

                        }
                    }

                }
            }, Props.ticksBetweenSections);
        }
    }
}

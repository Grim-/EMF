using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactAOEHediff : ProjectileCompProperties
    {
        public HediffDef hediffDef;
        public FloatRange radius = new FloatRange(3f, 3f);
        public IntRange sections = new IntRange(4, 4);
        public EffecterDef effecterDef = null;
        public int ticksBetweenSections = 15;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.All();

        public ProjectileCompProperties_ImpactAOEHediff()
        {
            compClass = typeof(ProjectileComp_ImpactAOEHediff);
        }
    }

    public class ProjectileComp_ImpactAOEHediff : ProjectileComp
    {
        public ProjectileCompProperties_ImpactAOEHediff Props => (ProjectileCompProperties_ImpactAOEHediff)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            StageVisualEffect.CreateRadialStageEffect(this.parent.Position, Props.radius.RandomInRange, this.parent.Map, Props.sections.RandomInRange, (IntVec3 cell, Map map, int currentSection) =>
            {
                if (cell.GetDamageablePawn(map, this.ParentAsProjectile.Launcher.Faction, Props.friendlyFireSettings, out Pawn pawnInCell))
                {
                    pawnInCell.health.GetOrAddHediff(Props.hediffDef);
                }
            }, Props.ticksBetweenSections);
        }
    }
}

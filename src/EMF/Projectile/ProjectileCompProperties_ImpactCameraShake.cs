using Verse;

namespace EMF
{
    public class ProjectileCompProperties_ImpactCameraShake : ProjectileCompProperties
    {
        public IntRange ticks = new IntRange(80,80);
        public FloatRange shakeMagnitude = new FloatRange(1,1);

        public ProjectileCompProperties_ImpactCameraShake()
        {
            compClass = typeof(ProjectileComp_ImpactCameraShake);
        }
    }

    public class ProjectileComp_ImpactCameraShake : ProjectileComp
    {
        public ProjectileCompProperties_ImpactCameraShake Props => (ProjectileCompProperties_ImpactCameraShake)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;
            Current.CameraDriver.shaker.DoShake(Props.shakeMagnitude.RandomInRange, Props.ticks.RandomInRange);
        }
    }
}

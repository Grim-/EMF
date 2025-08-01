using RimWorld;

namespace EMF
{
    public class CompProperties_AbilityZoneEffect : CompProperties_AbilityEffect
    {
        public ActiveZoneDef zoneDef;
        public int zoneLifetime = 1000;

        public CompProperties_AbilityZoneEffect()
        {
            compClass = typeof(AbilityZoneEffect);
        }
    }

    public abstract class AbilityZoneEffect : CompAbilityEffect
    {
        public CompProperties_AbilityZoneEffect Props => (CompProperties_AbilityZoneEffect)props;

    }

}
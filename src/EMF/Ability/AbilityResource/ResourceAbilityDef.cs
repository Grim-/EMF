using RimWorld;

namespace EMF
{
    public class ResourceAbilityDef : AbilityDef
    {
        public AbilityResourceDef resourceDef;
        public float resourceCost = 10f;

        public ResourceAbilityDef()
        {
            abilityClass = typeof(ResourceAbility);
        }
    }
}

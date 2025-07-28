namespace EMF
{
    public class ResourceToggleAbilityDef : ResourceAbilityDef
    {
        public float resourceMaintainCost = 0;
        public int resourceMaintainInterval = 300;

        public ResourceToggleAbilityDef()
        {
            abilityClass = typeof(ResourceToggleAbility);
        }
    }
}

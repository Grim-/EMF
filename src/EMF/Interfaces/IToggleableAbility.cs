namespace EMF
{
    public interface IToggleableAbility
    {

        ResourceToggleAbilityDef ToggleDef { get; }

        void Activate(bool force = false);
        void DeActivate(bool force = false);
    }
}

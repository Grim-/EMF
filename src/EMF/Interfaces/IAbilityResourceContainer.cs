namespace EMF
{
    public interface IAbilityResourceContainer
    {
        AbilityResourceDef ResourceDef { get; }
        float CurrentValue { get; }
        float MaxValue { get; }

        bool ResourceLocked { get; }

        bool HasResource(float amount);
        void ConsumeResource(float amount);
        void RestoreResource(float amount);
    }
}
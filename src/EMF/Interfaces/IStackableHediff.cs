namespace EMF
{
    public interface IStackableHediff
    {
        int StackLevel { get; }
        int MaxStackLevel { get; }
        void AddStack(int stacksToAdd = 1);
        void RemoveStack(int stacksToRemove = 1);
        void SetStack(int stackLevel);
    }
}

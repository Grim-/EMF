using Verse;

namespace EMF
{
    public abstract class HediffCompProperties_BaseStack : HediffCompProperties
    {
    }

    public abstract class HediffComp_BaseStack : HediffComp
    {
        new public HediffCompProperties_BaseStack Props => (HediffCompProperties_BaseStack)props;
        public IStackableHediff ParentWithStacks => parent as IStackableHediff;

        public virtual void OnStacksChanged(int newStackAmount)
        {

        }

        public virtual void OnMaxStacks()
        {

        }
    }

}
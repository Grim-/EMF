using Verse;

namespace EMF
{
    public class HediffCompProperties_RemoveSelfAtMaxStacks : HediffCompProperties_BaseStack
    {
        public HediffDef hediffDef;

        public HediffCompProperties_RemoveSelfAtMaxStacks()
        {
            compClass = typeof(HediffComp_RemoveSelfAtMaxStacks);
        }
    }
    public class HediffComp_RemoveSelfAtMaxStacks : HediffComp_BaseStack
    {
        new public HediffCompProperties_AddHediffAtMaxStacks Props => (HediffCompProperties_AddHediffAtMaxStacks)props;

        public override void OnStacksChanged(int newStackAmount)
        {

        }

        public override void OnMaxStacks()
        {
            if (Props.hediffDef != null)
            {
                this.parent.pawn.health.RemoveHediff(this.parent);
            }
        }
    }
}
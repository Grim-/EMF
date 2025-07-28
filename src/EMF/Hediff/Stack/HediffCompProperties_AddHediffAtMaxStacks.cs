using Verse;

namespace EMF
{
    public class HediffCompProperties_AddHediffAtMaxStacks : HediffCompProperties_BaseStack
    {
        public HediffDef hediffDef;

        public HediffCompProperties_AddHediffAtMaxStacks()
        {
            compClass = typeof(HediffComp_AddHediffAtMaxStacks);
        }
    }


    public class HediffComp_AddHediffAtMaxStacks : HediffComp_BaseStack
    {
        new public HediffCompProperties_AddHediffAtMaxStacks Props => (HediffCompProperties_AddHediffAtMaxStacks)props;

        public override void OnStacksChanged(int newStackAmount)
        {

        }

        public override void OnMaxStacks()
        {
            if (Props.hediffDef != null)
            {
                this.parent.pawn.health.GetOrAddHediff(Props.hediffDef);
            }
        }
    }
}
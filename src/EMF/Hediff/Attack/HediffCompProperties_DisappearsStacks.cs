using Verse;

namespace EMF
{
    public class HediffCompProperties_DisappearsStacks : HediffCompProperties_Disappears
    {
        public int stacksToRemoveOnDisappear = 1;
        public bool resetTimerOnStackLoss = true;

        public HediffCompProperties_DisappearsStacks()
        {
            this.compClass = typeof(HediffComp_DisappearsStacks);
        }
    }

    public class HediffComp_DisappearsStacks : HediffComp_Disappears
    {
        public new HediffCompProperties_DisappearsStacks Props
        {
            get
            {
                return (HediffCompProperties_DisappearsStacks)this.props;
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                if (base.CompShouldRemove)
                    return true;

                IStackableHediff stackHediff = parent as IStackableHediff;
                if (stackHediff != null && stackHediff.StackLevel <= 0)
                    return true;

                if (Props.requiredMentalState != null && base.Pawn.MentalStateDef != Props.requiredMentalState)
                    return true;

                return false;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToDisappear -= TicksLostPerTick;

            if (ticksToDisappear <= 0)
            {
                IStackableHediff stackHediff = parent as IStackableHediff;
                if (stackHediff != null)
                {
                    stackHediff.RemoveStack(Props.stacksToRemoveOnDisappear);

                    if (stackHediff.StackLevel > 0 && Props.resetTimerOnStackLoss)
                    {
                        ticksToDisappear = disappearsAfterTicks;
                    }
                }
            }
        }
    }
}
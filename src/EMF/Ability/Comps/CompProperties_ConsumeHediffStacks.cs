using RimWorld;
using System;
using Verse;

namespace EMF
{


    public abstract class CompProperties_ConsumeHediffStacks : CompProperties_AbilityEffect
    {
        public HediffDef hediffToConsume;
        public bool consumeAllStacks = true;
        public int maxStacksToConsume = -1;

        public CompProperties_ConsumeHediffStacks()
        {
            compClass = typeof(CompAbilityEffect_ConsumeHediffStacks);
        }
    }


    public abstract class CompAbilityEffect_ConsumeHediffStacks : CompAbilityEffect
    {
        protected int consumedStacks = 0;

        public new CompProperties_ConsumeHediffStacks Props => (CompProperties_ConsumeHediffStacks)this.props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null)
                return;

            consumedStacks = ConsumeHediffStacks(pawn);
            ApplyWithConsumedStacks(target, dest, consumedStacks);
        }

        protected int ConsumeHediffStacks(Pawn pawn)
        {
            if (Props.hediffToConsume == null)
                return 0;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToConsume);
            if (hediff == null)
                return 0;

            IStackableHediff stackHediff = hediff as IStackableHediff;
            if (stackHediff == null)
                return 0;

            int currentStacks = stackHediff.StackLevel;
            if (currentStacks <= 0)
                return 0;

            int stacksToConsume = currentStacks;
            if (!Props.consumeAllStacks && Props.maxStacksToConsume > 0)
            {
                stacksToConsume = Math.Min(currentStacks, Props.maxStacksToConsume);
            }

            stackHediff.RemoveStack(stacksToConsume);
            return stacksToConsume;
        }

        protected abstract void ApplyWithConsumedStacks(LocalTargetInfo target, LocalTargetInfo dest, int consumedStacks);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref consumedStacks, "consumedStacks", 0);
        }
    }
}

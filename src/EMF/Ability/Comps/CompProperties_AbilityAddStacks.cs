using RimWorld;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityAddStacks : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public int stacksToGive = 1;

        public CompProperties_AbilityAddStacks()
        {
            compClass = typeof(CompAbilityEffect_AddStacks);
        }
    }


    public class CompAbilityEffect_AddStacks : CompAbilityEffect
    {
        public new CompProperties_AbilityAddStacks Props => (CompProperties_AbilityAddStacks)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Pawn targetPawn = target.Pawn ?? parent.pawn;
            if (targetPawn != null && Props.hediffDef != null)
            {
                Hediff existingHediff = targetPawn.health.GetOrAddHediff(Props.hediffDef);

                if (existingHediff is IStackableHediff stackedHediff)
                {
                    stackedHediff.AddStack(Props.stacksToGive);
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return target.Pawn != null || parent.pawn != null;
        }
    }
}

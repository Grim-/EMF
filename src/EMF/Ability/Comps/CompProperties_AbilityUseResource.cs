using RimWorld;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityUseResource : CompProperties_AbilityEffect
    {
        public AbilityResourceDef resourceDef;
        public float resourceCost = 10f;

        public CompProperties_AbilityUseResource()
        {
            compClass = typeof(CompAbilityEffect_UseResource);
        }
    }
    public class CompAbilityEffect_UseResource : CompAbilityEffect
    {
        public new CompProperties_AbilityUseResource Props => (CompProperties_AbilityUseResource)props;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages)) return false;

            // 1. Try to find the Interface on a Gene
            IAbilityResourceContainer holder = parent.pawn.GetResourceHolder(Props.resourceDef);

            if (holder == null)
            {
                if (throwMessages) Messages.Message("No resource source found", MessageTypeDefOf.RejectInput);
                return false;
            }

            // 2. Use the Interface methods
            if (!holder.HasResource(Props.resourceCost))
            {
                if (throwMessages) Messages.Message($"Not enough {Props.resourceDef.resourceName}", MessageTypeDefOf.RejectInput);
                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            IAbilityResourceContainer holder = parent.pawn.GetResourceHolder(Props.resourceDef);
            holder?.ConsumeResource(Props.resourceCost);
        }

        // The Helper that bridges the gap

    }

}

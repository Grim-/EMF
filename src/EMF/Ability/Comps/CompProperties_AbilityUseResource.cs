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
            if (!base.Valid(target, throwMessages))
                return false;

            Gene_BasicResource resourceGene = GetResourceGene();
            if (resourceGene == null)
            {
                if (throwMessages)
                    Messages.Message("No resource gene found", MessageTypeDefOf.RejectInput);
                return false;
            }

            if (!resourceGene.Has(Props.resourceCost))
            {
                if (throwMessages)
                    Messages.Message($"Not enough {Props.resourceDef.resourceName}", MessageTypeDefOf.RejectInput);
                return false;
            }


            if (resourceGene.ResourceIsUnavailable(out string reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Gene_BasicResource resourceGene = GetResourceGene();
            if (resourceGene != null && resourceGene.Has(Props.resourceCost))
            {
                resourceGene.Consume(Props.resourceCost);
            }
        }

        private Gene_BasicResource GetResourceGene()
        {
            if (parent.pawn?.genes?.GenesListForReading == null || Props.resourceDef == null)
                return null;

            return parent.pawn.GetGeneForResourceDef(Props.resourceDef);
        }
    }

}

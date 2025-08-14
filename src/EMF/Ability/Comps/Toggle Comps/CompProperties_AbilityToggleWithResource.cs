using RimWorld;
using System.Linq;
using Verse;

namespace EMF
{
    public class CompProperties_AbilityToggleWithResource : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;
        public AbilityResourceDef resourceDef;
        public float activationCost = 10f;

        public CompProperties_AbilityToggleWithResource()
        {
            compClass = typeof(CompAbilityEffect_ToggleWithResource);
        }
    }

    public class CompAbilityEffect_ToggleWithResource : CompAbilityEffect
    {
        public new CompProperties_AbilityToggleWithResource Props => (CompProperties_AbilityToggleWithResource)props;

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages))
                return false;

            if (parent.pawn == null || Props.hediffDef == null)
                return false;

            bool hasHediff = parent.pawn.health.hediffSet.HasHediff(Props.hediffDef);

            if (!hasHediff)
            {
                Gene_BasicResource resourceGene = GetResourceGene();
                if (resourceGene == null)
                {
                    if (throwMessages)
                        Messages.Message("No resource gene found", MessageTypeDefOf.RejectInput);
                    return false;
                }

                if (!resourceGene.Has(Props.activationCost))
                {
                    if (throwMessages)
                        Messages.Message($"Not enough {Props.resourceDef.resourceName}", MessageTypeDefOf.RejectInput);
                    return false;
                }
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn == null || Props.hediffDef == null)
                return;

            if (this.parent is ResourceToggleAbility toggleAbility)
            {
                if (toggleAbility.IsActive)
                {
                    Apply();
                }
                else Remove();
            }
            else
            {
                bool hasHediff = parent.pawn.health.hediffSet.HasHediff(Props.hediffDef);

                if (hasHediff)
                {
                    Remove();
                }
                else
                {
                    Apply();
                }
            }

        }

        private void Apply()
        {
            Gene_BasicResource resourceGene = GetResourceGene();
            if (resourceGene != null && resourceGene.Has(Props.activationCost))
            {
                resourceGene.Consume(Props.activationCost);
                parent.pawn.health.AddHediff(Props.hediffDef, null);
            }
        }

        private void Remove()
        {
            parent.pawn.health.RemoveHediff(parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef));
        }

        private Gene_BasicResource GetResourceGene()
        {
            if (parent.pawn?.genes?.GenesListForReading == null || Props.resourceDef == null)
                return null;

            return parent.pawn.genes.GenesListForReading
                .OfType<Gene_BasicResource>()
                .FirstOrDefault(g => g.ResourceDef == Props.resourceDef);
        }
    }
}

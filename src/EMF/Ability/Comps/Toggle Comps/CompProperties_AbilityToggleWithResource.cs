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
                IAbilityResourceContainer container = parent.pawn.GetResourceHolder(Props.resourceDef);

                if (container == null)
                {
                    if (throwMessages)
                        Messages.Message("No resource source found", MessageTypeDefOf.RejectInput);
                    return false;
                }

                if (!container.HasResource(Props.activationCost))
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
                    ApplyToggle();
                }
                else
                {
                    RemoveToggle();
                }
            }
            else
            {
                bool hasHediff = parent.pawn.health.hediffSet.HasHediff(Props.hediffDef);

                if (hasHediff)
                {
                    RemoveToggle();
                }
                else
                {
                    ApplyToggle();
                }
            }
        }

        private void ApplyToggle()
        {
            IAbilityResourceContainer container = parent.pawn.GetResourceHolder(Props.resourceDef);

            if (container != null && container.HasResource(Props.activationCost))
            {
                container.ConsumeResource(Props.activationCost);
                parent.pawn.health.AddHediff(Props.hediffDef, null);
            }
        }

        private void RemoveToggle()
        {
            Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                parent.pawn.health.RemoveHediff(hediff);
            }
        }
    }
}

using Verse;

namespace EMF
{
    public abstract class HediffCompProperties_BaseInterval : HediffCompProperties
    {
        public int intervalTicks = 2400;

        public EffecterDef intervalEffector;
    }

    public abstract class HediffComp_BaseInterval : HediffComp
    {
        new public HediffCompProperties_BaseInterval Props => (HediffCompProperties_BaseInterval)props;
        protected int ticks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticks++;
            if (ticks >= Props.intervalTicks)
            {
                OnInterval();
                ticks = 0;
            }
        }


        protected virtual void OnInterval()
        {
            if (Props.intervalEffector != null)
            {
                Props.intervalEffector.Spawn(this.Pawn.Position, this.Pawn.Map, 2);
            }
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
        }

    }

    public class HediffCompProperties_UseResource : HediffCompProperties
    {
        public AbilityResourceDef resourceDef;
        public float resourceCostPerInterval = 1f;
        public int intervalTicks = 2500;
        public bool removeOnInsufficientResource = true;

        public HediffCompProperties_UseResource()
        {
            compClass = typeof(HediffComp_UseResource);
        }
    }

    public class HediffComp_UseResource : HediffComp
    {
        public HediffCompProperties_UseResource Props => (HediffCompProperties_UseResource)props;

        private int ticksSinceLastCost = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticksSinceLastCost++;

            if (ticksSinceLastCost >= Props.intervalTicks)
            {
                Gene_BasicResource resourceGene = GetResourceGene();
                if (resourceGene != null)
                {
                    if (resourceGene.Has(Props.resourceCostPerInterval))
                    {
                        resourceGene.Consume(Props.resourceCostPerInterval);
                        ticksSinceLastCost = 0;
                    }
                    else if (Props.removeOnInsufficientResource)
                    {
                        parent.pawn.health.RemoveHediff(parent);
                    }
                }
                else if (Props.removeOnInsufficientResource)
                {
                    parent.pawn.health.RemoveHediff(parent);
                }
            }
        }

        private Gene_BasicResource GetResourceGene()
        {
            return parent.pawn.GetGeneForResourceDef(Props.resourceDef);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticksSinceLastCost, "ticksSinceLastCost", 0);
        }
    }

}
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



}
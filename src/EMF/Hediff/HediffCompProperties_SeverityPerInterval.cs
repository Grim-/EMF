using Verse;

namespace EMF
{
    public class HediffCompProperties_SeverityPerInterval : HediffCompProperties_BaseInterval
    {
        public FloatRange severityChange = new FloatRange(-0.1f, -0.1f);
        public HediffCompProperties_SeverityPerInterval()
        {
            compClass = typeof(HediffComp_SeverityPerInterval);
        }
    }

    public class HediffComp_SeverityPerInterval : HediffComp_BaseInterval
    {
       new public HediffCompProperties_SeverityPerInterval Props => (HediffCompProperties_SeverityPerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();

            this.parent.Severity += Props.severityChange.RandomInRange;

        }
    }
}
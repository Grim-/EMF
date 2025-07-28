using Verse;

namespace EMF
{


    public class HediffCompProperties_AddHediffAOEPerInterval : HediffCompProperties_BaseInterval
    {
        public HediffDef hediff;
        public float radius = 5;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public HediffCompProperties_AddHediffAOEPerInterval()
        {
            compClass = typeof(HediffComp_AddHediffAOEPerInterval);
        }
    }

    public class HediffComp_AddHediffAOEPerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_AddHediffAOEPerInterval Props => (HediffCompProperties_AddHediffAOEPerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();
            if (Props.hediff != null)
            {
                TargetUtil.ApplyHediffInRadius(Props.hediff,
                    Pawn.Position,
                    Pawn.Map,
                    Props.radius,
                    Pawn.Faction,
                    Props.friendlyFireSettings,
                    true);


            }
        }
    }
}
using RimWorld;
using Verse;

namespace EMF
{


    public class HediffCompProperties_HealAOEPerInterval : HediffCompProperties_BaseInterval
    {
        public IntRange healAmount = new IntRange(3, 5);
        public float radius = 5;
        public int targetLimit = 5;

        public EffecterDef healEffecterDef = null;

        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.FriendlyFactionOnly();

        public HediffCompProperties_HealAOEPerInterval()
        {
            compClass = typeof(HediffComp_HealAOEPerInterval);
        }
    }

    public class HediffComp_HealAOEPerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_HealAOEPerInterval Props => (HediffCompProperties_HealAOEPerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();


            int targets = 0;

            foreach (var item in TargetUtil.GetPawnsInRadius(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, this.parent.pawn.Faction, Props.friendlyFireSettings, true))
            {
                if (targets >= Props.targetLimit)
                {
                    break;
                }

                if (!item.NeedsHealing())
                {
                    continue;
                }

                if (Props.healEffecterDef != null)
                {
                    Props.healEffecterDef.SpawnAttached(item, item.Map);
                }
                float healedAmount = item.SpendHealingAmount(Props.healAmount.RandomInRange, HealParameters.InjuriesOnly());
                MoteMaker.ThrowText(item.Position.ToVector3(), item.Map, $"<color=green>Healed {healedAmount}</color>");
                targets++;
            }
        }
    }
}
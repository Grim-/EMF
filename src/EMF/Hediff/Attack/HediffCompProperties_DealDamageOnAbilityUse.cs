using EMF;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnAbilityUseDealDamage : HediffCompProperties_AbilityEffect
    {
        public DamageDef damageDef;
        public FloatRange damageRange;
        public FloatRange armourpenRange;

        public bool isAOE = false;
        public float radius = 5;
        public FriendlyFireSettings friendlyFireSettings = FriendlyFireSettings.HostileOnly();

        public HediffCompProperties_OnAbilityUseDealDamage()
        {
            compClass = typeof(HediffComp_OnAbilityUseDealDamage);
        }
    }

    public class HediffComp_OnAbilityUseDealDamage : HediffComp_AbilityEffect
    {
        public HediffCompProperties_OnAbilityUseDealDamage Props => (HediffCompProperties_OnAbilityUseDealDamage)props;

        protected override void OnAbilityUsed(Pawn pawn, RimWorld.Ability ability)
        {
            if (Props.isAOE)
            {
                TargetUtil.ApplyDamageInRadius(Props.damageDef,
                    Props.damageRange.RandomInRange,
                    Props.armourpenRange.RandomInRange,
                    Pawn.Position,
                    Pawn.Map,
                    Props.radius,
                    Pawn.Faction,
                    Props.friendlyFireSettings,
                    true,
                    Pawn);
            }
            else
            {
                Pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damageRange.RandomInRange, Props.armourpenRange.RandomInRange));
            }
        }

        public override string CompDescriptionExtra => base.CompDescriptionExtra + $"\r\nDeals {Props.damageRange.min} - {Props.damageRange.max} {Props.damageDef.LabelCap} damage every time an ability is used.";
    }
}
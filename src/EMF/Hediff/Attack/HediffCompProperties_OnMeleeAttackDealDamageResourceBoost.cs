using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackDealDamageResourceBoost : HediffCompProperties_OnMeleeAttackDealDamage
    {
        public AbilityResourceDef resourceToUse;
        public float resourceCost = 5f;

        public float damageMutlplier = 1;
        public float damageFlatAmount = 0;

        public HediffCompProperties_OnMeleeAttackDealDamageResourceBoost()
        {
            compClass = typeof(HediffComp_OnMeleeAttackDealDamageResourceBoost);
        }
    }

    public class HediffComp_OnMeleeAttackDealDamageResourceBoost : HediffComp_OnMeleeAttackDealDamage
    {
        HediffCompProperties_OnMeleeAttackDealDamageResourceBoost Props => (HediffCompProperties_OnMeleeAttackDealDamageResourceBoost)props;

        protected override DamageInfo GetDamage()
        {
            DamageInfo damage = base.GetDamage();

            if (this.parent.pawn.HasResourceDef(Props.resourceToUse, out Gene_BasicResource basicResource))
            {
                if (basicResource.Has(Props.resourceToUse, Props.resourceCost))
                {
                    damage.SetAmount(damage.Amount + Props.damageFlatAmount * Props.damageMutlplier);
                }
            }

            return damage;
        }
    }
}
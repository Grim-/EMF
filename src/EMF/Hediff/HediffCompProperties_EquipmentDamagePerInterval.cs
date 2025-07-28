using RimWorld;
using Verse;

namespace EMF
{
    public class HediffCompProperties_EquipmentDamagePerInterval : HediffCompProperties_BaseInterval
    {
        public FloatRange damageAmount;
        public HediffCompProperties_EquipmentDamagePerInterval()
        {
            compClass = typeof(HediffComp_EquipmentDamagePerInterval);
        }
    }

    public class HediffComp_EquipmentDamagePerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_EquipmentDamagePerInterval Props => (HediffCompProperties_EquipmentDamagePerInterval)props;

        protected override void OnInterval()
        {
            base.OnInterval();

            if (this.Pawn != null && this.Pawn.EquippedWornOrInventoryThings.EnumerableCount() > 0)
            {
                foreach (var item in this.Pawn.EquippedWornOrInventoryThings)
                {
                    item.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Props.damageAmount.RandomInRange));
                }
            }
        }
    }
}
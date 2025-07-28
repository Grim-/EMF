using Verse;

namespace EMF
{
    public class HediffCompProperties_DamagePerInterval : HediffCompProperties_BaseInterval
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10, 10);
        public FloatRange armourPenAmount = new FloatRange(0, 1);
        public bool applyDamageToParentPart = true;
        public HediffCompProperties_DamagePerInterval()
        {
            compClass = typeof(HediffComp_DamagePerInterval);
        }
    }

    public class HediffComp_DamagePerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_DamagePerInterval Props => (HediffCompProperties_DamagePerInterval)props;
        protected override void OnInterval()
        {
            base.OnInterval();
            if (Props.damageDef != null)
            {
                DamageInfo damageInfo;
                if (Props.applyDamageToParentPart && parent.Part != null)
                {
                    damageInfo = new DamageInfo(Props.damageDef, Props.damageAmount.RandomInRange, Props.armourPenAmount.RandomInRange, -1f, null, parent.Part);
                }
                else
                {
                    damageInfo = new DamageInfo(Props.damageDef, Props.damageAmount.RandomInRange, Props.armourPenAmount.RandomInRange);
                }
                this.Pawn.TakeDamage(damageInfo);
            }
        }
    }

}
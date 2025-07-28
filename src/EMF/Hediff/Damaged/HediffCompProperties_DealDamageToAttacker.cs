using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class HediffCompProperties_DealDamageToAttacker : HediffCompProperties
    {
        public FloatRange damage;
        public DamageDef damageDef;
        public bool meleeRangeOnly = true;
        public List<DamageDef> triggerDamageTypes = new List<DamageDef>();
        public float minimumDamageToTrigger = 0f;

        public HediffCompProperties_DealDamageToAttacker()
        {
            compClass = typeof(HediffComp_DealDamageToAttacker);
        }
    }

    public class HediffComp_DealDamageToAttacker : HediffComp_OnDamageTakenEffect
    {
        public HediffCompProperties_DealDamageToAttacker Props => (HediffCompProperties_DealDamageToAttacker)props;

        protected override void OnDamageTaken(DamageInfo dinfo)
        {
            if (ShouldDealDamage(dinfo) && dinfo.Instigator is Thing attacker)
            {
                if (!Props.meleeRangeOnly || IsInMeleeRange(attacker))
                {
                    DamageInfo retaliation = new DamageInfo(
                        Props.damageDef,
                        Props.damage.RandomInRange,
                        instigator: parent.pawn
                    );
                    attacker.TakeDamage(retaliation);
                }
            }
        }

        private bool ShouldDealDamage(DamageInfo dinfo)
        {
            if (dinfo.Amount < Props.minimumDamageToTrigger)
            {
                return false;
            }

            if (Props.triggerDamageTypes.NullOrEmpty())
            {
                return true;
            }

            return Props.triggerDamageTypes.Contains(dinfo.Def);
        }

        private bool IsInMeleeRange(Thing attacker)
        {
            if (parent.pawn?.Position == null || attacker?.Position == null)
            {
                return false;
            }

            return parent.pawn.Position.AdjacentTo8WayOrInside(attacker.Position);
        }
    }
}
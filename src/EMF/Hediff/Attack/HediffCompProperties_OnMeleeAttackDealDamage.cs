using RimWorld;
using Verse;

namespace EMF
{
    public class HediffCompProperties_OnMeleeAttackDealDamage : HediffCompProperties
    {
        public FloatRange damage = new FloatRange(1, 1);
        public DamageDef damageDef;
        public FloatRange armourPen = new FloatRange(0, 0);
        public FloatRange chance = new FloatRange(1, 1);



        public bool useWeaponDamage = false;
        public FloatRange weaponDamageMultiplier = new FloatRange(1, 1);
        public FloatRange weaponArmourPen = new FloatRange(-1, -1);

        public HediffCompProperties_OnMeleeAttackDealDamage()
        {
            compClass = typeof(HediffComp_OnMeleeAttackDealDamage);
        }
    }

    public class HediffComp_OnMeleeAttackDealDamage : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_OnMeleeAttackDealDamage Props => (HediffCompProperties_OnMeleeAttackDealDamage)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            if (Target.Thing != null)
            {
                DamageInfo damage = GetDamage();

                if (Props.useWeaponDamage && Props.damageDef != null)
                    damage.Def = Props.damageDef;

                Target.Thing.TakeDamage(damage);
            }
        }

        protected virtual DamageInfo GetDamage()
        {
            if (Props.useWeaponDamage && this.parent.pawn.HasWeaponEquipped())
            {
                DamageInfo damage = this.parent.pawn.equipment.PrimaryEq.GetWeaponDamage(this.parent.pawn, Props.weaponDamageMultiplier.RandomInRange, Props.weaponArmourPen.RandomInRange);
                damage.SetAmount(damage.Amount + Props.damage.RandomInRange);
                return damage;
            }

            return new DamageInfo(Props.damageDef, Props.damage.RandomInRange, Props.armourPen.RandomInRange, -1, this.parent.pawn);
        }
    }


}
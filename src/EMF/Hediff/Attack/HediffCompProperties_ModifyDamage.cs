using Verse;

namespace EMF
{
    public class HediffCompProperties_ModifyDamage : HediffCompProperties
    {
        public HediffCompProperties_ModifyDamage()
        {
            compClass = typeof(HediffComp_ModifyDamage);
        }
    }

    public class HediffComp_ModifyDamage : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_ModifyDamage Props => (HediffCompProperties_ModifyDamage)props;

        protected override void OnBeforeMeleeDamage(Pawn attacker, LocalTargetInfo target, ref DamageInfo damageInfo)
        {
            base.OnBeforeMeleeDamage(attacker, target, ref damageInfo);
            Log.Message("setting armour pen to 1");
            damageInfo.SetIgnoreArmor(true);
        }
    }
}
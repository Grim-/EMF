using RimWorld;
using Verse;

namespace EMF
{
    public class DamageParameters
    {
        public DamageDef damageDef;
        public FloatRange damageAmount;
        public FloatRange armourPenAmount;
        public bool useWeaponDamageIfAvailable = false;
        public EffecterDef damageEffecterDef = null;

        public DamageDef weaponDamageDef;
        public FloatRange weaponDamageMult = new FloatRange(1f, 1f);

        public DamageParameters()
        {

        }

        public DamageParameters(DamageDef damageDef, FloatRange damageAmount, FloatRange armourPenAmount, bool useWeaponDamageIfAvailable, EffecterDef damageEffecterDef, DamageDef weaponDamageDef, FloatRange weaponDamageMult)
        {
            this.damageDef = damageDef;
            this.damageAmount = damageAmount;
            this.armourPenAmount = armourPenAmount;
            this.useWeaponDamageIfAvailable = useWeaponDamageIfAvailable;
            this.damageEffecterDef = damageEffecterDef;
            this.weaponDamageDef = weaponDamageDef;
            this.weaponDamageMult = weaponDamageMult;
        }

        public static DamageParameters Default()
        {
            return new DamageParameters(DamageDefOf.AcidBurn, new FloatRange(1, 1), new FloatRange(1, 1), false, EffecterDefOf.Deflect_General, null, new FloatRange(1, 1));
        }

        public void DealDamageTo(Thing Attacker, Thing Target)
        {
            DamageInfo damageInfo = new DamageInfo(damageDef, damageAmount.RandomInRange, armourPenAmount.RandomInRange, -1, Attacker);

            if (Attacker is Pawn attackerPawn)
            {
                damageInfo = attackerPawn.GetAttackDamageForPawn(damageDef, 
                    damageAmount.RandomInRange, 
                    armourPenAmount.RandomInRange, 
                    weaponDamageMult.RandomInRange, 
                    useWeaponDamageIfAvailable,
                    weaponDamageDef);
            }

            if (Target.Spawned)
            {
                if (damageEffecterDef != null)
                {
                    damageEffecterDef.Spawn(Target.Position, Target.Map);
                }

                Target.TakeDamage(damageInfo);
            }
        }
    }
}

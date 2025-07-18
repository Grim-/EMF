using RimWorld;
using Verse;

namespace EMF
{
    public static class DamageScalingUtility
    {
        public static DamageInfo GetAttackDamageForPawn(this Pawn pawn, DamageDef damageDef, float damage, float armourPen = 0, float damageMultiplier = 1f, bool tryUseWeaponDamage = false, DamageDef overrideWeaponDamageDef = null)
        {
            DamageInfo outDamage = new DamageInfo();

            if (tryUseWeaponDamage && pawn.HasWeaponEquipped())
            {
                outDamage = pawn.equipment.PrimaryEq.GetWeaponDamage(pawn, damageMultiplier, armourPen);

                if (overrideWeaponDamageDef != null)
                {
                    outDamage.Def = overrideWeaponDamageDef;
                }
            }
            else
            {
                outDamage = new DamageInfo(damageDef, damage * damageMultiplier, armourPen);
            }

            return outDamage;
        }

        public static DamageInfo GetAttackDamageForPawn(this Pawn pawn, DamageDef damageDef, StatDef damage, float armourPen = 0, float damageMultiplier = 1f, bool tryUseWeaponDamage = false)
        {
            return GetAttackDamageForPawn(pawn, damageDef, pawn.GetStatValue(damage), armourPen, damageMultiplier, tryUseWeaponDamage);
        }

        public static bool IsPhysicalDamage(DamageDef damageDef)
        {
            if (damageDef == null) return false;

            return damageDef == DamageDefOf.Blunt ||
                   damageDef == DamageDefOf.Cut ||
                   damageDef == DamageDefOf.Stab ||
                   damageDef == DamageDefOf.Scratch ||
                   damageDef == DamageDefOf.Bite;
        }
    }


}

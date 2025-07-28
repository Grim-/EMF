using RimWorld;
using UnityEngine;
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

        public static float GetDamageScalingMultiplierForDamageType(Pawn pawn, DamageDef damageDef, AbilityResourceDef resourceDef)
        {
            if (pawn == null || resourceDef == null)
                return resourceDef?.baseScalingMultiplier ?? 1f;

            StatDef scalingStat = resourceDef.GetScalingStatForDamageType(damageDef);
            if (scalingStat == null)
                return resourceDef.baseScalingMultiplier;

            float statValue = pawn.GetStatValue(scalingStat);
            float scalingMultiplier = resourceDef.baseScalingMultiplier * statValue;

            return Mathf.Clamp(scalingMultiplier, resourceDef.baseScalingMultiplier * 0.1f, resourceDef.maxScalingMultiplier);
        }

        public static DamageInfo ScaleDamageInfoWithResource(DamageInfo damageInfo, Pawn casterPawn, AbilityResourceDef resourceDef)
        {
            if (casterPawn == null || resourceDef == null)
                return damageInfo;

            float scalingMultiplier = GetDamageScalingMultiplierForDamageType(casterPawn, damageInfo.Def, resourceDef);

            DamageInfo scaledDamage = damageInfo;
            scaledDamage.SetAmount(damageInfo.Amount * scalingMultiplier);

            return scaledDamage;
        }

        public static DamageInfo ScaleDamageInfoForAbility(DamageInfo damageInfo, ResourceAbility ability)
        {
            if (ability?.pawn == null)
                return damageInfo;

            return ScaleDamageInfoWithResource(damageInfo, ability.pawn, ability.ResourceDef?.resourceDef);
        }

        public static float ScaleDamageAmountForAbility(float baseDamage, DamageDef damageDef, ResourceAbility ability)
        {
            if (ability?.pawn == null || ability.ResourceDef?.resourceDef == null)
                return baseDamage;

            float scalingMultiplier = GetDamageScalingMultiplierForDamageType(ability.pawn, damageDef, ability.ResourceDef.resourceDef);
            return baseDamage * scalingMultiplier;
        }

        public static AbilityResourceDef GetResourceDefFromHediff(HediffDef hediffDef)
        {
            var extension = hediffDef.GetModExtension<ResourceAssociatedHediffExtension>();
            return extension?.associatedResourceDef;
        }

        public static AbilityResourceDef GetResourceDefFromHediff(Hediff hediff)
        {
            return GetResourceDefFromHediff(hediff.def);
        }

        public static bool ShouldScaleDamageForHediff(HediffDef hediffDef, DamageDef damageDef)
        {
            var extension = hediffDef.GetModExtension<ResourceAssociatedHediffExtension>();
            if (extension == null) return false;

            if (!extension.scaleAllDamage)
            {
                if (extension.damageTypesToScale != null && !extension.damageTypesToScale.Contains(damageDef))
                    return false;
            }

            if (extension.damageTypesToIgnore != null && extension.damageTypesToIgnore.Contains(damageDef))
                return false;

            return true;
        }

        public static DamageInfo ScaleDamageInfoForHediff(DamageInfo damageInfo, Hediff hediff, Pawn casterPawn = null)
        {
            if (hediff?.def == null)
                return damageInfo;

            var resourceDef = GetResourceDefFromHediff(hediff);
            if (resourceDef == null)
                return damageInfo;

            if (!ShouldScaleDamageForHediff(hediff.def, damageInfo.Def))
                return damageInfo;

            Pawn pawnToUse = casterPawn ?? hediff.pawn;
            if (pawnToUse == null)
                return damageInfo;

            return ScaleDamageInfoWithResource(damageInfo, pawnToUse, resourceDef);
        }

        public static float ScaleDamageAmountForHediff(float baseDamage, DamageDef damageDef, Hediff hediff, Pawn casterPawn = null)
        {
            if (hediff?.def == null)
                return baseDamage;

            var resourceDef = GetResourceDefFromHediff(hediff);
            if (resourceDef == null)
                return baseDamage;

            if (!ShouldScaleDamageForHediff(hediff.def, damageDef))
                return baseDamage;

            Pawn pawnToUse = casterPawn ?? hediff.pawn;
            if (pawnToUse == null)
                return baseDamage;

            float scalingMultiplier = GetDamageScalingMultiplierForDamageType(pawnToUse, damageDef, resourceDef);
            return baseDamage * scalingMultiplier;
        }
    }


}

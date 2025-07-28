using HarmonyLib;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    [StaticConstructorOnStartup]
    public static class EMFUtil
    {
        static EMFUtil()
        {
      
        }

        public static bool HasCooldownByTick(int LastTriggerTick, int CooldownTicks)
        {
            if (LastTriggerTick <= 0)
            {
                return false;
            }
            return Current.Game.tickManager.TicksGame <= LastTriggerTick + CooldownTicks;
        }

        public static bool HasMagicDisabled(this Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null) return false;

            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                var hediff = pawn.health.hediffSet.hediffs[i];
                if (hediff.TryGetComp<HediffComp>() is IDisableMagic magicComp && magicComp.DisablesMagic)
                {
                    return true;
                }
            }
            return false;
        }
        public static Gene_BasicResource GetGeneForResourceDef(this Pawn pawn, AbilityResourceDef resourceDef)
        {
            if (pawn?.genes?.GenesListForReading == null || resourceDef == null)
                return null;


            Gene_BasicResource foundGene = pawn.genes.GenesListForReading
                .OfType<Gene_BasicResource>()
                .FirstOrDefault(g => g.HasResource(resourceDef));
            return foundGene;
        }

        public static bool HasResourceDef(this Pawn pawn, AbilityResourceDef resourceDef, out Gene_BasicResource OwningGene)
        {
            OwningGene = null;

            if (pawn?.genes?.GenesListForReading == null || resourceDef == null)
            {
                return false;
            }

            Gene_BasicResource foundGene = pawn.genes.GenesListForReading
            .OfType<Gene_BasicResource>()
            .FirstOrDefault(g => g.HasResource(resourceDef));

            if (foundGene != null)
            {
                OwningGene = foundGene;
                return true;
            }

            return false;
        }

        public static bool HasTeleportingDisabled(this Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null)
                return false;

            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                var hediff = pawn.health.hediffSet.hediffs[i];
                if (hediff.TryGetComp<HediffComp>() is IDisableTeleportingAbilities magicComp && magicComp.DisablesTeleporting)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HasRessurectionDisabled(this Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs == null) return false;

            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                var hediff = pawn.health.hediffSet.hediffs[i];
                if (hediff.TryGetComp<HediffComp>() is IDisableRessurection resComp && resComp.DisablesRessurection)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TryExtinguishFireAt(IntVec3 cell, Map map, float extinguishAmount = 100f)
        {
            if (!cell.IsValid || map == null)
            {
                return false;
            }

            if (FireUtility.NumFiresAt(cell, map) > 0)
            {
                foreach (var item in cell.GetFiresNearCell(map).ToArray())
                {
                    item.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, extinguishAmount, 0f, -1f));
                }
                return true;
            }

            return false;
        }


        public static IntVec3 CalculatePushDirection(IntVec3 Origin, IntVec3 Position, float minPushDistance, float maxPushDistance)
        {
            float distance = Position.DistanceTo(Origin);
            float pushFactor = 1f - (distance / maxPushDistance);
            int pushDistance = Mathf.RoundToInt(minPushDistance + pushFactor * (maxPushDistance - minPushDistance));
            IntVec3 direction = (Position - Origin);
            return Position + (direction * pushDistance);
        }

        public static void TrainPawn(Pawn PawnToTrain, Pawn Trainer = null)
        {
            if (PawnToTrain.training != null)
            {
                foreach (var item in DefDatabase<TrainableDef>.AllDefsListForReading)
                {
                    PawnToTrain.training.SetWantedRecursive(item, true);
                    PawnToTrain.training.Train(item, Trainer, true);
                }


                if (PawnToTrain.playerSettings != null)
                {
                    PawnToTrain.playerSettings.followDrafted = true;
                }
            }
        }

        public static bool HasWeaponEquipped(this Pawn pawn)
        {
            return pawn.def.race.Humanlike && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.PrimaryEq != null;
        }
        public static DamageInfo GetWeaponDamage(this CompEquippable Equippable, Pawn attacker, float damageMultiplier = 1, float overrideArmourPen = -1)
        {
            DamageDef damageDef = Equippable.PrimaryVerb.GetDamageDef();

            if (Equippable.PrimaryVerb == null || Equippable.PrimaryVerb.GetDamageDef() == null)
            {
                return default(DamageInfo);
            }


            float armourPen = overrideArmourPen > 0 ? overrideArmourPen : Equippable.PrimaryVerb.verbProps.AdjustedArmorPenetration(Equippable.PrimaryVerb, attacker);

            return new DamageInfo(damageDef,
                Equippable.PrimaryVerb.verbProps.AdjustedMeleeDamageAmount(Equippable.PrimaryVerb, attacker) * Mathf.Min(1, damageMultiplier),
                armourPen,
                -1,
                attacker,
                null,
                Equippable.parent.def);
        }


        public static bool TryGetWorstInjury(Pawn pawn, out Hediff hediff, out BodyPartRecord part, Func<Hediff, bool> filter = null, params HediffDef[] exclude)
        {
            part = null;
            hediff = null;

            bool PassesFilter(Hediff h) => filter == null || filter(h);

            Hediff lifeThreateningHediff = HealthUtility.FindLifeThreateningHediff(pawn, exclude);
            if (lifeThreateningHediff != null && PassesFilter(lifeThreateningHediff))
            {
                hediff = lifeThreateningHediff;
                return true;
            }

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 2500)
            {
                Hediff bleedingHediff = HealthUtility.FindMostBleedingHediff(pawn, exclude);
                if (bleedingHediff != null && PassesFilter(bleedingHediff))
                {
                    hediff = bleedingHediff;
                    return true;
                }
            }

            if (pawn.health.hediffSet.GetBrain() != null)
            {
                var brain = pawn.health.hediffSet.GetBrain();
                Hediff brainInjury = HealthUtility.FindPermanentInjury(pawn, Gen.YieldSingle(brain), exclude);
                if (brainInjury != null && PassesFilter(brainInjury))
                {
                    hediff = brainInjury;
                    return true;
                }
                brainInjury = HealthUtility.FindInjury(pawn, Gen.YieldSingle(brain), exclude);
                if (brainInjury != null && PassesFilter(brainInjury))
                {
                    hediff = brainInjury;
                    return true;
                }
            }

            float significantCoverage = ThingDefOf.Human.race.body.GetPartsWithDef(BodyPartDefOf.Hand).First().coverageAbsWithChildren;
            part = HealthUtility.FindBiggestMissingBodyPart(pawn, significantCoverage);
            if (part != null)
            {
                return true;
            }

            Hediff eyeInjury = HealthUtility.FindPermanentInjury(
                pawn,
                from x in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                where x.def == BodyPartDefOf.Eye
                select x,
                exclude);
            if (eyeInjury != null && PassesFilter(eyeInjury))
            {
                hediff = eyeInjury;
                return true;
            }

            part = HealthUtility.FindBiggestMissingBodyPart(pawn, 0f);
            if (part != null)
            {
                hediff = pawn.health.hediffSet.GetMissingPartFor(part);
                if (PassesFilter(hediff))
                {
                    return true;
                }
            }

            Hediff permanentInjury = HealthUtility.FindPermanentInjury(pawn, null, exclude);
            if (permanentInjury != null && PassesFilter(permanentInjury))
            {
                hediff = permanentInjury;
                return true;
            }

            Hediff anyInjury = HealthUtility.FindInjury(pawn, null, exclude);
            if (anyInjury != null && PassesFilter(anyInjury))
            {
                hediff = anyInjury;
                return true;
            }

            return false;
        }
        public static void QuickHeal(this Pawn pawn, float healAmount)
        {
            if (TryGetWorstInjury(pawn, out Hediff hediff, out BodyPartRecord part, null))
            {
                if (hediff == null || hediff.def == null)
                {
                    return;
                }

                HealthUtility.AdjustSeverity(pawn, hediff.def, -healAmount);
            }
        }
        public static bool HasMissingBodyParts(Pawn Target)
        {
            return Target.health.hediffSet.hediffs
                .OfType<Hediff_MissingPart>().Count() > 0;
        }

        public static List<Hediff> GetMostSevereHealthProblems(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                  .Where(x => !(x is Hediff_MissingPart) && x.Visible && x.def.isBad && !x.def.chronic)
                .OrderByDescending(x => x.Severity)
                .ToList();
        }
        public static List<Hediff_MissingPart> GetMissingPartsPrioritized(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_MissingPart>()
                .OrderByDescending(x => x.Part.def.hitPoints)
                .ThenByDescending(x => x.Part.def.GetMaxHealth(pawn))
                .ToList();
        }

        public static bool RestoreMissingPart(Pawn target)
        {
            List<Hediff_MissingPart> missingParts = GetMissingPartsPrioritized(target);
            if (missingParts.Count > 0)
            {
                Hediff_MissingPart highestPrio = missingParts.First();
                HealthUtility.Cure(highestPrio);
                return true;
            }
            return false;
        }
        public static Hediff_MissingPart GetMostPrioritizedMissingPartFromList(Pawn pawn, List<Hediff_MissingPart> List)
        {
            return List
               .OrderByDescending(x => x.Part.def.hitPoints)
               .ThenByDescending(x => x.Part.def.GetMaxHealth(pawn))
               .FirstOrDefault();
        }
        public static Hediff_MissingPart GetMostPrioritizedMissingPart(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
               .OfType<Hediff_MissingPart>()
               .OrderByDescending(x => x.Part.def.hitPoints)
               .ThenByDescending(x => x.Part.def.GetMaxHealth(pawn))
               .FirstOrDefault();
        }

        public static float SpendHealingAmount(this Pawn pawn, float totalHealAmount, HealParameters healParams)
        {
            return pawn.SpendHealingAmount(totalHealAmount, healParams.CreateFilter());
        }

        public static bool NeedsHealing(this Pawn pawn)
        {
            return pawn.health.summaryHealth.SummaryHealthPercent < 1;
        }
        public static float SpendHealingAmount(this Pawn pawn, float totalHealAmount, Func<Hediff, bool> filter = null)
        {
            if (totalHealAmount <= 0)
            {
                Log.Error($"No healing amount provided for SpendHealingAmount");
                return 0;
            }

            float remainingHeal = totalHealAmount;
            float totalHealed = 0f;



            while (remainingHeal > 0.01f)
            {
                if (EMFUtil.TryGetWorstInjury(pawn, out Hediff hediff, out BodyPartRecord part, filter))
                {
                    if (hediff == null && part == null)
                    {
                        break;
                    }


                    Hediff toHeal = hediff;

                    if (part != null)
                    {
                        toHeal = pawn.health.hediffSet.GetMissingPartFor(part);
                    }


                    if (toHeal != null)
                    {
                        float healToApply = Mathf.Min(remainingHeal, toHeal.Severity);
                        toHeal.Severity -= healToApply;
                        remainingHeal -= healToApply;
                        totalHealed += healToApply;

                        if (toHeal.Severity <= 0)
                        {
                            HealthUtility.Cure(toHeal);
                        }
                    }
                    else break;
                }
                else
                {
                    break;
                }
            }

            return totalHealed;
        }
        public static void DropAndEquip(this Pawn_EquipmentTracker equipment, ThingWithComps newEquipment)
        {
            if (equipment?.pawn == null || newEquipment == null)
                return;

            var existingEquipment = equipment.AllEquipmentListForReading
                .FirstOrDefault(x => x.def.equipmentType == newEquipment.def.equipmentType);

            if (existingEquipment != null)
            {
                equipment.Remove(existingEquipment);
                GenPlace.TryPlaceThing(existingEquipment, equipment.pawn.Position, equipment.pawn.Map, ThingPlaceMode.Near);
            }

            equipment.AddEquipment(newEquipment);
        }
    }
}

using HarmonyLib;
using LudeonTK;
using RimWorld;
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

        public static IEnumerable<HediffComp_SelectiveDamageImmunity> GetSelectiveDamageImmunityComps(this Pawn pawn)
        {
            return pawn.health.hediffSet.GetAllComps()
                .OfType<HediffComp_SelectiveDamageImmunity>();
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
            if (Trainer != null && PawnToTrain.Faction != Trainer.Faction)
            {
                PawnToTrain.SetFaction(Trainer.Faction);
            }

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
                    PawnToTrain.playerSettings.followFieldwork = true;

                    if (Trainer != null)
                    {
                        PawnToTrain.playerSettings.Master = Trainer;

                        if (!PawnToTrain.relations.DirectRelationExists(PawnRelationDefOf.Bond, Trainer))
                        {
                            PawnToTrain.relations.AddDirectRelation(PawnRelationDefOf.Bond, Trainer);
                        }       
                    }

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

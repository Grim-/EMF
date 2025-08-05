using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EMF
{
    [StaticConstructorOnStartup]
    public static class EMFPatches
    {
        public static string packageID = "com.emo.magicframework";
        static EMFPatches()
        {
            Harmony harmony = new Harmony(packageID);
            harmony.PatchAll();
        }



        [HarmonyPatch(typeof(ZoneManager))]
        [HarmonyPatch("Notify_NoZoneOverlapThingSpawned")]
        public static class Patch_ZoneManager_Notify_NoZoneOverlapThingSpawned
        {
            [HarmonyPrefix]
            public static bool Prefix(ZoneManager __instance, Thing thing, ref Zone[] ___zoneGrid)
            {
                CellRect cellRect = thing.OccupiedRect();
                for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
                {
                    for (int j = cellRect.minX; j <= cellRect.maxX; j++)
                    {
                        IntVec3 c = new IntVec3(j, 0, i);
                        Zone zone = __instance.ZoneAt(c);
                        if (zone != null && !(zone is Zone_AreaCapture))
                        {
                            zone.RemoveCell(c);
                            zone.CheckContiguous();
                        }
                    }
                }
                return false;
            }
        }


        [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
        public class EquipmentTracker_GetGizmos_Patch
        {
            static void Postfix(Pawn_EquipmentTracker __instance, ref IEnumerable<Gizmo> __result)
            {
                var originalGizmos = __result.ToList();

                var additionalGizmos = new List<Gizmo>();
                foreach (var eq in __instance.AllEquipmentListForReading)
                {
                    if (eq is IDrawEquippedGizmos equippedGizmos)
                    {
                        additionalGizmos.AddRange(equippedGizmos.GetEquippedGizmos());
                    }

                    foreach (var item in eq.AllComps)
                    {
                        if (item is IDrawEquippedGizmos compEquippedGizmos)
                        {
                            additionalGizmos.AddRange(compEquippedGizmos.GetEquippedGizmos());
                        }
                    }

                }

                __result = originalGizmos.Concat(additionalGizmos);
            }
        }



        [HarmonyPatch(typeof(Pawn_EquipmentTracker))]
        [HarmonyPatch("TryDropEquipment")]
        public static class Patch_TryDropEquipment
        {
            [HarmonyPrefix]
            public static bool Prefix(ThingWithComps eq)
            {
                var lockComp = eq.GetComp<Comp_CursedEquipment>();
                if (lockComp != null && lockComp.IsSlotLocked)
                {
                    Messages.Message($"Cannot remove {eq.Label}: it is locked to the equipment slot.",
                        MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Ability), nameof(Ability.GizmoDisabled))]
        public static class Ability_GizmoDisabled_Patch
        {
            public static bool Prefix(Ability __instance, out string reason, ref bool __result)
            {
                if (__instance.pawn != null && __instance.pawn.HasMagicDisabled())
                {
                    reason = "MagicDisabled".Translate();
                    __result = true;
                    return false;
                }
                reason = null;
                return true;
            }
        }

        [HarmonyPatch(typeof(ResurrectionUtility), nameof(ResurrectionUtility.TryResurrect))]
        public static class ResurrectionUtility_TryResurrect_Patch
        {
            public static bool Prefix(Pawn pawn, ref bool __result)
            {
                if (Prefs.DevMode)
                {
                    return true;
                }

                if (pawn != null && pawn.HasRessurectionDisabled())
                {
                    if (Find.CurrentMap != null && Current.ProgramState == ProgramState.Playing)
                    {
                        __result = false;
                        Messages.Message($"{pawn.LabelShort} cannot be ressurrected!", MessageTypeDefOf.NegativeEvent);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ResurrectionUtility), nameof(ResurrectionUtility.TryResurrectWithSideEffects))]
        public static class ResurrectionUtility_TryResurrectWithSideEffects_Patch
        {
            public static bool Prefix(Pawn pawn, ref bool __result)
            {
                if (pawn != null && pawn.HasRessurectionDisabled())
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

    }
}

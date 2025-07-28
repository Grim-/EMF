using HarmonyLib;
using RimWorld;
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

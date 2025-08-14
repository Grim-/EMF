using HarmonyLib;
using RimWorld;
using Verse;

namespace EMF
{
    public static class TransformationPatches
    {

        //[HarmonyPatch(typeof(Pawn_DraftController), "ShowDraftGizmo", MethodType.Getter)]
        //public static class Patch_Pawn_DraftController
        //{
        //    public static bool Prefix(Pawn_DraftController __instance, Pawn ___pawn, ref bool __result)
        //    {
        //        if (___pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(___pawn, out Pawn original))
        //        {
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(AutoUndrafter), "ShouldAutoUndraft")]
        //public static class Patch_AutoUndrafter
        //{
        //    public static bool Prefix(AutoUndrafter __instance, Pawn ___pawn, ref bool __result)
        //    {
        //        if (___pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(___pawn, out Pawn original))
        //        {
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}
        //[HarmonyPatch(typeof(Designator_Slaughter), "CanDesignateThing")]
        //public static class Patch_Designator_Slaughter
        //{
        //    public static bool Prefix(Designator_Slaughter __instance, Thing t, ref AcceptanceReport __result)
        //    {
        //        if (t is Pawn pawn)
        //        {
        //            if (pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original))
        //            {
        //                __result = AcceptanceReport.WasRejected;
        //                return false;
        //            }
        //        }

        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(Designator_ReleaseAnimalToWild), "CanDesignateThing")]
        //public static class Patch_Designator_ReleaseAnimalToWild
        //{
        //    public static bool Prefix(Designator_ReleaseAnimalToWild __instance, Thing t, ref AcceptanceReport __result)
        //    {
        //        if (t is Pawn pawn)
        //        {
        //            if (pawn.Faction == Faction.OfPlayer && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original))
        //            {
        //                __result = AcceptanceReport.WasRejected;
        //                return false;
        //            }
        //        }

        //        return true;
        //    }
        //}

    }
}

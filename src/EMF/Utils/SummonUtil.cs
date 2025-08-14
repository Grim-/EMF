using RimWorld;
using System.Collections.Generic;
using Verse;

namespace EMF
{
    public static class SummonUtil
    {
        public static Pawn SpawnSummonFor(Pawn Summon, Pawn Summoner, Map Map, IntVec3 Position, bool autoTrain = true, bool enableDrafting = true, HediffDef customSummonHediff = null)
        {
            if (Summon == null || Map == null)
            {
                return null;
            }

            //if (Summon.abilities == null)
            //{
            //    Summon.abilities = new Pawn_AbilityTracker(Summon);
            //}

            HediffWithComps hediff = (HediffWithComps)Summon.health.GetOrAddHediff(customSummonHediff != null ? customSummonHediff : EMFDefOf.EMF_SummonedPawn);

            if (hediff.TryGetComp(out HediffComp_Summon summonComp))
            {
                summonComp.Summoner = Summoner;
            }
            else
            {
                Log.Error($"EMF Summon Util : Summon Hediff must have a HediffComp_Summon");
            }

            if (autoTrain)
                EMFUtil.TrainPawn(Summon, Summoner);

            if (enableDrafting)
                DraftingUtility.MakeDraftable(Summon);

            GenSpawn.Spawn(Summon, Position, Map);
            return Summon;
        }
        public static Pawn SpawnSummonFor(PawnKindDef pawnKindDef, Pawn Summoner, Map Map, IntVec3 Position, bool autoTrain = true, bool enableDrafting = true, HediffDef customSummonHediff = null)
        {
            if (pawnKindDef == null || Map == null)
            {
                return null;
            }

            Pawn newSummonPawn = PawnGenerator.GeneratePawn(pawnKindDef, Summoner.Faction);

            //if (newSummonPawn.abilities == null)
            //{
            //    newSummonPawn.abilities = new Pawn_AbilityTracker(newSummonPawn);
            //}

            HediffWithComps hediff = (HediffWithComps)newSummonPawn.health.GetOrAddHediff(customSummonHediff != null ? customSummonHediff : EMFDefOf.EMF_SummonedPawn);

            if (hediff.TryGetComp(out HediffComp_Summon summonComp))
            {
                summonComp.Summoner = Summoner;
            }
            else
            {
                Log.Error($"EMF Summon Util : Summon Hediff must have a HediffComp_Summon");
            }

            if (enableDrafting)
                DraftingUtility.MakeDraftable(newSummonPawn);

            if (autoTrain)
                EMFUtil.TrainPawn(newSummonPawn, Summoner);

            GenSpawn.Spawn(newSummonPawn, Position, Map);

            return newSummonPawn;
        }


        public static bool IsASummon(this Pawn pawn, out HediffComp_Summon summonComp)
        {
            summonComp = pawn.health.hediffSet.GetHediffComps<HediffComp_Summon>().FirstOrFallback();
            return summonComp != null;
        }

        public static Pawn GetSummonerFor(this Pawn pawn)
        {
            if (IsASummon(pawn, out HediffComp_Summon summonComp))
            {
                return summonComp.Summoner;
            }

            return null;
        }
    }
}
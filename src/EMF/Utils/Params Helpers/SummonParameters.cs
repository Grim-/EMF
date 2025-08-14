using System.Collections.Generic;
using Verse;

namespace EMF
{
    public class SummonParameters
    {
        public IntRange spawnCount = new IntRange(1, 1);
        public PawnKindDef pawnKind;
        public HediffDef customSummonHediff;
        public bool autoTrain = true;
        public bool enableDrafting = true;

        public List<Pawn> Spawn(IntVec3 spawnOrigin, Map map, Pawn Summoner)
        {
            List<Pawn> summonsCreated = new List<Pawn>();

            for (int i = 0; i < spawnCount.RandomInRange; i++)
            {
               Pawn CurrentSummon = SummonUtil.SpawnSummonFor(
                    pawnKind,
                    Summoner,
                    map,
                    spawnOrigin,
                    autoTrain,
                    enableDrafting,
                    customSummonHediff
                );

                if (CurrentSummon != null)
                {
                    summonsCreated.Add(CurrentSummon);
                }
            }

            return summonsCreated;
        }
    }
}

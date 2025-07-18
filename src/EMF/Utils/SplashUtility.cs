using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public static class SplashUtility
    {
        public static int SplashThingsAround(IntVec3 center, Map map, float radius, ThingDef thingDef,
            ThingDef stuffDef = null, int maxSpawnCount = -1, float areaCoverage = 1f,
            bool onlyOnPassableTerrain = true, bool requiresLineOfSight = false,
            bool avoidPawns = false, bool avoidBuildings = false)
        {
            if (map == null || thingDef == null)
                return 0;

            int spawnedCount = 0;
            List<IntVec3> candidateCells = new List<IntVec3>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!cell.InBounds(map))
                    continue;

                if (onlyOnPassableTerrain && !cell.Standable(map))
                    continue;

                if (requiresLineOfSight && !GenSight.LineOfSight(center, cell, map))
                    continue;

                if (avoidPawns && cell.GetFirstPawn(map) != null)
                    continue;

                if (avoidBuildings && cell.GetFirstBuilding(map) != null)
                    continue;

                candidateCells.Add(cell);
            }

            candidateCells.Shuffle();

            int targetSpawnCount = maxSpawnCount > 0 ?
                Mathf.Min(maxSpawnCount, Mathf.RoundToInt(candidateCells.Count * areaCoverage)) :
                Mathf.RoundToInt(candidateCells.Count * areaCoverage);

            for (int i = 0; i < targetSpawnCount && i < candidateCells.Count; i++)
            {
                IntVec3 spawnCell = candidateCells[i];

                Thing thing = ThingMaker.MakeThing(thingDef, stuffDef);
                if (thing != null)
                {
                    GenSpawn.Spawn(thing, spawnCell, map);
                    spawnedCount++;
                }
            }

            return spawnedCount;
        }

        public static int SplashDamageAround(IntVec3 center, Map map, float radius, DamageDef damageDef,
            FloatRange damageAmount, Faction sourceFaction, FriendlyFireSettings targetSettings,
            float areaCoverage = 1f, bool requiresLineOfSight = true, Thing instigator = null)
        {
            if (map == null || damageDef == null)
                return 0;

            int damagedCount = 0;
            List<IntVec3> candidateCells = new List<IntVec3>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!cell.InBounds(map))
                    continue;

                if (requiresLineOfSight && !GenSight.LineOfSight(center, cell, map))
                    continue;

                candidateCells.Add(cell);
            }

            int targetCellCount = Mathf.RoundToInt(candidateCells.Count * areaCoverage);
            candidateCells.Shuffle();

            for (int i = 0; i < targetCellCount && i < candidateCells.Count; i++)
            {
                IntVec3 damageCell = candidateCells[i];

                foreach (Thing thing in damageCell.GetThingList(map))
                {
                    if (thing is Pawn pawn && pawn.CanTargetThing(sourceFaction, targetSettings))
                    {
                        DamageInfo damageInfo = new DamageInfo(damageDef, damageAmount.RandomInRange, 0f, -1f, instigator);
                        pawn.TakeDamage(damageInfo);
                        damagedCount++;
                    }
                }
            }

            return damagedCount;
        }

        public static int SplashHediffAround(IntVec3 center, Map map, float radius, HediffDef hediffDef,
            Faction sourceFaction, FriendlyFireSettings targetSettings, float areaCoverage = 1f,
            bool requiresLineOfSight = true, BodyPartDef targetBodyPart = null)
        {
            if (map == null || hediffDef == null)
                return 0;

            int affectedCount = 0;
            List<IntVec3> candidateCells = new List<IntVec3>();

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, radius, true))
            {
                if (!cell.InBounds(map))
                    continue;

                if (requiresLineOfSight && !GenSight.LineOfSight(center, cell, map))
                    continue;

                Pawn pawn = cell.GetFirstPawn(map);
                if (pawn != null && pawn.CanTargetThing(sourceFaction, targetSettings))
                {
                    candidateCells.Add(cell);
                }
            }

            int targetCellCount = Mathf.RoundToInt(candidateCells.Count * areaCoverage);
            candidateCells.Shuffle();

            for (int i = 0; i < targetCellCount && i < candidateCells.Count; i++)
            {
                IntVec3 effectCell = candidateCells[i];
                Pawn pawn = effectCell.GetFirstPawn(map);

                if (pawn != null)
                {
                    BodyPartRecord bodyPart = targetBodyPart != null ?
                        pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def == targetBodyPart) :
                        null;

                    pawn.health.AddHediff(hediffDef, bodyPart);
                    affectedCount++;
                }
            }

            return affectedCount;
        }
    }
}

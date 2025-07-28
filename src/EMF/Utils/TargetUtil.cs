using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public static class TargetUtil
    {
        public static bool HasAbility(this Pawn pawn, AbilityDef AbiityDef)
        {
            if (pawn?.abilities == null)
            {
                return false;
            }

            return pawn.abilities.AllAbilitiesForReading.Find(x => x.def == AbiityDef) != null;
        }
        public static void QuickHealInRadius(float healAmount, IntVec3 Position, Map map, float radius, Faction Faction, FriendlyFireSettings friendlyFireSettings, bool useCenter = true)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (pawn.CanTargetThing(Faction, friendlyFireSettings))
                    {
                        pawn.SpendHealingAmount(healAmount);
                    }
                }
            }
        }




        public static List<Hediff> ApplyHediffInRadius(HediffDef hediffDef, IntVec3 Position, Map map, float radius, Faction Faction, FriendlyFireSettings friendlyFireSettings, bool useCenter = true)
        {
            List<Hediff> hediffs = new List<Hediff>();
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (pawn.CanTargetThing(Faction, friendlyFireSettings))
                    {
                        hediffs.Add(pawn.health.GetOrAddHediff(hediffDef));
                    }
                }
            }

            return hediffs;
        }

        public static void ApplyHediffSeverityInRadius(HediffDef hediffDef, IntVec3 Position, Map map, float radius, Faction Faction, float SeverityChange, FriendlyFireSettings friendlyFireSettings, bool useCenter = true)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (pawn.CanTargetThing(Faction, friendlyFireSettings))
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            if (pawn.health.hediffSet.TryGetHediff(hediffDef, out Hediff hediff))
                            {
                                hediff.Severity += SeverityChange;
                            }
                        }
                    }
                }
            }
        }


        public static void ApplyDamageInRadius(DamageDef damageDef, float damageAmount, float armourPenArmount, IntVec3 Position, Map map, float radius, Faction Faction, FriendlyFireSettings friendlyFireSettings, bool useCenter = true, Thing instigator = null)
        {
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item.CanTargetThing(Faction, friendlyFireSettings))
                {
                    item.TakeDamage(new DamageInfo(damageDef, damageAmount, armourPenArmount, -1, instigator));
                }
            }
        }

        public static List<Pawn> GetPawnsInRadius(IntVec3 Position, Map map, float radius, Faction Faction, FriendlyFireSettings friendlyFireSettings, bool useCenter = true)
        {
            List<Pawn> pawns = new List<Pawn>();
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, map, radius, useCenter))
            {
                if (item is Pawn pawn)
                {
                    if (pawn.CanTargetThing(Faction, friendlyFireSettings))
                    {
                        pawns.Add(pawn);
                    }
                }
            }

            return pawns;
        }

        public static bool CanTargetThing(this Thing thing, Faction sourceFaction, FriendlyFireSettings validTargetParams)
        {
            if (thing.Faction == null)
                return validTargetParams.canTargetNeutral;

            if (thing.Faction == sourceFaction && validTargetParams.canTargetFriendly)
                return true;

            if (validTargetParams.canTargetHostile && thing.Faction.HostileTo(sourceFaction))
                return true;

            if (validTargetParams.canTargetNeutral && !thing.Faction.HostileTo(sourceFaction) && thing.Faction != sourceFaction)
                return true;

            return false;
        }


        public static List<IntVec3> GetAllCellsInRect(IntVec3 Origin, IntVec3 Target, int width, int height)
        {
            List<IntVec3> result = new List<IntVec3>();

            IntVec3 diff = Target - Origin;
            float distance = diff.LengthHorizontal;

            if (distance < 0.001f)
            {
                diff = new IntVec3(0, 0, 1);
                distance = 1f;
            }

            Vector3 dirNormalized = new Vector3(diff.x, 0, diff.z).normalized;

            Vector3 perpendicular = new Vector3(-dirNormalized.z, 0, dirNormalized.x);

            Vector3 halfWidthPerp = perpendicular * (width / 2f);
            Vector3 originV3 = Origin.ToVector3Shifted();

            int boundingRadius = Mathf.CeilToInt(Mathf.Sqrt(height * height + width * width) / 2f);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Origin, boundingRadius, true))
            {
                Vector3 cellVec = cell.ToVector3Shifted() - originV3;

                float alongLine = Vector3.Dot(cellVec, dirNormalized);
                float perpLine = Vector3.Dot(cellVec, perpendicular);

                if (alongLine >= 0 && alongLine <= height &&
                    perpLine >= -width / 2f && perpLine <= width / 2f)
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        public static List<IntVec3> GetCellsInCone(IntVec3 Origin, IntVec3 Target, int length, float angle, bool includeOrigin = false)
        {
            List<IntVec3> result = new List<IntVec3>();
            IntVec3 diff = Target - Origin;
            Vector3 direction;
            if (diff.x == 0 && diff.z == 0)
            {
                direction = Vector3.forward;
            }
            else
            {
                direction = new Vector3(diff.x, 0, diff.z).normalized;
            }
            float cosHalfAngle = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(Origin, length, true))
            {
                if (cell == Origin)
                {
                    if (includeOrigin)
                    {
                        result.Add(cell);
                    }
                    continue;
                }
                Vector3 toCellVec = (cell.ToVector3Shifted() - Origin.ToVector3Shifted()).normalized;
                float dot = Vector3.Dot(direction, toCellVec);
                if (dot >= cosHalfAngle)
                {
                    if ((cell - Origin).LengthHorizontalSquared <= length * length)
                    {
                        result.Add(cell);
                    }
                }
            }
            return result;
        }


        public static HashSet<Thing> GetThingsInCells(List<IntVec3> Cells, Map map, Func<Thing, bool> filter = null)
        {
            HashSet<Thing> things = new HashSet<Thing>();

            foreach (var item in Cells)
            {
                things.AddRange(item.GetThingList(map).Where(x => filter?.Invoke(x) == true));

                Pawn pawn = item.GetFirstPawn(map);
                if (pawn != null && (filter != null && filter?.Invoke(pawn) == true))
                {
                    things.Add(pawn);
                }
            }

            return things;
        }
        public static List<Thing> GetDamageableThingsInCells(List<IntVec3> Cells, Map map, Func<Thing, bool> filter = null)
        {
            List<Thing> things = new List<Thing>();

            foreach (var item in Cells)
            {
                things.AddRange(item.GetThingList(map).Where(x => filter?.Invoke(x) == true));
            }

            return things;
        }

        public static List<Thing> GetDamageableThingsInCells(List<IntVec3> Cells, Map map, Faction faction, FriendlyFireSettings friendlyFireSettings)
        {
            List<Thing> things = new List<Thing>();

            foreach (var item in Cells)
            {
                things.AddRange(item.GetThingList(map).Where(x => x.CanTargetThing(faction, friendlyFireSettings)));
            }

            return things;
        }

        public static bool GetDamageablePawn(this IntVec3 cell, Map map, Faction faction, FriendlyFireSettings friendlyFireSettings, out Pawn pawn)
        {
            pawn = cell.GetFirstPawn(map);

            if (pawn != null && pawn.CanTargetThing(faction, friendlyFireSettings))
            {
                return true;
            }

            return false;
        }

        public static HashSet<Pawn> GetPawnsInCells(this List<IntVec3> Cells, Map map)
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();
            foreach (var item in Cells)
            {
                Pawn pawn = item.GetFirstPawn(map);
                if (pawn != null)
                {
                    pawns.Add(pawn);
                }
            }
            return pawns;
        }

        public static HashSet<Pawn> GetPawnsInCells(this List<IntVec3> Cells, Map map, Faction faction, FriendlyFireSettings friendlyFireSettings)
        {
            HashSet<Pawn> pawns = new HashSet<Pawn>();
            foreach (var item in Cells)
            {
                Pawn pawn = item.GetFirstPawn(map);
                if (pawn != null && pawn.CanTargetThing(faction, friendlyFireSettings))
                {
                    pawns.Add(pawn);
                }
            }
            return pawns;
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EMF
{
    [StaticConstructorOnStartup]
    public static class LightningStrike
    {

        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);

        public static void GenerateLightningStrike(Map map, IntVec3 Position, float Radius, out IEnumerable<IntVec3> affectedCells, int Damage = 0, float ArmourPen = 1f, DamageDef OverrideDamage = null, SoundDef OverrideSoundToPlay = null, int repeatVisualCount = 4)
        {
            affectedCells = null;

            if (Position.InBounds(map))
            {
                if (!Position.IsValid)
                {
                    Position = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
                }

                if (!Position.Fogged(map))
                {
                    affectedCells = GenRadial.RadialCellsAround(Position, Radius, true);
                    Vector3 loc = Position.ToVector3Shifted();
                    for (int x = 0; x < repeatVisualCount; x++)
                    {
                        FleckMaker.ThrowSmoke(loc, map, 1.5f);
                        FleckMaker.ThrowMicroSparks(loc, map);
                        FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                    }
                }

                SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, map, false), MaintenanceType.None);
                SoundDefOf.Thunder_OnMap.PlayOneShot(info);

                Graphics.DrawMesh(LightningBoltMeshPool.RandomBoltMesh, Position.ToVector3Shifted(),
                    Quaternion.identity, LightningMat, 0);
            }
        }

        public static void GenerateLightningStrike(Map map, IntVec3 Position, float Radius, Action<IntVec3, Map> ForCellAction, int repeatVisualCount = 4)
        {
            if (Position.InBounds(map))
            {
                if (!Position.IsValid)
                {
                    Position = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
                }

                if (!Position.Fogged(map))
                {
                    foreach (var item in GenRadial.RadialCellsAround(Position, Radius, true))
                    {
                        ForCellAction?.Invoke(Position, map);
                    }

                    Vector3 loc = Position.ToVector3Shifted();
                    for (int x = 0; x < repeatVisualCount; x++)
                    {
                        FleckMaker.ThrowSmoke(loc, map, 1.5f);
                        FleckMaker.ThrowMicroSparks(loc, map);
                        FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                    }
                    GenerateLightningStrikeVisual(map, Position, repeatVisualCount);
                }
            }
        }

        public static void GenerateLightningStrikeVisual(Map map, IntVec3 Position, int repeatVisualCount = 4)
        {
            if (Position.InBounds(map))
            {
                if (Position.IsValid)
                {
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(Position, map, false), MaintenanceType.None);
                    SoundDefOf.Thunder_OnMap.PlayOneShot(info);

                    Vector3 loc = Position.ToVector3Shifted();
                    for (int x = 0; x < repeatVisualCount; x++)
                    {
                        FleckMaker.ThrowSmoke(loc, map, 1.5f);
                        FleckMaker.ThrowMicroSparks(loc, map);
                        FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                    }
                    Graphics.DrawMesh(LightningBoltMeshPool.RandomBoltMesh, Position.ToVector3Shifted(),
                        Quaternion.identity, LightningMat, 0);
                }
            }
        }
    }
}

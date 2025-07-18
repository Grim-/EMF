using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CompProperties_DomainZone : CompProperties
    {
        public TerrainDef FloorDef;
        public ThingDef WallDef;
        public ThingDef WallStuffDef;
        public ThingDef FilthDef;
        public bool ConstructWalls = true;
        public bool ReplaceFloor = true;
        public bool CreateFilth = true;

        public List<ThingSpawnCoverage> thingsToSpawn;

        public CompProperties_DomainZone()
        {
            this.compClass = typeof(DomainZoneComp);
        }
    }


    public class ThingSpawnCoverage
    {
        public ThingDef thingDef;
        public float coverage = 1;
        public int maxCount = -1;
        public bool removeOnDestroy = false;
    }

    public class DomainZoneComp : ActiveZoneComp
    {
        public CompProperties_DomainZone Props => (CompProperties_DomainZone)this.props;

        protected HashSet<IntVec3> wallCells;
        protected List<Thing> constructedWalls;
        protected List<Thing> spawnedThings;
        private HashSet<IntVec3> AddedFilth = new HashSet<IntVec3>();

        private Dictionary<IntVec3, TerrainDef> originalTerrain = new Dictionary<IntVec3, TerrainDef>();
        private Dictionary<IntVec3, List<Thing>> originalThings = new Dictionary<IntVec3, List<Thing>>();
        private Dictionary<IntVec3, List<Thing>> originalWallCellContents = new Dictionary<IntVec3, List<Thing>>();
        private bool terrainChanged = false;
        private bool wallsConstructed = false;

        private ThingCategory AllowedForSavingThingCategories = ThingCategory.Item;


        private bool ShouldConstructWalls => Props.WallDef != null && Props.ConstructWalls;
        private bool ShouldReplaceTerrain => Props.FloorDef != null && Props.ReplaceFloor;
        private bool ShouldSpawnFilth => Props.FilthDef != null && Props.CreateFilth;

        public override void OnZoneSpawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return;
            }

            CalculateWallCells(cells);

            if (ShouldConstructWalls)
            {
                ConstructWalls(cells);
            }

            if (ShouldReplaceTerrain)
            {
                ChangeTerrain(cells);
            }

            if (ShouldSpawnFilth)
            {
                CreateFilth(cells);
            }
        }

        public override void OnZoneDespawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            RemoveWalls();
            RemoveFilth();
            RevertTerrain();
        }

        protected virtual void CalculateWallCells(List<IntVec3> zoneCells)
        {
            if (ParentZone.Map == null || zoneCells == null || zoneCells.Count == 0)
            {
                return;
            }

            wallCells = new HashSet<IntVec3>();
            HashSet<IntVec3> zoneCellsSet = new HashSet<IntVec3>(zoneCells);

            foreach (IntVec3 cell in zoneCells)
            {
                bool isEdge = false;
                foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
                {
                    if (!zoneCellsSet.Contains(neighbor))
                    {
                        isEdge = true;
                        break;
                    }
                }

                if (isEdge && cell.InBounds(ParentZone.Map))
                {
                    wallCells.Add(cell);
                }
            }
        }

        private void ChangeTerrain(List<IntVec3> zoneCells)
        {
            if (Props.FloorDef != null)
            {
                foreach (IntVec3 cell in zoneCells)
                {
                    if (cell.InBounds(ParentZone.Map))
                    {
                        TerrainDef currentTerrain = cell.GetTerrain(ParentZone.Map);
                        if (!originalTerrain.ContainsKey(cell))
                        {
                            originalTerrain[cell] = currentTerrain;
                        }

                        StoreThingsForCell(cell);
                        ParentZone.Map.terrainGrid.SetTerrain(cell, Props.FloorDef);
                    }
                }
                terrainChanged = true;
            }
        }

        private void RevertTerrain()
        {
            if (terrainChanged)
            {
                foreach (var kvp in originalTerrain)
                {
                    IntVec3 cell = kvp.Key;
                    TerrainDef terrain = kvp.Value;

                    if (cell.InBounds(ParentZone.Map))
                    {
                        ParentZone.Map.terrainGrid.SetTerrain(cell, terrain);

                        if (originalThings.TryGetValue(cell, out List<Thing> cellThings))
                        {
                            foreach (Thing thing in cellThings)
                            {
                                if (!thing.Spawned)
                                {
                                    GenSpawn.Spawn(thing, cell, ParentZone.Map);
                                }
                            }
                        }
                    }
                }
                originalTerrain.Clear();
                originalThings.Clear();
                terrainChanged = false;
            }
        }

        private void CreateFilth(List<IntVec3> zoneCells)
        {
            if (Props.FilthDef != null)
            {
                AddedFilth.Clear();
                foreach (IntVec3 cell in zoneCells)
                {
                    if (FilthMaker.CanMakeFilth(cell, ParentZone.Map, Props.FilthDef))
                    {
                        if (FilthMaker.TryMakeFilth(cell, ParentZone.Map, Props.FilthDef,1 , FilthSourceFlags.None))
                        {
                            AddedFilth.Add(cell);
                        }
                    }
                }
            }
        }

        private void RemoveFilth()
        {
            foreach (IntVec3 cell in AddedFilth)
            {
                FilthMaker.RemoveAllFilth(cell, ParentZone.Map);
            }
            AddedFilth.Clear();
        }

        public virtual void ConstructWalls(List<IntVec3> zoneCells)
        {
            if (Props.WallDef != null)
            {
                CalculateWallCells(zoneCells);
                constructedWalls = new List<Thing>();

                foreach (IntVec3 cell in wallCells)
                {
                    if (cell.InBounds(ParentZone.Map) && cell != ParentZone.Position)
                    {
                        StoreThingsForCell(cell);

                        Thing wall = ThingMaker.MakeThing(Props.WallDef, Props.WallStuffDef != null ? Props.WallStuffDef : GenStuff.DefaultStuffFor(Props.WallDef));
                        if (wall != null)
                        {
                            Thing spawnedWall = GenSpawn.Spawn(wall, cell, ParentZone.Map);
                            if (spawnedWall != null)
                            {
                                constructedWalls.Add(spawnedWall);
                            }
                        }
                    }
                }
                wallsConstructed = true;
            }
        }

        private void StoreThingsForCell(IntVec3 cell)
        {
            List<Thing> cellContents = cell.GetThingList(ParentZone.Map)
               .Where(t => CanBeSavedByZone(t))
               .ToList();
            if (cellContents.Any())
            {
                originalWallCellContents[cell] = cellContents;
                foreach (Thing thing in cellContents)
                {
                    if (thing.Spawned)
                    {
                        thing.DeSpawn();
                    }
                }
            }
        }

        private bool CanBeSavedByZone(Thing thing)
        {
            return AllowedForSavingThingCategories.HasFlag(thing.def.category) && CanEverBeSavedByZone(thing);
        }

        private bool CanEverBeSavedByZone(Thing thing)
        {
            return thing != ParentZone && !thing.GetType().IsAssignableFrom(typeof(ActiveZone));
        }

        public virtual void RemoveWalls()
        {
            if (wallsConstructed)
            {
                foreach (Thing wall in constructedWalls)
                {
                    if (!wall.Destroyed)
                    {
                        wall.Destroy();
                    }
                }
                constructedWalls.Clear();

                foreach (var kvp in originalWallCellContents)
                {
                    IntVec3 cell = kvp.Key;
                    List<Thing> cellContents = kvp.Value;

                    if (cell.InBounds(ParentZone.Map))
                    {
                        foreach (Thing thing in cellContents)
                        {
                            if (!thing.Spawned)
                            {
                                GenSpawn.Spawn(thing, cell, ParentZone.Map);
                            }
                        }
                    }
                }
                originalWallCellContents.Clear();
                wallsConstructed = false;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref constructedWalls, "constructedWalls", LookMode.Reference);
            Scribe_Collections.Look(ref originalTerrain, "originalTerrain", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref originalWallCellContents, "originalWallCellContents", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref wallsConstructed, "wallsConstructed", false);
            Scribe_Collections.Look(ref originalThings, "originalThings", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref terrainChanged, "terrainChanged", false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Vector3> wallPositions = wallCells?.Select(c => c.ToVector3()).ToList();
                Scribe_Collections.Look(ref wallPositions, "wallCells", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<Vector3> wallPositions = null;
                Scribe_Collections.Look(ref wallPositions, "wallCells", LookMode.Value);
                if (wallPositions != null)
                {
                    wallCells = new HashSet<IntVec3>(wallPositions.Select(v => v.ToIntVec3()));
                }
            }
        }
    }
}
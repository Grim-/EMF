using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class LayoutDataManager
    {
        private Dictionary<IntVec2, List<PlacementInfo>> cellContents = new Dictionary<IntVec2, List<PlacementInfo>>();
        private StructureLayoutDef layoutDef;
        private int currentStageIndex;
        private bool showAllStages;

        public event Action OnDataChanged;

        private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        private Color gridFarColor = new Color(1f, 1f, 1f, 0.1f);
        private Color terrainColor = new Color(0.5f, 0.4f, 0.3f, 0.8f);
        private Color wallColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        private Color doorColor = new Color(0.6f, 0.4f, 0.2f, 0.8f);
        private Color powerColor = new Color(1f, 1f, 0f, 0.8f);
        private Color furnitureColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
        private Color otherColor = new Color(0.8f, 0.4f, 0.8f, 0.8f);
        public LayoutDataManager(StructureLayoutDef layout)
        {
            layoutDef = layout.DeepCopy();
        }

        public List<PlacementInfo> GetCellContents(IntVec2 position)
        {
            return cellContents.TryGetValue(position, out var contents) ?
                new List<PlacementInfo>(contents) :
                new List<PlacementInfo>();
        }

        public void AddPlacement(IntVec2 position, PlacementInfo info)
        {
            // Get all cells this placement will occupy
            var occupiedCells = GetOccupiedCells(position, info);

            // Remove existing placements in those cells
            foreach (var cell in occupiedCells)
            {
                RemoveExistingAtCell(cell, info);
            }

            // Add to all occupied cells
            foreach (var cell in occupiedCells)
            {
                if (!cellContents.ContainsKey(cell))
                {
                    cellContents[cell] = new List<PlacementInfo>();
                }
                cellContents[cell].Add(info);
            }

            UpdateLayoutDef();
            OnDataChanged?.Invoke();
        }
        private HashSet<IntVec2> GetOccupiedCells(IntVec2 anchor, PlacementInfo info)
        {
            var cells = new HashSet<IntVec2>();

            if (info.thingDef != null)
            {
                IntVec2 size = info.thingDef.size;
                if (info.rotation == Rot4.East || info.rotation == Rot4.West)
                {
                    size = new IntVec2(size.z, size.x);
                }

                for (int x = 0; x < size.x; x++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        cells.Add(new IntVec2(anchor.x + x, anchor.z + z));
                    }
                }
            }
            else
            {
                cells.Add(anchor);
            }

            return cells;
        }
        private void RemoveExistingAtCell(IntVec2 cell, PlacementInfo newPlacement)
        {
            if (!cellContents.ContainsKey(cell)) return;

            var toRemove = new List<PlacementInfo>();

            foreach (var existing in cellContents[cell])
            {
                // Remove terrain only if placing terrain
                // Remove everything else when placing non-terrain
                if (newPlacement.terrainDef != null)
                {
                    if (existing.terrainDef != null)
                        toRemove.Add(existing);
                }
                else
                {
                    toRemove.Add(existing);
                }
            }

            // If removing multi-cell objects, remove from all their cells
            foreach (var placement in toRemove)
            {
                if (placement.thingDef != null && (placement.thingDef.size.x > 1 || placement.thingDef.size.z > 1))
                {
                    RemoveMultiCellPlacement(placement);
                }
                else
                {
                    cellContents[cell].Remove(placement);
                }
            }

            if (cellContents[cell].Count == 0)
                cellContents.Remove(cell);
        }

        private void RemoveMultiCellPlacement(PlacementInfo placement)
        {
            var cellsToCheck = cellContents.Keys.ToList();
            foreach (var cell in cellsToCheck)
            {
                if (cellContents[cell].Contains(placement))
                {
                    cellContents[cell].Remove(placement);
                    if (cellContents[cell].Count == 0)
                        cellContents.Remove(cell);
                }
            }
        }

        public void RemovePlacement(IntVec2 position, PlacementInfo placement)
        {
            if (cellContents.ContainsKey(position))
            {
                cellContents[position].Remove(placement);
                if (cellContents[position].Count == 0)
                {
                    cellContents.Remove(position);
                }
                UpdateLayoutDef();
                OnDataChanged?.Invoke();
            }
        }

        public void RemoveAllAtPosition(IntVec2 position)
        {
            if (cellContents.ContainsKey(position))
            {
                cellContents.Remove(position);
                UpdateLayoutDef();
                OnDataChanged?.Invoke();
            }
        }
        public void RotatePlacement(IntVec2 position, PlacementInfo placement)
        {
            placement.rotation.Rotate(RotationDirection.Clockwise);
            UpdateLayoutDef();
        }

        private void UpdateLayoutDef()
        {
            // Update the actual def based on current cell contents
        }

        public void ProcessStageData(int stageIndex, bool showAll)
        {
            currentStageIndex = stageIndex;
            showAllStages = showAll;
            RefreshData();
        }

        private void RefreshData()
        {
            cellContents.Clear();

            if (showAllStages)
            {
                for (int i = 0; i < layoutDef.stages.Count; i++)
                {
                    ProcessStage(i);
                }
            }
            else
            {
                ProcessStage(currentStageIndex);
            }

            OnDataChanged?.Invoke();
        }

        private void ProcessStage(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= layoutDef.stages.Count) return;

            BuildingStage stage = layoutDef.stages[stageIndex];

            foreach (var terrain in stage.terrain)
            {
                AddPlacement(terrain.position, new PlacementInfo
                {
                    category = "Terrain",
                    terrainDef = terrain.terrain,
                    stage = stageIndex,
                    categoryColor = terrainColor
                });
            }

            foreach (var wall in stage.walls)
            {
                AddPlacement(wall.position, new PlacementInfo
                {
                    category = "Wall",
                    thingDef = wall.thing,
                    stuffDef = wall.stuff,
                    rotation = wall.rotation,
                    stage = stageIndex,
                    categoryColor = wallColor
                });
            }

            foreach (var door in stage.doors)
            {
                AddPlacement(door.position, new PlacementInfo
                {
                    category = "Door",
                    thingDef = door.thing,
                    stuffDef = door.stuff,
                    rotation = door.rotation,
                    stage = stageIndex,
                    categoryColor = doorColor
                });
            }

            foreach (var power in stage.power)
            {
                AddPlacement(power.position, new PlacementInfo
                {
                    category = "Power",
                    thingDef = power.thing,
                    stuffDef = power.stuff,
                    rotation = power.rotation,
                    stage = stageIndex,
                    categoryColor = powerColor
                });
            }

            foreach (var furniture in stage.furniture)
            {
                AddPlacement(furniture.position, new PlacementInfo
                {
                    category = "Furniture",
                    thingDef = furniture.thing,
                    stuffDef = furniture.stuff,
                    rotation = furniture.rotation,
                    stage = stageIndex,
                    categoryColor = furnitureColor
                });
            }

            foreach (var other in stage.other)
            {
                AddPlacement(other.position, new PlacementInfo
                {
                    category = "Other",
                    thingDef = other.thing,
                    stuffDef = other.stuff,
                    rotation = other.rotation,
                    stage = stageIndex,
                    categoryColor = otherColor
                });
            }
        }
    }
}
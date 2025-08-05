using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public static class StructurePreviewUtility
    {
        /// <summary>
        /// Draws the preview for all stages of a structure layout or a specific stage
        /// </summary>
        public static void DrawStagePreview(StructureLayoutDef layout, IntVec3 center, Rot4 rot, Map map,
            int currentStagePreviewIndex, List<Color> previewColors, Color defaultPreviewColor,
            int nextWallIndex = 0, int nextDoorIndex = 0, int nextPowerIndex = 0,
            int nextFurnitureIndex = 0, int nextOtherIndex = 0, List<IntVec3> allPreviewCells = null)
        {
            if (allPreviewCells == null)
                allPreviewCells = new List<IntVec3>();

            if (currentStagePreviewIndex == -1)
            {
                for (int i = 0; i < layout.stages.Count; i++)
                {
                    int colorIndex = i % previewColors.Count;
                    Color startColor = previewColors[colorIndex];
                    BuildingStage currentBStage = layout.stages[i];

                    DrawThingPreviewsForStage(currentBStage, center, rot, map, startColor, allPreviewCells);
                }
            }
            else
            {
                int previewStage = Mathf.Clamp(currentStagePreviewIndex, 0, layout.stages.Count - 1);
                BuildingStage currentBStage = layout.stages[previewStage];
                DrawThingPreviewsForStage(currentBStage, center, rot, map, defaultPreviewColor, allPreviewCells);
            }
        }

        /// <summary>
        /// Draws all thing previews for a specific building stage
        /// </summary>
        private static void DrawThingPreviewsForStage(BuildingStage stage, IntVec3 center, Rot4 rot, Map map,
            Color previewColor, List<IntVec3> allPreviewCells)
        {
            DrawThingPreviews(stage.walls, center, rot, map, previewColor, allPreviewCells);
            DrawThingPreviews(stage.doors, center, rot, map, previewColor, allPreviewCells);
            DrawThingPreviews(stage.power, center, rot, map, previewColor, allPreviewCells);
            DrawThingPreviews(stage.furniture, center, rot, map, previewColor, allPreviewCells);
            DrawThingPreviews(stage.other, center, rot, map, previewColor, allPreviewCells);
        }

        /// <summary>
        /// Draws previews for specific thing placements
        /// </summary>
        public static void DrawThingPreviews(List<ThingPlacement> things, IntVec3 center, Rot4 rot, Map map,
            Color previewColor, List<IntVec3> allPreviewCells, ThingDef materialOverride = null)
        {
            foreach (ThingPlacement placement in things)
            {
                if (placement.thing == null || !placement.thing.BuildableByPlayer)
                    continue;

                IntVec3 pos = CalculatePosition(center, placement.position, rot);

                if (!pos.InBounds(map))
                    continue;

                if (allPreviewCells != null && !allPreviewCells.Contains(pos))
                    allPreviewCells.Add(pos);

                // Draw the ghost graphic
                ThingDef stuffToUse = placement.thing.MadeFromStuff && materialOverride != null
                    ? materialOverride
                    : placement.stuff;

                Graphic ghostGraphic = GhostUtility.GhostGraphicFor(
                    placement.thing.graphic,
                    placement.thing,
                    previewColor,
                    stuffToUse);

                if (ghostGraphic != null)
                {
                    // Calculate rotation
                    Rot4 finalRot = new Rot4((rot.AsInt + placement.rotation.AsInt) % 4);

                    // Draw the mesh
                    Mesh mesh = ghostGraphic.MeshAt(finalRot);
                    Material mat = ghostGraphic.MatAt(finalRot, null);
                    Quaternion quat = finalRot.AsQuat;
                    Vector3 drawPos = pos.ToVector3Shifted();
                    drawPos.y = AltitudeLayer.Blueprint.AltitudeFor();
                    drawPos += ghostGraphic.DrawOffset(finalRot);

                    Graphics.DrawMesh(mesh, drawPos, quat, mat, 0);
                }
            }
        }

        /// <summary>
        /// Draws previews for terrain placements
        /// </summary>
        public static void DrawTerrainPreviews(List<TerrainPlacement> terrains, IntVec3 center, Rot4 rot, Map map,
            Color previewColor, List<IntVec3> allPreviewCells)
        {
            foreach (TerrainPlacement placement in terrains)
            {
                if (placement.terrain == null)
                    continue;

                IntVec3 pos = CalculatePosition(center, placement.position, rot);

                if (!pos.InBounds(map))
                    continue;

                if (allPreviewCells != null && !allPreviewCells.Contains(pos))
                    allPreviewCells.Add(pos);

                GenDraw.DrawFieldEdges(new List<IntVec3> { pos }, previewColor);
            }
        }

        /// <summary>
        /// Draws an outline around the entire structure
        /// </summary>
        public static void DrawStructureOutline(BuildingStage stage, IntVec3 center, Rot4 rot, Map map, Color outlineColor)
        {
            HashSet<IntVec3> structureCells = new HashSet<IntVec3>();

            // Add cells from all placements
            AddCellsFromPlacements(stage.walls, center, rot, map, structureCells);
            AddCellsFromPlacements(stage.doors, center, rot, map, structureCells);
            AddCellsFromPlacements(stage.power, center, rot, map, structureCells);
            AddCellsFromPlacements(stage.furniture, center, rot, map, structureCells);
            AddCellsFromPlacements(stage.other, center, rot, map, structureCells);

            AddCellsFromTerrainPlacements(stage.terrain, center, rot, map, structureCells);

            if (structureCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(structureCells.ToList(), outlineColor);
            }
        }

        /// <summary>
        /// Helper method to add cells from thing placements to a set
        /// </summary>
        private static void AddCellsFromPlacements(List<ThingPlacement> placements, IntVec3 center, Rot4 rot, Map map,
            HashSet<IntVec3> cells)
        {
            foreach (var placement in placements)
            {
                IntVec3 pos = CalculatePosition(center, placement.position, rot);
                if (pos.InBounds(map))
                    cells.Add(pos);
            }
        }

        /// <summary>
        /// Helper method to add cells from terrain placements to a set
        /// </summary>
        private static void AddCellsFromTerrainPlacements(List<TerrainPlacement> placements, IntVec3 center, Rot4 rot, Map map,
            HashSet<IntVec3> cells)
        {
            foreach (var placement in placements)
            {
                IntVec3 pos = CalculatePosition(center, placement.position, rot);
                if (pos.InBounds(map))
                    cells.Add(pos);
            }
        }

        /// <summary>
        /// Calculates the final position of a placement based on center, relative position and rotation
        /// </summary>
        public static IntVec3 CalculatePosition(IntVec3 center, IntVec2 relativePos, Rot4 rot)
        {
            IntVec3 offset;

            switch (rot.AsInt)
            {
                case 0: // North
                    offset = new IntVec3(relativePos.x, 0, relativePos.z);
                    break;
                case 1: // East
                    offset = new IntVec3(relativePos.z, 0, -relativePos.x);
                    break;
                case 2: // South
                    offset = new IntVec3(-relativePos.x, 0, -relativePos.z);
                    break;
                case 3: // West
                    offset = new IntVec3(-relativePos.z, 0, relativePos.x);
                    break;
                default:
                    offset = new IntVec3(relativePos.x, 0, relativePos.z);
                    break;
            }

            return center + offset;
        }
    }
}
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class PlaceWorker_GrowableStructure : PlaceWorker
    {
        private List<Color> previewColors = new List<Color>()
        {
            Color.cyan,
            Color.yellow,
            Color.green,
            Color.blue
        };

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);

            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return;

            StructureLayoutDef layout = growableDef.structureLayout;
            Map map = Find.CurrentMap;

            if (map == null || layout.stages.Count == 0)
                return;

            int finalStageIndex = layout.stages.Count - 1;
            Color previewColor = new Color(0.2f, 0.8f, 0.9f, 0.4f);

            GenDraw.DrawFieldEdges(new List<IntVec3> { center }, Color.yellow);

            // Draw just the final stage (we could show all stages if desired)
            StructurePreviewUtility.DrawStagePreview(
                layout,
                center,
                rot,
                map,
                finalStageIndex,
                previewColors,
                previewColor);

            // Draw outline of the entire structure
            StructurePreviewUtility.DrawStructureOutline(
                layout.stages[finalStageIndex],
                center,
                rot,
                map,
                Color.cyan);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            GrowableStructureDef growableDef = def as GrowableStructureDef;
            if (growableDef?.structureLayout == null)
                return true;

            StructureLayoutDef layout = growableDef.structureLayout;
            if (layout.stages.Count == 0)
                return true;

            BuildingStage finalStage = layout.stages[layout.stages.Count - 1];

            bool hasEnoughSpace = CheckSpaceForStructure(finalStage, center, rot, map, thingToIgnore);
            if (!hasEnoughSpace)
                return new AcceptanceReport("MagicAndMyths.NotEnoughSpaceToGrow".Translate());

            return true;
        }

        private bool CheckSpaceForStructure(BuildingStage stage, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore)
        {
            List<IntVec3> allPositions = new List<IntVec3>();
            foreach (var placement in stage.walls)
            {
                allPositions.Add(StructurePreviewUtility.CalculatePosition(center, placement.position, rot));
            }

            foreach (var placement in stage.doors)
            {
                allPositions.Add(StructurePreviewUtility.CalculatePosition(center, placement.position, rot));
            }

            foreach (var placement in stage.power)
            {
                allPositions.Add(StructurePreviewUtility.CalculatePosition(center, placement.position, rot));
            }

            foreach (var placement in stage.furniture)
            {
                allPositions.Add(StructurePreviewUtility.CalculatePosition(center, placement.position, rot));
            }

            foreach (var placement in stage.other)
            {
                allPositions.Add(StructurePreviewUtility.CalculatePosition(center, placement.position, rot));
            }

            foreach (IntVec3 pos in allPositions)
            {
                if (!pos.InBounds(map))
                    return false;

                if (pos != center && !CanPlaceAtPosition(pos, map, thingToIgnore))
                    return false;
            }

            return true;
        }

        private bool CanPlaceAtPosition(IntVec3 pos, Map map, Thing thingToIgnore)
        {
            TerrainDef terrain = map.terrainGrid.TerrainAt(pos);
            if (terrain != null && !terrain.affordances.Contains(TerrainAffordanceDefOf.Heavy))
                return false;

            List<Thing> thingsAtPos = pos.GetThingList(map);
            foreach (Thing t in thingsAtPos)
            {
                if (t == thingToIgnore)
                    continue;

                if (t.def.passability == Traversability.Impassable ||
                    t.def.BuildableByPlayer ||
                    t.def.category == ThingCategory.Building)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

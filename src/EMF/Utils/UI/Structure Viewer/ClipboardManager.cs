using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ClipboardManager
    {
        private Dictionary<IntVec2, List<PlacementInfo>> clipboard = new Dictionary<IntVec2, List<PlacementInfo>>();
        private IntVec2? clipboardOrigin;
        private Window_StructureLayoutViewer parent;

        public bool HasContent => clipboard.Count > 0;

        public ClipboardManager(Window_StructureLayoutViewer parent)
        {
            this.parent = parent;
        }

        public void Copy()
        {
            clipboard.Clear();
            clipboardOrigin = null;

            HashSet<IntVec2> cellsToCopy = null;

            if (parent.GetCurrentTool() is SelectionTool selectionTool)
            {
                cellsToCopy = selectionTool.GetSelectedCells();
            }
            else
            {

            }
            if (cellsToCopy == null || cellsToCopy.Count == 0)
            {
                IntVec2? mousePos = parent.GridRenderer.GetGridPositionFromMouse(
                    Event.current.mousePosition - parent.CurrentGridRect.position,
                    parent.GridCenter);

                if (mousePos.HasValue)
                {
                    cellsToCopy = new HashSet<IntVec2> { mousePos.Value };
                }
            }

            if (cellsToCopy != null && cellsToCopy.Count > 0)
            {
                // Find the min corner as origin
                clipboardOrigin = cellsToCopy.OrderBy(c => c.x).ThenBy(c => c.z).First();

                foreach (var cell in cellsToCopy)
                {
                    var contents = parent.DataManager.GetCellContents(cell);
                    if (contents.Count > 0)
                    {
                        clipboard[cell - clipboardOrigin.Value] = new List<PlacementInfo>(contents);
                    }
                }

                Messages.Message($"Copied {clipboard.Count} cells", MessageTypeDefOf.NeutralEvent);
            }
        }

        public void Paste()
        {
            if (clipboard.Count == 0 || !clipboardOrigin.HasValue)
                return;

            // Get paste position - mouse position
            IntVec2? pastePos = parent.GridRenderer.GetGridPositionFromMouse(
                Event.current.mousePosition - parent.CurrentGridRect.position,
                parent.GridCenter);

            if (!pastePos.HasValue)
                return;

            foreach (var kvp in clipboard)
            {
                IntVec2 targetPos = pastePos.Value + kvp.Key;

                foreach (var placement in kvp.Value)
                {
                    var newPlacement = new PlacementInfo
                    {
                        category = placement.category,
                        thingDef = placement.thingDef,
                        terrainDef = placement.terrainDef,
                        stuffDef = placement.stuffDef,
                        rotation = placement.rotation,
                        stage = parent.ShowAllStages ? 0 : parent.CurrentStageIndex,
                        categoryColor = placement.categoryColor
                    };

                    parent.DataManager.AddPlacement(targetPos, newPlacement);
                }
            }

            Messages.Message($"Pasted {clipboard.Count} cells", MessageTypeDefOf.PositiveEvent);
        }

        public void Cut()
        {
            Copy();

            if (parent.GetCurrentTool() is SelectionTool selectionTool)
            {
                foreach (var cell in selectionTool.GetSelectedCells())
                {
                    parent.DataManager.RemoveAllAtPosition(cell);
                }
                selectionTool.ClearSelection();
            }
        }
    }
}
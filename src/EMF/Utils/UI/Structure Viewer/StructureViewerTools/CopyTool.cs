using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class CopyTool : LayoutEditorTool
    {
        private HashSet<IntVec2> copiedCells = new HashSet<IntVec2>();
        private Dictionary<IntVec2, List<PlacementInfo>> clipboard = new Dictionary<IntVec2, List<PlacementInfo>>();
        private IntVec2? copyOrigin;

        public CopyTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Copy";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/Copy");
        }

        public override void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (evt.shift && copiedCells.Count > 0)
                {
                    PasteAtPosition(gridPos);
                }
                else
                {
                    CopyFromPosition(gridPos);
                }
                evt.Use();
            }
        }

        private void CopyFromPosition(IntVec2 gridPos)
        {
            clipboard.Clear();
            copiedCells.Clear();

            // Check if selection tool has cells selected
            var selectionTool = parent.GetCurrentTool() as SelectionTool;
            var cellsToCopy = selectionTool?.GetSelectedCells();

            if (cellsToCopy != null && cellsToCopy.Count > 0)
            {
                // Copy selection
                copyOrigin = cellsToCopy.MinBy(c => c.x + c.z * 1000);
                foreach (var cell in cellsToCopy)
                {
                    var contents = parent.DataManager.GetCellContents(cell);
                    if (contents.Count > 0)
                    {
                        clipboard[cell - copyOrigin.Value] = new List<PlacementInfo>(contents);
                        copiedCells.Add(cell);
                    }
                }
                Messages.Message($"Copied {copiedCells.Count} cells", MessageTypeDefOf.NeutralEvent);
            }
            else
            {
                // Copy single cell
                copyOrigin = gridPos;
                var contents = parent.DataManager.GetCellContents(gridPos);
                if (contents.Count > 0)
                {
                    clipboard[IntVec2.Zero] = new List<PlacementInfo>(contents);
                    copiedCells.Add(gridPos);
                    Messages.Message($"Copied {contents.Count} items", MessageTypeDefOf.NeutralEvent);
                }
            }
        }

        private void PasteAtPosition(IntVec2 gridPos)
        {
            if (clipboard.Count == 0 || !copyOrigin.HasValue) return;

            foreach (var kvp in clipboard)
            {
                IntVec2 targetPos = gridPos + kvp.Key;

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

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            // Not used - copy has its own logic
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (copiedCells.Contains(gridPos))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(0f, 1f, 0f, 0.3f));
            }
        }

        public override void DrawToolOptions(Rect rect)
        {
            Rect labelRect = new Rect(rect.x, rect.y, 200f, rect.height);
            if (clipboard.Count > 0)
            {
                Widgets.Label(labelRect, $"Copied: {clipboard.Count} cells (Shift+Click to paste)");
            }
            else
            {
                Widgets.Label(labelRect, "Click to copy cell contents");
            }
        }
    }
}
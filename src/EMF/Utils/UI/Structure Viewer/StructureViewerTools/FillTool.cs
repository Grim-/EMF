using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class FillTool : PlaceEditorTool
    {
        private HashSet<IntVec2> previewCells = new HashSet<IntVec2>();

        public FillTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Fill";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/Fill");
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            previewCells.Clear();
        }

        public override void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (FillItem == null)
                return;

            if (evt.type == EventType.MouseMove)
            {
                UpdatePreview(gridPos);
            }
            else if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                FloodFill();
                evt.Use();
            }
        }

        private void UpdatePreview(IntVec2 gridPos)
        {
            previewCells.Clear();
            var targetContents = parent.DataManager.GetCellContents(gridPos);
            FloodFillPreview(gridPos, targetContents, new HashSet<IntVec2>());
        }

        private void FloodFillPreview(IntVec2 pos, List<PlacementInfo> matchContents, HashSet<IntVec2> visited)
        {
            if (visited.Contains(pos) || Mathf.Abs(pos.x) > 20 || Mathf.Abs(pos.z) > 20)
                return;

            visited.Add(pos);

            var currentContents = parent.DataManager.GetCellContents(pos);
            if (AreCellContentsEqual(currentContents, matchContents))
            {
                previewCells.Add(pos);
                FloodFillPreview(pos + new IntVec2(1, 0), matchContents, visited);
                FloodFillPreview(pos + new IntVec2(-1, 0), matchContents, visited);
                FloodFillPreview(pos + new IntVec2(0, 1), matchContents, visited);
                FloodFillPreview(pos + new IntVec2(0, -1), matchContents, visited);
            }
        }

        private bool AreCellContentsEqual(List<PlacementInfo> a, List<PlacementInfo> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].thingDef != b[i].thingDef ||
                    a[i].terrainDef != b[i].terrainDef ||
                    a[i].category != b[i].category)
                    return false;
            }
            return true;
        }

        private void FloodFill()
        {
            foreach (var pos in previewCells)
            {
                parent.ApplyPaintItem(pos, FillItem);
            }
            previewCells.Clear();
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (previewCells.Contains(gridPos))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(1f, 1f, 1f, 0.3f));
            }
        }
    }
}
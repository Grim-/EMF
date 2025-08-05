using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    // Line Tool
    public class LineTool : PlaceEditorTool
    {
        private IntVec2? lineStart;
        private IntVec2? lineEnd;
        private List<IntVec2> previewLine = new List<IntVec2>();

        public LineTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Line";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/LineTool");
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            lineStart = null;
            lineEnd = null;
            previewLine.Clear();
        }

        public override void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (FillItem == null)
                return;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (!lineStart.HasValue)
                {
                    lineStart = gridPos;
                }
                else
                {
                    DrawLine();
                    lineStart = null;
                    lineEnd = null;
                    previewLine.Clear();
                }
                evt.Use();
            }
            else if (evt.type == EventType.MouseMove && lineStart.HasValue)
            {
                lineEnd = gridPos;
                UpdateLinePreview();
            }
        }

        private void UpdateLinePreview()
        {
            previewLine.Clear();
            if (!lineStart.HasValue || !lineEnd.HasValue)
                return;

            // Bresenham's line algorithm
            int x0 = lineStart.Value.x;
            int z0 = lineStart.Value.z;
            int x1 = lineEnd.Value.x;
            int z1 = lineEnd.Value.z;

            int dx = Mathf.Abs(x1 - x0);
            int dz = Mathf.Abs(z1 - z0);
            int sx = x0 < x1 ? 1 : -1;
            int sz = z0 < z1 ? 1 : -1;
            int err = dx - dz;

            while (true)
            {
                previewLine.Add(new IntVec2(x0, z0));

                if (x0 == x1 && z0 == z1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dz)
                {
                    err -= dz;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    z0 += sz;
                }
            }
        }

        private void DrawLine()
        {
            foreach (var pos in previewLine)
            {
                parent.ApplyPaintItem(pos, FillItem);
            }
        }

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            // Not used - line tool has its own logic
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (previewLine.Contains(gridPos))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(1f, 1f, 0f, 0.4f));
            }
            else if (lineStart.HasValue && gridPos == lineStart.Value)
            {
                Widgets.DrawBoxSolid(cellRect, new Color(0f, 1f, 0f, 0.5f));
            }
        }

        public override void DrawToolOptions(Rect rect)
        {
            if (lineStart.HasValue)
            {
                Rect statusRect = new Rect(rect.x, rect.y, 200f, 30f);
                Widgets.Label(statusRect, "Click to set end point");
            }
        }
    }
}
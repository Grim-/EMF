using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class RectangleTool : PlaceEditorTool
    {
        private IntVec2? rectStart;
        private IntVec2? rectEnd;
        private bool fillRect = false;
        private List<IntVec2> previewRect = new List<IntVec2>();

        public RectangleTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Rectangle";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/RectangleTool");
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            rectStart = null;
            rectEnd = null;
            previewRect.Clear();
        }

        public override void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (FillItem == null)
                return;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (!rectStart.HasValue)
                {
                    rectStart = gridPos;
                }
                else
                {
                    DrawRectangle();
                    rectStart = null;
                    rectEnd = null;
                    previewRect.Clear();
                }
                evt.Use();
            }
            else if (evt.type == EventType.MouseMove && rectStart.HasValue)
            {
                rectEnd = gridPos;
                UpdateRectPreview();
            }
        }

        private void UpdateRectPreview()
        {
            previewRect.Clear();
            if (!rectStart.HasValue || !rectEnd.HasValue)
                return;

            int minX = Mathf.Min(rectStart.Value.x, rectEnd.Value.x);
            int maxX = Mathf.Max(rectStart.Value.x, rectEnd.Value.x);
            int minZ = Mathf.Min(rectStart.Value.z, rectEnd.Value.z);
            int maxZ = Mathf.Max(rectStart.Value.z, rectEnd.Value.z);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (fillRect || x == minX || x == maxX || z == minZ || z == maxZ)
                    {
                        previewRect.Add(new IntVec2(x, z));
                    }
                }
            }
        }

        private void DrawRectangle()
        {
            foreach (var pos in previewRect)
            {
                parent.ApplyPaintItem(pos, FillItem);
            }
        }

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            // Not used - rectangle tool has its own logic
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (previewRect.Contains(gridPos))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(0.5f, 0.5f, 1f, 0.4f));
            }
            else if (rectStart.HasValue && gridPos == rectStart.Value)
            {
                Widgets.DrawBoxSolid(cellRect, new Color(0f, 1f, 0f, 0.5f));
            }
        }

        public override void DrawToolOptions(Rect rect)
        {
            Rect fillToggle = new Rect(rect.x, rect.y, 100f, 30f);
            Widgets.CheckboxLabeled(fillToggle, "Fill", ref fillRect);

            if (rectStart.HasValue)
            {
                Rect statusRect = new Rect(fillToggle.xMax + 10f, rect.y, 200f, 30f);
                Widgets.Label(statusRect, "Click to set corner");
            }
        }
    }
}
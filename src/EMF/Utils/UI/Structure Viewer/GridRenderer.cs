using UnityEngine;
using Verse;

namespace EMF
{
    public class GridRenderer
    {
        private Window_StructureLayoutViewer parent;
        private float cellSize = 40f;
        private float zoomFactor = 1f;
        private Vector2 gridOffset = Vector2.zero;

        private Color gridColor = new Color(1f, 1f, 1f, 0.3f);
        private Color gridFarColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

        public float CellDrawSize => cellSize * zoomFactor;

        public GridRenderer(Window_StructureLayoutViewer parent)
        {
            this.parent = parent;
        }

        public void DrawGrid(Rect rect, int gridRadius, Vector2 gridCenter)
        {
            Vector2 adjustedCenter = gridCenter + gridOffset;

            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector2 cellPos = adjustedCenter + new Vector2(x * CellDrawSize, -z * CellDrawSize);
                    Rect cellRect = new Rect(cellPos.x - CellDrawSize / 2f,
                                           cellPos.y - CellDrawSize / 2f,
                                           CellDrawSize, CellDrawSize);

                    if (!cellRect.Overlaps(new Rect(0, 0, rect.width, rect.height)))
                        continue;

                    DrawGridCell(cellRect, new IntVec2(x, z), adjustedCenter);
                }
            }

            DrawCenterMarker(adjustedCenter);
        }

        private void DrawGridCell(Rect cellRect, IntVec2 gridCoord, Vector2 center)
        {
            float t = Mathf.Clamp01(Vector2.Distance(cellRect.center, center) / (20 * CellDrawSize));
            Widgets.DrawBoxSolidWithOutline(cellRect, Color.clear, Color.Lerp(gridColor, gridFarColor, t));

            parent.DrawCellContents(cellRect, gridCoord);
        }

        private void DrawCenterMarker(Vector2 center)
        {
            Rect centerMarker = new Rect(center.x - 5f, center.y - 5f, 10f, 10f);
            Widgets.DrawBoxSolid(centerMarker, Color.red);
        }

        public void HandlePanning(Event evt, ref bool isDragging, ref Vector2 dragStartPos)
        {
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                isDragging = true;
                dragStartPos = evt.mousePosition;
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag && isDragging)
            {
                Vector2 delta = evt.mousePosition - dragStartPos;
                gridOffset += delta;
                dragStartPos = evt.mousePosition;
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp && isDragging)
            {
                isDragging = false;
                evt.Use();
            }
        }

        public void HandleZoom(Event evt)
        {
            if (evt.type == EventType.ScrollWheel)
            {
                float newZoomFactor = zoomFactor - evt.delta.y * 0.03f;
                zoomFactor = Mathf.Clamp(newZoomFactor, 0.1f, 10f);
                evt.Use();
            }
        }

        public IntVec2? GetGridPositionFromMouse(Vector2 mousePos, Vector2 gridCenter)
        {
            Vector2 adjustedCenter = gridCenter + gridOffset;
            Vector2 relativePos = mousePos - adjustedCenter;

            int x = Mathf.RoundToInt(relativePos.x / CellDrawSize);
            int z = -Mathf.RoundToInt(relativePos.y / CellDrawSize);

            return new IntVec2(x, z);
        }

        public void ResetView()
        {
            gridOffset = Vector2.zero;
            zoomFactor = 1f;
        }
    }

}
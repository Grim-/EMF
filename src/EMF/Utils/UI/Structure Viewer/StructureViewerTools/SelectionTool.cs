using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    // Selection Tool
    public class SelectionTool : LayoutEditorTool
    {
        private HashSet<IntVec2> selectedCells = new HashSet<IntVec2>();
        private IntVec2? selectionStart;
        private IntVec2? selectionEnd;
        private bool isSelecting;

        public SelectionTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Select";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/Select");
        }

        public override void OnActivate()
        {
            selectedCells.Clear();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            ClearSelection();
        }

        public void ClearSelection()
        {
            selectedCells.Clear();
            selectionStart = null;
            selectionEnd = null;
            isSelecting = false;
        }

        public override void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (evt.shift)
                {
                    if (selectedCells.Contains(gridPos))
                        selectedCells.Remove(gridPos);
                    else
                        selectedCells.Add(gridPos);
                }
                else
                {
                    selectedCells.Clear();
                    selectedCells.Add(gridPos);
                    selectionStart = gridPos;
                    isSelecting = true;
                }
                evt.Use();
            }
            else if (evt.type == EventType.MouseDrag && isSelecting)
            {
                selectionEnd = gridPos;
                UpdateRectSelection();
                evt.Use();
            }
            else if (evt.type == EventType.MouseUp && isSelecting)
            {
                isSelecting = false;
                evt.Use();
            }
        }

        private void UpdateRectSelection()
        {
            if (!selectionStart.HasValue || !selectionEnd.HasValue) 
                return;

            selectedCells.Clear();

            int minX = Math.Min(selectionStart.Value.x, selectionEnd.Value.x);
            int maxX = Math.Max(selectionStart.Value.x, selectionEnd.Value.x);
            int minZ = Math.Min(selectionStart.Value.z, selectionEnd.Value.z);
            int maxZ = Math.Max(selectionStart.Value.z, selectionEnd.Value.z);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    selectedCells.Add(new IntVec2(x, z));
                }
            }
        }

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            // Not used - selection has its own logic
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (selectedCells.Contains(gridPos))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(0.5f, 0.5f, 1f, 0.3f));
            }
        }

        public HashSet<IntVec2> GetSelectedCells() => new HashSet<IntVec2>(selectedCells);
    }
}
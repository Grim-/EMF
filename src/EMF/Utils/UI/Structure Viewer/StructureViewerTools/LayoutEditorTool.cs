using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public abstract class LayoutEditorTool
    {
        protected Window_StructureLayoutViewer parent;
        protected string toolName;
        protected Texture2D icon;
        protected bool isDragging;
        protected HashSet<IntVec2> processedCells = new HashSet<IntVec2>();

        public string ToolName => toolName;
        public Texture2D Icon => icon;
        public IntVec2 CurrentGridPosition { get; protected set; }

        public LayoutEditorTool(Window_StructureLayoutViewer parent)
        {
            this.parent = parent;
        }

        public virtual void OnActivate() 
        {

        }

        public virtual void OnDeactivate()
        {
            isDragging = false;
            processedCells.Clear();
        }

        public virtual void HandleInput(Event evt, IntVec2 gridPos)
        {
            CurrentGridPosition = gridPos;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                if (CanProcessCell(gridPos))
                {
                    isDragging = true;
                    ProcessCell(gridPos);
                    evt.Use();
                }
            }
            else if (evt.type == EventType.MouseDrag && isDragging)
            {
                if (CanProcessCell(gridPos))
                {
                    ProcessCell(gridPos);
                    evt.Use();
                }
            }
            else if (evt.type == EventType.MouseUp)
            {
                isDragging = false;
                processedCells.Clear();
                evt.Use();
            }
        }

        protected virtual bool CanProcessCell(IntVec2 gridPos)
        {
            return !processedCells.Contains(gridPos);
        }

        protected void ProcessCell(IntVec2 gridPos)
        {
            if (processedCells.Contains(gridPos))
                return;

            processedCells.Add(gridPos);
            OnProcessCell(gridPos);
        }

        protected abstract void OnProcessCell(IntVec2 gridPos);

        public abstract void Draw(Rect cellRect, IntVec2 gridPos);

        public virtual void DrawToolOptions(Rect rect) { }
    }
}
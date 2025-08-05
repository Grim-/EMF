using UnityEngine;
using Verse;

namespace EMF
{
    // Delete Tool
    public class DeleteTool : LayoutEditorTool
    {
        public DeleteTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Delete";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/Delete");
        }

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            parent.RemoveAllAtPosition(gridPos);
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (Mouse.IsOver(cellRect))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(1f, 0f, 0f, 0.3f));
            }
        }
    }

}
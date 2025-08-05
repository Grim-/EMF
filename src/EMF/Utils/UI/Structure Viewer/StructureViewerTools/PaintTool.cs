using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    // Paint Tool
    public class PaintTool : PlaceEditorTool
    {
        public PaintTool(Window_StructureLayoutViewer parent) : base(parent)
        {
            toolName = "Paint";
            icon = ContentFinder<Texture2D>.Get("UI/StructureEditor/Paint");

            if (parent.PaletteBar != null && parent.PaletteBar.SelectedItem != null)
            {
                FillItem = parent.PaletteBar.SelectedItem;
            }
        }

        public override void Draw(Rect cellRect, IntVec2 gridPos)
        {
            if (FillItem != null && Mouse.IsOver(cellRect))
            {
                Widgets.DrawBoxSolid(cellRect, new Color(1f, 1f, 1f, 0.2f));
                if (FillItem.PreviewTexture != null)
                {
                    GUI.DrawTexture(cellRect.ContractedBy(cellRect.width * 0.2f), FillItem.PreviewTexture);
                }
            }
        }
    }
}
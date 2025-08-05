using Verse;

namespace EMF
{
    public abstract class PlaceEditorTool : LayoutEditorTool
    {
        private PaintableItem fillItem;
        public PaintableItem FillItem { get => fillItem; set => fillItem = value; }

        protected PlaceEditorTool(Window_StructureLayoutViewer parent) : base(parent) { }

        protected override bool CanProcessCell(IntVec2 gridPos)
        {
            return base.CanProcessCell(gridPos) && FillItem != null;
        }

        protected override void OnProcessCell(IntVec2 gridPos)
        {
            parent.ApplyPaintItem(gridPos, FillItem);
        }
    }
}
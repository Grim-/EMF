using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace EMF
{
    public class ToolBar
    {
        private Window_StructureLayoutViewer parent;

        private Dictionary<string, LayoutEditorTool> tools;
        private LayoutEditorTool currentTool;

        public LayoutEditorTool CurrentTool
        {
            get => currentTool;
            set => currentTool = value;
        }

        public ToolBar(Window_StructureLayoutViewer parent)
        {
            this.parent = parent;


            tools = new Dictionary<string, LayoutEditorTool>
            {
                ["select"] = new SelectionTool(parent),
                ["paint"] = new PaintTool(parent),
                ["delete"] = new DeleteTool(parent),
                ["fill"] = new FillTool(parent),
                ["line"] = new LineTool(parent),
                ["rect"] = new RectangleTool(parent)
            };

            currentTool = tools["select"];
            currentTool.OnActivate();
        }

        public void SelectToolByName(string toolName)
        {
            if (tools.ContainsKey(toolName))
            {
                SelectTool(tools[toolName]);
            }
        }

        public void SelectTool(LayoutEditorTool tool)
        {
            currentTool?.OnDeactivate();
            currentTool = tool;
            currentTool.OnActivate();
        }

        public void DrawToolbar(Rect inRect)
        {
            float toolbarY = 40f;
            float buttonSize = 40f;
            float spacing = 5f;
            float x = 10f;

            // Background for toolbar
            Rect toolbarBg = new Rect(5f, toolbarY - 5f, inRect.width - 10f, buttonSize + 10f);
            Widgets.DrawMenuSection(toolbarBg);

            // Draw tool buttons
            foreach (var kvp in tools)
            {
                var tool = kvp.Value;
                Rect buttonRect = new Rect(x, toolbarY, buttonSize, buttonSize);
                bool isActive = currentTool == tool;

                Color buttonColor = isActive ? Color.yellow : Color.white;
                if (Widgets.ButtonImage(buttonRect, tool.Icon, buttonColor, buttonColor))
                {
                    SelectTool(tool);
                }

                TooltipHandler.TipRegion(buttonRect, tool.ToolName);
                x += buttonSize + spacing;
            }

            // Separator
            x += 20f;
            Widgets.DrawLineVertical(x - 10f, toolbarY, buttonSize);

            // Copy/Paste command buttons
            Rect copyButton = new Rect(x, toolbarY, buttonSize, buttonSize);
            if (Widgets.ButtonImage(copyButton, ContentFinder<Texture2D>.Get("UI/StructureEditor/Copy")))
            {
                parent.Clipboard.Copy();
            }
            TooltipHandler.TipRegion(copyButton, "Copy (Ctrl+C)");
            x += buttonSize + spacing;

            Rect pasteButton = new Rect(x, toolbarY, buttonSize, buttonSize);
            bool canPaste = parent.Clipboard.HasContent;
            GUI.enabled = canPaste;
            if (Widgets.ButtonImage(pasteButton, ContentFinder<Texture2D>.Get("UI/StructureEditor/Paste")))
            {
                parent.Clipboard.Paste();
            }
            GUI.enabled = true;
            TooltipHandler.TipRegion(pasteButton, canPaste ? "Paste (Ctrl+V)" : "Nothing to paste");
            x += buttonSize + spacing;

            // Separator
            x += 20f;
            Widgets.DrawLineVertical(x - 10f, toolbarY, buttonSize);

            // Tool options area
            if (currentTool != null)
            {
                Rect optionsRect = new Rect(x, toolbarY, inRect.width - x - 20f, buttonSize);
                currentTool.DrawToolOptions(optionsRect);
            }
        }
    }
}
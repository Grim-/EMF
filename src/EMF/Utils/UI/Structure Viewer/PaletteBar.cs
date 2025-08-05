using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class PaletteBar
    {
        private Window_StructureLayoutViewer parent;
        private List<PaintableItem> recentItems = new List<PaintableItem>();
        private int maxItems = 20;
        private float itemSize = 32f;
        private float spacing = 2f;
        private Vector2 scrollPosition = Vector2.zero;
        private PaintableItem selectedItem;

        public PaintableItem SelectedItem => selectedItem;

        public PaletteBar(Window_StructureLayoutViewer parent)
        {
            this.parent = parent;
            LoadDefaultItems();
        }

        private void LoadDefaultItems()
        {
            TerrainDef smoothStone = TerrainDefOf.Sandstone_Smooth;
            if (smoothStone != null)
            {
                AddRecentItem(new PaintableItem(smoothStone));
            }

            ThingDef wall = ThingDefOf.Wall;
            ThingDef steel = ThingDefOf.Steel;
            if (wall != null && steel != null)
            {
                AddRecentItem(new PaintableItem(wall, steel, "Wall"));
            }

            ThingDef door = ThingDefOf.Door;
            if (door != null && steel != null)
            {
                AddRecentItem(new PaintableItem(door, steel, "Door"));
            }
        }

        public void Draw(Rect rect)
        {
            // Background
            Widgets.DrawWindowBackground(rect);

            float contentWidth = (itemSize + spacing) * recentItems.Count + spacing;

            Rect viewRect = new Rect(rect.x, rect.y, rect.width, rect.height);
            Rect contentRect = new Rect(0, 0, rect.width, rect.height);

            GUI.BeginGroup(viewRect);

            float x = spacing;
            float y = (rect.height - itemSize) / 2f;

            for (int i = 0; i < recentItems.Count; i++)
            {
                var item = recentItems[i];
                Rect itemRect = new Rect(x, y, itemSize, itemSize);

                DrawPaletteItem(itemRect, item, i);
                x += itemSize + spacing;
            }

            // Add button at the end
            Rect addButtonRect = new Rect(x, y, itemSize, itemSize);
            DrawAddButton(addButtonRect);
            GUI.EndGroup();
        }

        private void DrawPaletteItem(Rect rect, PaintableItem item, int index)
        {
            // Background
            bool isSelected = selectedItem == item;
            bool isHovered = Mouse.IsOver(rect);

            Color bgColor = isSelected ? new Color(0.3f, 0.5f, 0.7f, 0.8f) :
                           isHovered ? new Color(0.5f, 0.5f, 0.5f, 0.5f) :
                           new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Widgets.DrawBoxSolid(rect, bgColor);
            Widgets.DrawBox(rect);

            // Icon
            Rect iconRect = rect.ContractedBy(4f);
            if (item.PreviewTexture != null)
            {
                GUI.DrawTexture(iconRect, item.PreviewTexture);
            }

            // Category indicator
            Color categoryColor = parent.GetCategoryColor(item.Category);
            Rect indicatorRect = new Rect(rect.x, rect.yMax - 4f, rect.width, 4f);
            Widgets.DrawBoxSolid(indicatorRect, categoryColor);

            // Interaction
            if (Widgets.ButtonInvisible(rect))
            {
                if (Event.current.button == 0)
                {
                    SelectItem(item);
                }
                else if (Event.current.button == 1)
                {
                    ShowItemContextMenu(item, index);
                }
            }

            // Tooltip
            string tooltip = BuildTooltip(item);
            TooltipHandler.TipRegion(rect, tooltip);
        }

        private void DrawAddButton(Rect rect)
        {
            bool isHovered = Mouse.IsOver(rect);
            Color bgColor = isHovered ? new Color(0.5f, 0.5f, 0.5f, 0.5f) :
                           new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Widgets.DrawBoxSolid(rect, bgColor);
            Widgets.DrawBox(rect);

            // Draw plus sign
            float plusSize = rect.width * 0.5f;
            float plusThickness = 2f;
            Rect horizontalRect = new Rect(rect.center.x - plusSize / 2f,
                                          rect.center.y - plusThickness / 2f,
                                          plusSize, plusThickness);
            Rect verticalRect = new Rect(rect.center.x - plusThickness / 2f,
                                        rect.center.y - plusSize / 2f,
                                        plusThickness, plusSize);

            Widgets.DrawBoxSolid(horizontalRect, Color.white);
            Widgets.DrawBoxSolid(verticalRect, Color.white);

            if (Widgets.ButtonInvisible(rect))
            {
                ShowAddMenu();
            }

            TooltipHandler.TipRegion(rect, "Add item to palette");
        }

        private string BuildTooltip(PaintableItem item)
        {
            if (item.TerrainDef != null)
            {
                return $"{item.TerrainDef.LabelCap}\n{item.TerrainDef.description}";
            }
            else if (item.ThingDef != null)
            {
                string tooltip = item.ThingDef.LabelCap;
                if (item.StuffDef != null)
                {
                    tooltip += $" ({item.StuffDef.LabelCap})";
                }
                tooltip += $"\n{item.ThingDef.description}";
                return tooltip;
            }
            return "Unknown item";
        }

        private void SelectItem(PaintableItem item)
        {
            selectedItem = item;
            var currentTool = parent.GetCurrentTool();
            if (currentTool is PlaceEditorTool paintTool)
            {
                paintTool.FillItem = item;
            }
            else
            {
                parent.ToolBar.SelectToolByName("paint");
                if (parent.GetCurrentTool() is PlaceEditorTool newPaintTool)
                {
                    newPaintTool.FillItem = item;
                }
            }
        }

        private void ShowItemContextMenu(PaintableItem item, int index)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("Select", () => SelectItem(item)));

            if (item.ThingDef != null && item.ThingDef.rotatable)
            {
                options.Add(new FloatMenuOption("Rotate", () =>
                {
                    item.Rotation.Rotate(RotationDirection.Clockwise);
                }));
            }

            options.Add(new FloatMenuOption("Move to Front", () =>
            {
                recentItems.RemoveAt(index);
                recentItems.Insert(0, item);
            }));

            options.Add(new FloatMenuOption("Remove from Palette", () =>
            {
                recentItems.RemoveAt(index);
                if (selectedItem == item)
                {
                    selectedItem = null;
                }
            }));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowAddMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("Add Terrain...", () => ShowTerrainMenu()));
            options.Add(new FloatMenuOption("Add Wall...", () => ShowThingMenu("Wall")));
            options.Add(new FloatMenuOption("Add Door...", () => ShowThingMenu("Door")));
            options.Add(new FloatMenuOption("Add Power...", () => ShowThingMenu("Power")));
            options.Add(new FloatMenuOption("Add Furniture...", () => ShowThingMenu("Furniture")));
            options.Add(new FloatMenuOption("Add Other...", () => ShowThingMenu("Other")));

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowTerrainMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            var terrains = DefDatabase<TerrainDef>.AllDefsListForReading
                .Where(t => t.designationCategory != null)
                .OrderBy(t => t.label);

            foreach (var terrain in terrains)
            {
                options.Add(new FloatMenuOption(terrain.LabelCap, () =>
                {
                    var item = new PaintableItem(terrain);
                    AddRecentItem(item);
                    SelectItem(item);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowThingMenu(string category)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            var things = parent.GetThingsForCategory(category).OrderBy(t => t.label);

            foreach (var thing in things)
            {
                if (thing.MadeFromStuff)
                {
                    options.Add(new FloatMenuOption(thing.LabelCap + "...",
                        () => ShowStuffMenu(thing, category)));
                }
                else
                {
                    options.Add(new FloatMenuOption(thing.LabelCap, () =>
                    {
                        var item = new PaintableItem(thing, null, category);
                        AddRecentItem(item);
                        SelectItem(item);
                    }));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowStuffMenu(ThingDef thingDef, string category)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            var stuffs = GenStuff.AllowedStuffsFor(thingDef).OrderBy(s => s.label);

            foreach (var stuff in stuffs)
            {
                options.Add(new FloatMenuOption(stuff.LabelCap, () =>
                {
                    var item = new PaintableItem(thingDef, stuff, category);
                    AddRecentItem(item);
                    SelectItem(item);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        public void AddRecentItem(PaintableItem item)
        {
            // Check if item already exists
            var existing = recentItems.FirstOrDefault(i =>
                i.ThingDef == item.ThingDef &&
                i.TerrainDef == item.TerrainDef &&
                i.StuffDef == item.StuffDef);

            if (existing != null)
            {
                // Move to front
                recentItems.Remove(existing);
            }

            // Add to front
            recentItems.Insert(0, item);

            // Trim to max size
            if (recentItems.Count > maxItems)
            {
                recentItems.RemoveAt(recentItems.Count - 1);
            }
        }
    }
}
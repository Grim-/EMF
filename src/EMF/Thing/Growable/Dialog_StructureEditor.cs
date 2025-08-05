using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    public class Dialog_StructureEditor : Window
    {
        // General UI constants
        private const float STANDARD_MARGIN = 10f;
        private const float STANDARD_SPACING = 10f;
        private const float STANDARD_LINE_HEIGHT = 22f;
        private const float STANDARD_PADDING = 5f;
        private const float CONTENT_INDENT = 10f;
        private const float GENERAL_MARGIN_HEIGHT = 4f;

        // Header heights
        private const float TITLE_HEIGHT = 60f;
        private const float SECTION_HEADER_HEIGHT = 30f;
        private const float STAGE_HEADER_HEIGHT = 35f;
        private const float SIZE_HEADER_HEIGHT = 40f;
        private const float CATEGORY_HEADER_HEIGHT = 35f;
        private const float CATEGORY_SECTION_HEIGHT = 30f;

        // Label properties
        private const float STAGE_LABEL_HEIGHT = 200f;
        private const float STAGE_LABEL_WIDTH = 22f;
        private const float LABEL_WIDTH = 200f;

        // Expander properties
        private const float EXPANDER_WIDTH = 25f;
        private const float EXPANDER_HEIGHT = 22f;
        private const float STAGE_EXPANDER_WIDTH = 25f;
        private const float STAGE_EXPANDER_HEIGHT = 22f;

        // Item properties
        private const float LIST_ITEM_HEIGHT = 22f;
        private const float LIST_ITEM_INDENT = 60f;

        // Button properties
        private const float BUTTON_WIDTH = 130f;
        private const float BUTTON_HEIGHT = 30f;
        private const float BUTTON_ROW_HEIGHT = 40f;
        private const float ICON_BUTTON_SIZE = 30f;
        private const float SMALL_BUTTON_SIZE = 20f;

        // Settings panel
        private const float SETTINGS_PANEL_HEIGHT = 200f;

        // Scroll view properties
        private const float MAX_HEIGHT = 300f;
        private const float SCROLL_BAR_WIDTH = 16f;
        private const float HEIGHT_PADDING = 20f;

        // Existing fields
        private StructureLayoutDef structureDef;
        private Vector2 scrollPosition = Vector2.zero;
        private Dictionary<string, bool> expandedSections = new Dictionary<string, bool>();
        private bool viewXml = false;
        private bool copySuccessful = false;
        private float copyMessageTimer = 0f;

        private Zone_AreaCapture zone;
        private readonly Color headerColor = new Color(0.8f, 0.8f, 0.8f);
        private readonly Color sectionBgColor = new Color(0.21f, 0.21f, 0.21f);
        private readonly float lineHeight = 22f;
        public override Vector2 InitialSize => new Vector2(550f, 600f);

        // New fields for settings
        private string zoneName;
        private bool captureFloors = true;
        private bool captureTerrain = true;
        private bool captureThings = true;
        private IntVec2 originSize;


        private List<StructureLayoutDef> _layoutDefs = new List<StructureLayoutDef>();
        private List<StructureLayoutDef> layoutDefs
        {
            get
            {
                if (_layoutDefs == null || _layoutDefs.Count == 0)
                {
                    _layoutDefs = DefDatabase<StructureLayoutDef>.AllDefsListForReading.ToList();
                }

                return _layoutDefs;
            }
        }

        public Dialog_StructureEditor(Zone_AreaCapture Zone, StructureLayoutDef def)
        {
            LoadStructureLayoutDef(def);
            doCloseX = true;
            doCloseButton = false;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            layer = WindowLayer.Dialog;
            forcePause = true;
            resizeable = true;
            draggable = true;
            zone = Zone;
            expandedSections["main"] = true;
            expandedSections["settings"] = false;
            InitializeExpandedSections(def);
            zoneName = zone.label;
            originSize = zone.OriginSize;
            captureFloors = zone.CaptureFloors;
            captureTerrain = zone.CaptureTerrain;
            captureThings = zone.CaptureThings;
        }



        private void InitializeExpandedSections(StructureLayoutDef def)
        {
            for (int i = 0; i < def.stages.Count; i++)
            {
                string stageKey = GetStageKey(i);
                expandedSections[stageKey] = false;

                foreach (string category in GetCategoryNames())
                {
                    expandedSections[$"{stageKey}_{category}"] = false;
                }
            }
        }

        private void LoadStructureLayoutDef(StructureLayoutDef sourceDef)
        {
            StructureLayoutDef copiedDef = sourceDef.DeepCopy();
            structureDef = copiedDef;
            StructureBuilder.Instance.SetStructure(copiedDef);
            InitializeExpandedSections(copiedDef);
        }



        public override void DoWindowContents(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, new Color(0.1f, 0.1f, 0.1f));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;


            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, TITLE_HEIGHT);
            RowLayoutManager titleRow = new RowLayoutManager(titleRect);

            Rect titleLabelRect = titleRow.NextRect(inRect.width);
            Widgets.DrawBoxSolid(titleLabelRect, new Color(0.2f, 0.2f, 0.2f));
            Widgets.Label(titleLabelRect, "Structure Editor");


            Texture2D chosenTex = viewXml ? TexButton.CategorizedResourceReadout : TexButton.ShowLearningHelper;
            Rect toggleViewRect = new Rect(
                inRect.xMax - ICON_BUTTON_SIZE - STANDARD_PADDING,
                titleLabelRect.center.y - (ICON_BUTTON_SIZE / 2),
                ICON_BUTTON_SIZE,
                ICON_BUTTON_SIZE
            );

            if (Widgets.ButtonImage(toggleViewRect, chosenTex, true, viewXml ? "Tree View" : "View XML"))
            {
                viewXml = !viewXml;
            }

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            float settingsHeight = DrawSettingsSection(inRect, titleRect.yMax + STANDARD_SPACING);

            Rect contentRect = new Rect(
                inRect.x,
                titleRect.yMax + STANDARD_SPACING + settingsHeight,
                inRect.width,
                inRect.height - TITLE_HEIGHT - settingsHeight - BUTTON_ROW_HEIGHT - (STANDARD_SPACING * 2)
            );

            Rect buttonRowRect = new Rect(
                inRect.x,
                contentRect.yMax + STANDARD_SPACING,
                inRect.width,
                BUTTON_ROW_HEIGHT
            );

            DrawContent(contentRect);
            DrawButtons(buttonRowRect);
            DrawCopySuccessMessage(buttonRowRect);
        }

        private float DrawSettingsSection(Rect inRect, float startY)
        {
            bool settingsExpanded = GetExpandState("settings");

            // Draw settings header
            Rect settingsHeaderRect = new Rect(
                inRect.x + STANDARD_MARGIN,
                startY,
                inRect.width - (STANDARD_MARGIN * 2),
                SECTION_HEADER_HEIGHT
            );
            Widgets.DrawHighlight(settingsHeaderRect);

            if (Widgets.ButtonInvisible(settingsHeaderRect))
            {
                expandedSections["settings"] = !settingsExpanded;
            }

            // Draw expander and label
            Widgets.Label(
                new Rect(settingsHeaderRect.x + EXPANDER_WIDTH, settingsHeaderRect.y + STANDARD_PADDING, LABEL_WIDTH, STANDARD_LINE_HEIGHT),
                "Capture Settings"
            );
            Widgets.Label(
                new Rect(settingsHeaderRect.x + STANDARD_PADDING, settingsHeaderRect.y + STANDARD_PADDING, EXPANDER_WIDTH, EXPANDER_HEIGHT),
                settingsExpanded ? "-" : "+"
            );

            float totalHeight = SECTION_HEADER_HEIGHT + STANDARD_SPACING;

            if (settingsExpanded)
            {
                // Settings content panel
                Rect settingsRect = new Rect(
                    inRect.x + (STANDARD_MARGIN * 2),
                    startY + SECTION_HEADER_HEIGHT + STANDARD_SPACING,
                    inRect.width - (STANDARD_MARGIN * 4),
                    SETTINGS_PANEL_HEIGHT
                );
                Widgets.DrawBoxSolid(settingsRect, sectionBgColor);

                float contentX = settingsRect.x + STANDARD_MARGIN;
                float contentWidth = settingsRect.width - (STANDARD_MARGIN * 2);
                float fieldHeight = STANDARD_LINE_HEIGHT + STANDARD_PADDING;
                float currentY = settingsRect.y + STANDARD_MARGIN;

                // Zone name

                // Origin size
                Rect originSizeRect = new Rect(contentX, currentY, contentWidth, fieldHeight);
                Widgets.Label(originSizeRect.LeftHalf(), "Origin Building size (X, Z):");

                float numFieldWidth = contentWidth * 0.2f;
                Rect xRect = new Rect(contentX + (contentWidth * 0.5f), currentY, numFieldWidth, fieldHeight);
                Rect zRect = new Rect(contentX + (contentWidth * 0.75f), currentY, numFieldWidth, fieldHeight);

                string xBuffer = originSize.x.ToString();
                Widgets.TextFieldNumeric(xRect, ref originSize.x, ref xBuffer, 1, 50);

                string zBuffer = originSize.z.ToString();
                Widgets.TextFieldNumeric(zRect, ref originSize.z, ref zBuffer, 1, 50);
                currentY += fieldHeight + STANDARD_SPACING;


                Widgets.CheckboxLabeled(
                    new Rect(contentX, currentY, contentWidth, fieldHeight),
                    "Capture floors",
                    ref captureFloors
                );

                currentY += fieldHeight + STANDARD_SPACING;

                Widgets.CheckboxLabeled(
                    new Rect(contentX, currentY, contentWidth, fieldHeight),
                    "Capture terrain",
                    ref captureTerrain
                );
                currentY += fieldHeight + STANDARD_SPACING;

                Widgets.CheckboxLabeled(
                    new Rect(contentX, currentY, contentWidth, fieldHeight),
                    "Capture things (buildings, furniture, etc.)",
                    ref captureThings
                );
                currentY += fieldHeight + STANDARD_SPACING;


                Rect saveButtonRect = new Rect(
                    settingsRect.xMax - BUTTON_WIDTH - STANDARD_MARGIN,
                    currentY,
                    BUTTON_WIDTH - STANDARD_MARGIN,
                    BUTTON_HEIGHT
                );
                if (Widgets.ButtonText(saveButtonRect, "Apply"))
                {
                    SaveSettings();
                }

                totalHeight += SETTINGS_PANEL_HEIGHT + STANDARD_SPACING;
            }

            return totalHeight;
        }

        private void SaveSettings()
        {
            zone.label = zoneName;
            zone.OriginSize = originSize;
            zone.CaptureFloors = captureFloors;
            zone.CaptureTerrain = captureTerrain;
            zone.CaptureThings = captureThings;
        }

        private void DrawContent(Rect contentRect)
        {
            if (viewXml)
            {
                DrawXmlView(contentRect);
            }
            else
            {
                DrawTreeView(contentRect);
            }
        }

        private void DrawCopySuccessMessage(Rect buttonRowRect)
        {
            if (copySuccessful && copyMessageTimer > 0)
            {
                Rect messageRect = new Rect(
                    STANDARD_MARGIN,
                    buttonRowRect.y + STANDARD_PADDING,
                    150f,
                    BUTTON_HEIGHT
                );

                GUI.color = Color.green;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(messageRect, "Copied to clipboard!");
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private void DrawButtons(Rect rect)
        {
            float availableWidth = rect.width - (STANDARD_MARGIN * 2);
            int buttonCount = 6;
            float buttonWidth = Mathf.Min(
                BUTTON_WIDTH,
                (availableWidth - (STANDARD_SPACING * (buttonCount - 1))) / buttonCount
            );

            CellRect cellRect = CellRect.FromCellList(zone.cells);

            RowLayoutManager toolbarButtonRow = new RowLayoutManager(
                new Rect(rect.x, rect.y, rect.width, BUTTON_ROW_HEIGHT),
                STANDARD_SPACING
            );


            Rect newStructure = toolbarButtonRow.NextRect(buttonWidth, 10);
            TooltipHandler.TipRegion(newStructure, $"Start a new StructureDef for export.");
            if (Widgets.ButtonText(newStructure, "New Structure"))
            {
                StructureBuilder.Instance.Reset();
                this.structureDef = StructureBuilder.Instance.GetStructure();
                return;
            }



            Rect newStage = toolbarButtonRow.NextRect(buttonWidth, 10);
            TooltipHandler.TipRegion(newStage, $"Add a new BuildStage.");
            if (Widgets.ButtonText(newStage, "New Stage"))
            {
                StructureBuilder.Instance.AddStage();
                int newStageIndex = structureDef.stages.Count - 1;
                string newStageKey = GetStageKey(newStageIndex);
                expandedSections[newStageKey] = true;

                foreach (string category in GetCategoryNames())
                {
                    expandedSections[$"{newStageKey}_{category}"] = false;
                }
            }

            Rect captureButtonRect = toolbarButtonRow.NextRect(buttonWidth + 30, 10);

            TooltipHandler.TipRegion(captureButtonRect, $"Captures all valid things in area to current stage data.");

            if (Widgets.ButtonText(captureButtonRect, $"Zone to Stage"))
            {
                // Add if no current stage exists
                if (StructureBuilder.Instance.GetCurrentStage() == null)
                {
                    StructureBuilder.Instance.AddStage();
                    int newStageIndex = structureDef.stages.Count - 1;
                    string newStageKey = GetStageKey(newStageIndex);
                    expandedSections[newStageKey] = true;
                    foreach (string category in GetCategoryNames())
                    {
                        expandedSections[$"{newStageKey}_{category}"] = false;
                    }
                }

                // Capture zone rect

                StructureBuilder.Instance.CaptureAreaToCurrentStage(zone.Map, cellRect, cellRect.CenterCell, null, (TerrainDef terrain) =>
                {
                    if (terrain.natural && !zone.CaptureTerrain)
                    {
                        return false;
                    }
                    return true;
                });
            }

            Rect buildButton = toolbarButtonRow.NextRect(buttonWidth, 5);

            TooltipHandler.TipRegion(buildButton, $"Build's a stage from a selected StructureLayoutDef into the zone.");
            if (Widgets.ButtonText(buildButton, "Build Stage"))
            {
                List<FloatMenuOption> defOptions = new List<FloatMenuOption>();
                foreach (var item in layoutDefs)
                {
                    defOptions.Add(new FloatMenuOption(item.defName, () =>
                    {
                        List<FloatMenuOption> stageOptions = new List<FloatMenuOption>();
                        for (int i = 0; i < item.stages.Count; i++)
                        {
                            StructureLayoutDef capturedDef = item;
                            int stageIndex = i;
                            stageOptions.Add(new FloatMenuOption($"Stage {i}", () =>
                            {
                                StructureBuilder.BuildStructure(capturedDef, cellRect.CenterCell, stageIndex, this.zone.Map, Faction.OfPlayer);
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(stageOptions));
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(defOptions));
            }


            Rect loadDefButton = toolbarButtonRow.NextRect(buttonWidth, 5);
            TooltipHandler.TipRegion(loadDefButton, $"Load an existing StructureLayoutDef into the editor.");
            if (Widgets.ButtonText(loadDefButton, "Load Def"))
            {
                List<FloatMenuOption> defOptions = new List<FloatMenuOption>();
                foreach (var def in layoutDefs)
                {
                    StructureLayoutDef capturedDef = def;
                    defOptions.Add(new FloatMenuOption(def.defName, () =>
                    {
                        LoadStructureLayoutDef(capturedDef);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(defOptions));
            }

            Rect copyButton = toolbarButtonRow.NextRect(buttonWidth, 5);
            TooltipHandler.TipRegion(copyButton, $"Copy StructureLayoutDef XML to clipboard.");
            if (Widgets.ButtonText(copyButton, "Copy XML"))
            {
                GUIUtility.systemCopyBuffer = StructureBuilder.Instance.ToXmlString();
                copySuccessful = true;
                copyMessageTimer = 3f;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            SaveSettings();
            base.Close(doCloseSound);
        }

        private void DrawXmlView(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.15f));

            string xmlContent = StructureBuilder.Instance.ToXmlString();
            float textHeight = Math.Max(
                Text.CalcHeight(xmlContent, rect.width - (STANDARD_MARGIN * 2)),
                500f
            );

            Widgets.BeginScrollView(
                rect,
                ref scrollPosition,
                new Rect(0, 0, rect.width - SCROLL_BAR_WIDTH, textHeight)
            );

            GUI.color = new Color(0.9f, 0.9f, 0.9f);
            Rect textAreaRect = new Rect(
                STANDARD_PADDING,
                STANDARD_PADDING,
                rect.width - SCROLL_BAR_WIDTH - STANDARD_PADDING,
                textHeight
            );
            Widgets.TextArea(textAreaRect, xmlContent, true);
            GUI.color = Color.white;

            Widgets.EndScrollView();
        }


        private void DrawTreeView(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.15f, 0.15f, 0.15f));

            Rect viewRect = new Rect(0, 0, rect.width - SCROLL_BAR_WIDTH, CalculateTreeViewHeight());

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float curY = STANDARD_MARGIN;

            // Structure name field
            Rect structureHeaderRect = new Rect(STANDARD_MARGIN, curY, viewRect.width - (STANDARD_MARGIN * 2), SECTION_HEADER_HEIGHT);
            Widgets.DrawBoxSolid(structureHeaderRect, new Color(0.25f, 0.25f, 0.25f));
            Text.Font = GameFont.Medium;
            GUI.color = headerColor;
            structureDef.defName = Widgets.TextField(
                new Rect(STANDARD_MARGIN * 2, curY + STANDARD_PADDING, viewRect.width - (STANDARD_MARGIN * 4), SECTION_HEADER_HEIGHT),
                structureDef.defName
            );
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            curY += SECTION_HEADER_HEIGHT + STANDARD_SPACING;

            // Draw Stages section
            bool stagesExpanded = GetExpandState("main");
            Rect stagesSectionRect = new Rect(STANDARD_MARGIN, curY, viewRect.width - (STANDARD_MARGIN * 2), SECTION_HEADER_HEIGHT);
            Widgets.DrawHighlight(stagesSectionRect);

            if (Widgets.ButtonInvisible(stagesSectionRect))
            {
                expandedSections["main"] = !stagesExpanded;
            }

            string stagesLabel = $"Stages ({structureDef.stages.Count})";

            Widgets.Label(
                new Rect(stagesSectionRect.x + EXPANDER_WIDTH + STANDARD_PADDING,
                         curY + STANDARD_PADDING,
                         STAGE_LABEL_HEIGHT,
                         STANDARD_LINE_HEIGHT),
                stagesLabel
            );
            Widgets.Label(
                new Rect(stagesSectionRect.x + STANDARD_PADDING,
                         curY + STANDARD_PADDING,
                         EXPANDER_WIDTH,
                         EXPANDER_HEIGHT),
                stagesExpanded ? "-" : "+"
            );
            curY += SECTION_HEADER_HEIGHT + STANDARD_SPACING;

            if (stagesExpanded)
            {
                for (int i = 0; i < structureDef.stages.Count; i++)
                {
                    curY = DrawStage(structureDef.stages[i], i, curY, viewRect.width);
                }
            }

            Widgets.EndScrollView();
        }

        private float DrawStage(BuildingStage stage, int stageIndex, float startY, float width)
        {
            float curY = startY;
            string stageKey = GetStageKey(stageIndex);
            bool stageExpanded = GetExpandState(stageKey);

            // Stage header
            float stageIndent = CONTENT_INDENT * 3;
            Rect stageHeaderRect = new Rect(
                stageIndent,
                curY,
                width - (stageIndent * 2),
                STANDARD_LINE_HEIGHT + STANDARD_PADDING
            );
            Widgets.DrawLightHighlight(stageHeaderRect);

            // Expander and label
            Widgets.Label(
                new Rect(stageIndent + STANDARD_PADDING, curY + STANDARD_PADDING, EXPANDER_WIDTH, EXPANDER_HEIGHT),
                stageExpanded ? "-" : "+"
            );
            Widgets.Label(
                new Rect(stageIndent + EXPANDER_WIDTH + STANDARD_PADDING, curY + STANDARD_PADDING, LABEL_WIDTH, STANDARD_LINE_HEIGHT),
                $"Stage {stageIndex + 1}"
            );

            // Delete button for stages (except when there's only one)
            if (structureDef.stages.Count > 1)
            {
                Rect deleteRect = new Rect(
                    width - BUTTON_WIDTH + STANDARD_MARGIN,
                    curY,
                    BUTTON_WIDTH - (STANDARD_MARGIN * 2),
                    STANDARD_LINE_HEIGHT + STANDARD_PADDING
                );
                if (Widgets.ButtonText(deleteRect, "Delete"))
                {
                    Dialog_MessageBox.CreateConfirmation(
                        "Delete this stage?",
                        confirmedAct: () => {
                            structureDef.stages.RemoveAt(stageIndex);
                        },
                        null,
                        false,
                        "Confirm Deletion"
                    );
                }
            }

            // Make the entire header clickable
            if (Widgets.ButtonInvisible(stageHeaderRect))
            {
                expandedSections[stageKey] = !stageExpanded;
            }
            curY += stageHeaderRect.height + STANDARD_SPACING;

            if (stageExpanded)
            {
                // Stage info section
                float stageContentIndent = stageIndent + CONTENT_INDENT;
                Rect infoRect = new Rect(
                    stageContentIndent,
                    curY,
                    width - (stageContentIndent * 2),
                    STANDARD_LINE_HEIGHT + STANDARD_PADDING
                );
                Widgets.DrawBoxSolid(infoRect, sectionBgColor);

                Widgets.Label(
                    new Rect(stageContentIndent + STANDARD_MARGIN, curY + STANDARD_PADDING, 50f, STANDARD_LINE_HEIGHT),
                    "Size:"
                );
                Widgets.Label(
                    new Rect(stageContentIndent + STANDARD_MARGIN + 50f, curY + STANDARD_PADDING, 200f, STANDARD_LINE_HEIGHT),
                    $"{stage.size.x}x{stage.size.z}"
                );
                curY += infoRect.height + STANDARD_SPACING;

                // Draw all category sections
                string[] categories = GetCategoryNames();
                string[] displayNames = { "Terrain", "Walls", "Doors", "Power", "Furniture", "Other" };

                for (int i = 0; i < categories.Length; i++)
                {
                    int count = 0;
                    switch (categories[i])
                    {
                        case "terrain": count = stage.terrain.Count; break;
                        case "walls": count = stage.walls.Count; break;
                        case "doors": count = stage.doors.Count; break;
                        case "power": count = stage.power.Count; break;
                        case "furniture": count = stage.furniture.Count; break;
                        case "other": count = stage.other.Count; break;
                    }

                    DrawCategoryWithItems(stage, stageIndex, categories[i], displayNames[i], count, ref curY, width);
                }
            }

            return curY + STANDARD_SPACING;
        }

        private void DrawCategoryWithItems(BuildingStage stage, int stageIndex, string category, string displayName, int count,
                                        ref float curY, float width)
        {
            string stageKey = GetStageKey(stageIndex);
            string categoryKey = $"{stageKey}_{category}";
            bool expanded = GetExpandState(categoryKey);

            // Calculate indents and positions
            float categoryIndent = CONTENT_INDENT * 5;
            float categoryWidth = width - (categoryIndent * 2);

            // Category header
            Rect headerRect = new Rect(categoryIndent, curY, categoryWidth, CATEGORY_HEADER_HEIGHT);
            Widgets.DrawOptionBackground(headerRect, false);

            // Clear category button
            Rect buttonRect = new Rect(
                headerRect.xMax + STANDARD_SPACING,
                curY,
                SMALL_BUTTON_SIZE * 2.5f,
                CATEGORY_HEADER_HEIGHT
            );
            if (Widgets.ButtonImage(buttonRect, TexButton.Delete, true, $"Clear Category"))
            {
                // Clear the category
                switch (category)
                {
                    case "terrain": stage.terrain.Clear(); break;
                    case "walls": stage.walls.Clear(); break;
                    case "doors": stage.doors.Clear(); break;
                    case "power": stage.power.Clear(); break;
                    case "furniture": stage.furniture.Clear(); break;
                    case "other": stage.other.Clear(); break;
                }
            }

            // Make header clickable
            if (Widgets.ButtonInvisible(headerRect))
            {
                expandedSections[categoryKey] = !expanded;
            }

            // Draw expander and label
            Widgets.Label(
                new Rect(categoryIndent + STANDARD_PADDING, curY + STANDARD_PADDING, EXPANDER_WIDTH, EXPANDER_HEIGHT),
                expanded ? "-" : "+"
            );
            Widgets.Label(
                new Rect(categoryIndent + EXPANDER_WIDTH + STANDARD_PADDING, curY + STANDARD_PADDING, width - categoryIndent - 160f, STANDARD_LINE_HEIGHT),
                $"{displayName} ({count})"
            );
            curY += CATEGORY_SECTION_HEIGHT;

            if (expanded)
            {
                float contentIndent = categoryIndent + CONTENT_INDENT;
                float contentWidth = width - (contentIndent * 2);

                if (count > 0)
                {
                    // Background for list items
                    float listHeight = count * LIST_ITEM_HEIGHT + (STANDARD_PADDING * 2);
                    Rect contentBg = new Rect(contentIndent, curY, contentWidth, listHeight);
                    Widgets.DrawBoxSolid(contentBg, new Color(0.18f, 0.18f, 0.18f, 0.3f));
                    curY += STANDARD_PADDING;

                    if (category == "terrain")
                    {
                        for (int i = 0; i < stage.terrain.Count; i++)
                        {
                            TerrainPlacement terrain = stage.terrain[i];
                            Rect listItemRect = new Rect(
                                contentIndent + STANDARD_PADDING,
                                curY,
                                contentWidth - (STANDARD_PADDING * 2),
                                LIST_ITEM_HEIGHT
                            );

                            if (DrawTerrainListItem(listItemRect, terrain, category, i))
                            {
                                stage.terrain.RemoveAt(i);
                                break;
                            }

                            curY += LIST_ITEM_HEIGHT;
                        }
                    }
                    else
                    {
                        List<ThingPlacement> things = null;

                        switch (category)
                        {
                            case "walls": things = stage.walls; break;
                            case "doors": things = stage.doors; break;
                            case "power": things = stage.power; break;
                            case "furniture": things = stage.furniture; break;
                            case "other": things = stage.other; break;
                        }

                        if (things != null)
                        {
                            for (int i = 0; i < things.Count; i++)
                            {
                                ThingPlacement thing = things[i];
                                Rect listItemRect = new Rect(
                                    contentIndent + STANDARD_PADDING,
                                    curY,
                                    contentWidth - (STANDARD_PADDING * 2),
                                    LIST_ITEM_HEIGHT
                                );

                                if (DrawThingListItem(listItemRect, thing, category, i))
                                {
                                    things.RemoveAt(i);
                                    break;
                                }

                                curY += LIST_ITEM_HEIGHT;
                            }
                        }
                    }

                    curY += STANDARD_PADDING;
                }
                else
                {
                    // Empty category display
                    float emptyHeight = LIST_ITEM_HEIGHT + (STANDARD_PADDING * 2);
                    Rect noneRect = new Rect(contentIndent, curY, contentWidth, emptyHeight);
                    Widgets.DrawBoxSolid(noneRect, new Color(0.18f, 0.18f, 0.18f, 0.3f));

                    GUI.color = new Color(0.7f, 0.7f, 0.7f);
                    Widgets.Label(
                        new Rect(contentIndent + STANDARD_MARGIN, curY + STANDARD_PADDING, contentWidth - (STANDARD_MARGIN * 2), LIST_ITEM_HEIGHT),
                        "(none)"
                    );
                    GUI.color = Color.white;
                    curY += emptyHeight;
                }
            }

            curY += STANDARD_SPACING;
        }

        private bool DrawThingListItem(Rect listItemRect, ThingPlacement thing, string category, int index)
        {
            bool removed = false;
            Widgets.DrawBoxSolid(listItemRect, new Color(0.1f, 0.1f, 0.1f));

            // Proportions for the different sections
            float nameRatio = 0.25f;
            float categoryRatio = 0.2f;
            float detailsRatio = 0.45f;

            // Calculate positions
            float currentX = listItemRect.x + STANDARD_PADDING;

            // 1. Label first
            float labelWidth = listItemRect.width * nameRatio;
            GUI.color = new Color(1f, 0.9f, 0.5f);
            Widgets.Label(
                new Rect(currentX, listItemRect.y, labelWidth, listItemRect.height),
                thing.thing.LabelCap
            );

            // 2. Thing category
            currentX += labelWidth + STANDARD_PADDING;
            float categoryWidth = listItemRect.width * categoryRatio;
            GUI.color = new Color(0.7f, 0.7f, 1f);
            Widgets.Label(
                new Rect(currentX, listItemRect.y, categoryWidth, listItemRect.height),
                thing.thing.category.ToString()
            );

            // 3. Details
            currentX += categoryWidth + STANDARD_PADDING;
            float detailsWidth = listItemRect.width * detailsRatio - STANDARD_PADDING;
            GUI.color = new Color(0.85f, 0.85f, 0.85f);

            string stuffInfo = thing.stuff != null ? $"Stuff (<color=yellow>{thing.stuff.LabelCap})</color>" : "";
            string rotInfo = thing.rotation != Rot4.North ? $"[{thing.rotation}]" : "";
            string desc = $"{stuffInfo} Pos <color=cyan>({thing.position.x}, {thing.position.z})</color>".Trim();

            Widgets.Label(
                new Rect(currentX, listItemRect.y, detailsWidth, listItemRect.height),
                desc
            );

            // Delete button
            Rect deleteButtonRect = new Rect(
                listItemRect.xMax - SMALL_BUTTON_SIZE - STANDARD_PADDING,
                listItemRect.y + (listItemRect.height - SMALL_BUTTON_SIZE) / 2,
                SMALL_BUTTON_SIZE,
                SMALL_BUTTON_SIZE
            );

            GUI.color = Color.white;
            if (Widgets.ButtonImage(deleteButtonRect, TexButton.Delete))
            {
                removed = true;
            }

            // Separator line
            GUI.color = Color.white * 0.4f;
            Widgets.DrawLineHorizontal(listItemRect.x, listItemRect.yMax, listItemRect.width);
            GUI.color = Color.white;

            return removed;
        }


        private bool DrawTerrainListItem(Rect listItemRect, TerrainPlacement terrain, string category, int index)
        {
            bool removed = false;
            Widgets.DrawBoxSolid(listItemRect, new Color(0.1f, 0.1f, 0.1f));

            // Proportions for the different sections
            float nameRatio = 0.25f;
            float categoryRatio = 0.2f;
            float detailsRatio = 0.45f;

            // Calculate positions
            float currentX = listItemRect.x + STANDARD_PADDING;

            // 1. Terrain name
            float labelWidth = listItemRect.width * nameRatio;
            GUI.color = new Color(1f, 0.9f, 0.5f);
            Widgets.Label(
                new Rect(currentX, listItemRect.y, labelWidth, listItemRect.height),
                terrain.terrain.LabelCap
            );

            // 2. Category
            currentX += labelWidth + STANDARD_PADDING;
            float categoryWidth = listItemRect.width * categoryRatio;
            GUI.color = new Color(0.7f, 0.7f, 1f);
            Widgets.Label(
                new Rect(currentX, listItemRect.y, categoryWidth, listItemRect.height),
                category
            );

            // 3. Position details
            currentX += categoryWidth + STANDARD_PADDING;
            float detailsWidth = listItemRect.width * detailsRatio - STANDARD_PADDING;
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            Widgets.Label(
                new Rect(currentX, listItemRect.y, detailsWidth, listItemRect.height),
                $"at ({terrain.position.x}, {terrain.position.z})"
            );

            // Delete button
            Rect deleteButtonRect = new Rect(
                listItemRect.xMax - SMALL_BUTTON_SIZE - STANDARD_PADDING,
                listItemRect.y + (listItemRect.height - SMALL_BUTTON_SIZE) / 2,
                SMALL_BUTTON_SIZE,
                SMALL_BUTTON_SIZE
            );

            GUI.color = Color.white;
            if (Widgets.ButtonImage(deleteButtonRect, TexButton.Delete))
            {
                removed = true;
            }

            // Separator line
            GUI.color = Color.white * 0.4f;
            Widgets.DrawLineHorizontal(listItemRect.x, listItemRect.yMax, listItemRect.width);
            GUI.color = Color.white;

            return removed;
        }

        private bool GetExpandState(string key)
        {
            if (expandedSections.TryGetValue(key, out bool value))
            {
                return value;
            }
            expandedSections[key] = false;
            return false;
        }

        private string GetStageKey(int index)
        {
            return "stage_" + index;
        }

        private string[] GetCategoryNames()
        {
            return new[] { "terrain", "walls", "doors", "power", "furniture", "other" };
        }

        private float CalculateTreeViewHeight()
        {
            float height = 120f;

            if (GetExpandState("settings"))
            {
                height += SETTINGS_PANEL_HEIGHT + SECTION_HEADER_HEIGHT + STANDARD_SPACING;
            }
            else
            {
                height += SECTION_HEADER_HEIGHT + STANDARD_SPACING;
            }

            height += SECTION_HEADER_HEIGHT + STANDARD_SPACING;

            if (GetExpandState("main"))
            {
                for (int i = 0; i < structureDef.stages.Count; i++)
                {
                    BuildingStage stage = structureDef.stages[i];
                    string stageKey = GetStageKey(i);

                    height += STAGE_HEADER_HEIGHT + STANDARD_SPACING;

                    if (GetExpandState(stageKey))
                    {
                        height += SIZE_HEADER_HEIGHT + STANDARD_SPACING;

                        foreach (string category in GetCategoryNames())
                        {
                            height += CATEGORY_HEADER_HEIGHT + STANDARD_SPACING;

                            if (GetExpandState($"{stageKey}_{category}"))
                            {
                                int count = 0;
                                switch (category)
                                {
                                    case "terrain": count = stage.terrain.Count; break;
                                    case "walls": count = stage.walls.Count; break;
                                    case "doors": count = stage.doors.Count; break;
                                    case "power": count = stage.power.Count; break;
                                    case "furniture": count = stage.furniture.Count; break;
                                    case "other": count = stage.other.Count; break;
                                }

                                if (count == 0)
                                {
                                    height += LIST_ITEM_HEIGHT + (STANDARD_PADDING * 4);
                                }
                                else
                                {
                                    height += (count * LIST_ITEM_HEIGHT) + (STANDARD_PADDING * 4);
                                }
                            }
                        }
                    }
                }
            }

            return Math.Max(MAX_HEIGHT, height);
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if (copyMessageTimer > 0)
            {
                copyMessageTimer -= Time.deltaTime;
            }
        }
    }
}
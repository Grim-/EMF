using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace EMF
{
    /// <summary>
    /// A generic window that displays a grid of selectable items grouped by category.
    /// T is the type of object being selected (e.g., WishDef, AdvancementNode, etc.)
    /// </summary>
    public class Window_GridSelection<T> : Window
    {
        public string Title { get; set; }
        public Func<T, string> GetCategory { get; set; } = _ => "Miscellaneous";
        public Func<T, string> GetLabel { get; set; } = x => x.ToString();
        public Func<T, Texture2D> GetIcon { get; set; } = _ => null;
        public Func<T, string> GetTooltip { get; set; } = _ => "";
        public Func<T, int?> GetCost { get; set; } = _ => null;
        public Func<T, bool> CanSelect { get; set; } = _ => true;

        // Actions
        public Action<T> OnSelect { get; set; } = _ => { };
        public Action OnCancel { get; set; } = () => { }; // Triggered if closed without selection

        // UI Config
        public Func<string> GetHeaderInfo { get; set; }
        public bool ShowCancelButton { get; set; } = true;
        public string CancelButtonLabel { get; set; } = "Delay Advancement";

        // -- State --
        private readonly List<T> items;
        private Dictionary<string, List<T>> categorizedItems;
        private Vector2 scrollPosition = Vector2.zero;
        private bool selectionMade = false; // Tracks if a choice was successfully made

        // -- Layout Constants --
        private static readonly Vector2 WinSize = new Vector2(800f, 600f);
        private const float BUTTON_SIZE = 120f;
        private const float BUTTON_PADDING = 10f;
        private const float SECTION_PADDING = 20f;
        private const float CATEGORY_HEADER_HEIGHT = 30f;
        private const float LABEL_HEIGHT = 40f;
        private const float TITLE_HEIGHT = 35f;
        private const float INNER_PADDING = 5f;
        private const float BOTTOM_BAR_HEIGHT = 50f; // New reserved space

        // -- Colors --
        private static readonly Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private static readonly Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private static readonly Color CATEGORY_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        public override Vector2 InitialSize => WinSize;

        public Window_GridSelection(string title, IEnumerable<T> items)
        {
            this.Title = title;
            this.items = items.ToList();

            this.doCloseX = true;
            this.doCloseButton = false; // We draw our own cancel button if needed
            this.absorbInputAroundWindow = true;
            this.forcePause = true;

            CacheCategories();
        }

        private void CacheCategories()
        {
            categorizedItems = new Dictionary<string, List<T>>();
            foreach (var item in items)
            {
                string cat = GetCategory(item) ?? "Miscellaneous";
                if (!categorizedItems.ContainsKey(cat))
                {
                    categorizedItems[cat] = new List<T>();
                }
                categorizedItems[cat].Add(item);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle(inRect);

            // Calculate Grid Height
            float contentHeight = 0f;
            int itemsPerRow = Mathf.FloorToInt((inRect.width - 16f) / (BUTTON_SIZE + BUTTON_PADDING));

            foreach (var category in categorizedItems)
            {
                if (category.Value.Count == 0) continue;
                contentHeight += CATEGORY_HEADER_HEIGHT;
                contentHeight += Mathf.Ceil(category.Value.Count / (float)itemsPerRow) * (BUTTON_SIZE + BUTTON_PADDING);
                contentHeight += SECTION_PADDING;
            }

            // Reserve space for Bottom Bar if enabled
            float bottomMargin = ShowCancelButton ? BOTTOM_BAR_HEIGHT : 0f;

            // Adjust ScrollView to fit between Title and Bottom Bar
            Rect outRect = new Rect(0f, TITLE_HEIGHT + 5f, inRect.width, inRect.height - TITLE_HEIGHT - 5f - bottomMargin);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, contentHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (var category in categorizedItems)
            {
                if (category.Value.Count == 0) continue;

                Text.Font = GameFont.Medium;
                Rect headerRect = new Rect(0f, curY, viewRect.width, CATEGORY_HEADER_HEIGHT);
                GUI.DrawTexture(headerRect, SolidColorMaterials.NewSolidColorTexture(CATEGORY_COLOR));
                Widgets.Label(headerRect, category.Key);
                curY += CATEGORY_HEADER_HEIGHT + 5f;

                float curX = 0f;
                foreach (T item in category.Value)
                {
                    if (curX + BUTTON_SIZE > viewRect.width)
                    {
                        curX = 0f;
                        curY += BUTTON_SIZE + BUTTON_PADDING;
                    }

                    Rect itemRect = new Rect(curX, curY, BUTTON_SIZE, BUTTON_SIZE);
                    DrawItemButton(itemRect, item);
                    curX += BUTTON_SIZE + BUTTON_PADDING;
                }

                curY += BUTTON_SIZE + SECTION_PADDING;
            }

            Widgets.EndScrollView();

            // Draw Bottom "Cancel/Delay" Button
            if (ShowCancelButton)
            {
                Rect bottomRect = new Rect(0f, inRect.height - BOTTOM_BAR_HEIGHT + 10f, inRect.width, 40f);
                // Center the button
                Rect btnRect = new Rect(bottomRect.center.x - 80f, bottomRect.y, 160f, 30f);

                if (Widgets.ButtonText(btnRect, CancelButtonLabel))
                {
                    // Regular Close() calls PostClose(), which handles the OnCancel event logic
                    Close();
                }
            }
        }

        // Override PostClose to catch Esc key, X button, or Cancel Button
        public override void PostClose()
        {
            base.PostClose();
            if (!selectionMade)
            {
                OnCancel?.Invoke();
            }
        }

        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

            string fullTitle = Title;
            if (GetHeaderInfo != null)
            {
                string info = GetHeaderInfo();
                if (!string.IsNullOrEmpty(info)) fullTitle += $" - {info}";
            }

            Widgets.Label(titleRect, fullTitle);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawItemButton(Rect rect, T item)
        {
            bool enabled = CanSelect(item);

            GUI.color = enabled ? Color.white : Color.gray;
            Widgets.DrawBox(rect, 1, SolidColorMaterials.NewSolidColorTexture(Color.grey));
            Rect innerRect = rect.ContractedBy(INNER_PADDING);

            // Icon
            Rect iconRect = new Rect(innerRect.x, innerRect.y, innerRect.width, innerRect.height - LABEL_HEIGHT);
            Texture2D icon = GetIcon(item);
            if (icon != null) Widgets.DrawTextureFitted(iconRect, icon, 0.8f);

            // Footer
            Rect footerRect = new Rect(rect.x, rect.y + rect.height - LABEL_HEIGHT, rect.width, LABEL_HEIGHT);
            GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(FOOTER_COLOR));

            int? cost = GetCost(item);
            float labelXOffset = 0f;

            if (cost.HasValue)
            {
                float costBoxWidth = LABEL_HEIGHT * 0.7f;
                Rect costBoxRect = new Rect(footerRect.x, footerRect.y, costBoxWidth, costBoxWidth);
                GUI.DrawTexture(costBoxRect, SolidColorMaterials.NewSolidColorTexture(FOOTER_COLOR * 0.8f));
                Widgets.DrawBox(costBoxRect, 1, SolidColorMaterials.NewSolidColorTexture(Color.grey));
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(costBoxRect, cost.Value.ToString());
                labelXOffset = costBoxWidth;
            }

            Rect labelRect = new Rect(footerRect.x + labelXOffset, footerRect.y, footerRect.width - labelXOffset, LABEL_HEIGHT);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, GetLabel(item));

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, GetTooltip(item));

                if (enabled && Widgets.ButtonInvisible(rect))
                {
                    // === CRITICAL CHANGE ===
                    // Mark selection as made BEFORE calling the callback, 
                    // because the callback might close the window immediately.
                    selectionMade = true;
                    OnSelect(item);
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}

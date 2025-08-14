using UnityEngine;
using Verse;

namespace EMF
{
    public static class UIPawnGridLayoutUtility
    {
        public const float CARD_SIZE = 120f;
        public const float CARD_MARGIN = 8f;
        public const float ICON_SIZE = 40f;
        public const float PROGRESS_HEIGHT = 4f;
        public const float BUTTON_HEIGHT = 20f;
        public const float LABEL_HEIGHT = 16f;
        public const float HEADER_HEIGHT = 30f;
        public const float HEADER_OFFSET = 35f;
        public const float CONTENT_MARGIN = 10f;
        public const float CARD_PADDING = 4f;
        public const float CARD_INTERNAL_MARGIN = 8f;
        public const float ICON_TOP_MARGIN = 8f;
        public const float ELEMENT_SPACING = 2f;
        public const float SCROLLBAR_WIDTH = 16f;
        public const float EMPTY_STATE_TOP_MARGIN = 50f;
        public const float EMPTY_STATE_HEIGHT = 30f;
        public const float QUALITY_LABEL_HEIGHT = 12f;
        public const float PROGRESS_BAR_HORIZONTAL_MARGIN = 8f;
        public const float PROGRESS_BAR_TOP_MARGIN = 4f;
        public const int BORDER_WIDTH = 1;

        private static readonly Color ACTIVE_BORDER_COLOR = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        private static readonly Color INACTIVE_BORDER_COLOR = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        private static readonly Color ACTIVE_BG_COLOR = new Color(0.1f, 0.3f, 0.1f, 0.3f);
        private static readonly Color INACTIVE_BG_COLOR = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        private static readonly Color PROGRESS_BG_COLOR = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private static readonly Color RELEASE_BUTTON_COLOR = new Color(1f, 0.7f, 0.7f, 1f);

        public static void DrawHeader(Rect rect, string text, int count)
        {
            Text.Font = GameFont.Medium;
            Rect headerRect = new Rect(rect.x, rect.y, rect.width, HEADER_HEIGHT);
            Widgets.Label(headerRect, $"{text} ({count})");
            Text.Font = GameFont.Small;
        }

        public static void DrawEmptyState(Rect contentRect, string message)
        {
            Rect emptyRect = new Rect(contentRect.x, contentRect.y + EMPTY_STATE_TOP_MARGIN, contentRect.width, EMPTY_STATE_HEIGHT);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.gray;
            Widgets.Label(emptyRect, message);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void DrawCard(Rect cardRect, bool isActive, System.Action drawContent)
        {
            DrawCardBackground(cardRect, isActive);
            drawContent?.Invoke();
        }

        public static void DrawCardBackground(Rect cardRect, bool isActive)
        {
            Color bgColor = isActive ? ACTIVE_BG_COLOR : INACTIVE_BG_COLOR;
            Widgets.DrawBoxSolid(cardRect, bgColor);
            Widgets.DrawBox(cardRect, BORDER_WIDTH);
        }

        public static void DrawCardIcon(Rect cardRect, Thing thing, System.Action onIconClick = null)
        {
            Rect iconRect = new Rect(
                cardRect.x + (cardRect.width - ICON_SIZE) / 2f,
                cardRect.y + ICON_TOP_MARGIN,
                ICON_SIZE,
                ICON_SIZE
            );

            Widgets.ThingIcon(iconRect, thing);

            if (Mouse.IsOver(iconRect))
            {
                Widgets.DrawHighlight(iconRect);
                if (onIconClick != null && Widgets.ButtonInvisible(iconRect))
                {
                    onIconClick();
                }
            }
        }

        public static void DrawCardLabel(Rect cardRect, float yPosition, string text)
        {
            Rect nameRect = new Rect(
                cardRect.x + CARD_PADDING,
                yPosition,
                cardRect.width - (CARD_PADDING * 2),
                LABEL_HEIGHT
            );

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            string truncatedText = text.Truncate(nameRect.width);
            Widgets.Label(nameRect, truncatedText);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public static void DrawProgressBar(Rect cardRect, float yPosition, float progress, string label = null)
        {
            Rect progressBgRect = new Rect(
                cardRect.x + PROGRESS_BAR_HORIZONTAL_MARGIN,
                yPosition + PROGRESS_BAR_TOP_MARGIN,
                cardRect.width - (PROGRESS_BAR_HORIZONTAL_MARGIN * 2),
                PROGRESS_HEIGHT
            );

            Rect progressRect = new Rect(
                progressBgRect.x,
                progressBgRect.y,
                progressBgRect.width * progress,
                progressBgRect.height
            );

            Widgets.DrawBoxSolid(progressBgRect, PROGRESS_BG_COLOR);
            Color progressColor = Color.Lerp(Color.red, Color.green, progress);
            Widgets.DrawBoxSolid(progressRect, progressColor);

            if (!string.IsNullOrEmpty(label))
            {
                Rect qualityLabelRect = new Rect(
                    cardRect.x + CARD_PADDING,
                    progressBgRect.yMax + ELEMENT_SPACING,
                    cardRect.width - (CARD_PADDING * 2),
                    QUALITY_LABEL_HEIGHT
                );

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                GUI.color = Color.gray;
                Widgets.Label(qualityLabelRect, label);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
        }

        public static bool DrawCardButton(Rect rect, string text, Color? color = null)
        {
            Text.Font = GameFont.Tiny;

            if (color.HasValue)
            {
                GUI.color = color.Value;
            }

            bool result = Widgets.ButtonText(rect, text);

            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            return result;
        }

        public static void DrawCardButtons(Rect cardRect, string primaryText, System.Action onPrimary, string secondaryText, System.Action onSecondary, bool useReleaseColor = false)
        {
            float buttonY = cardRect.yMax - (BUTTON_HEIGHT * 2) - CARD_INTERNAL_MARGIN;

            Rect primaryButtonRect = new Rect(
                cardRect.x + CARD_PADDING,
                buttonY,
                cardRect.width - (CARD_PADDING * 2),
                BUTTON_HEIGHT
            );

            Rect secondaryButtonRect = new Rect(
                cardRect.x + CARD_PADDING,
                buttonY + BUTTON_HEIGHT + ELEMENT_SPACING,
                cardRect.width - (CARD_PADDING * 2),
                BUTTON_HEIGHT
            );

            if (DrawCardButton(primaryButtonRect, primaryText))
            {
                onPrimary?.Invoke();
            }

            if (DrawCardButton(secondaryButtonRect, secondaryText, useReleaseColor ? RELEASE_BUTTON_COLOR : (Color?)null))
            {
                onSecondary?.Invoke();
            }
        }

        public static int CalculateGridColumns(float availableWidth)
        {
            int columns = Mathf.FloorToInt((availableWidth - CARD_MARGIN) / (CARD_SIZE + CARD_MARGIN));
            return Mathf.Max(1, columns);
        }

        public static Rect CalculateCardPosition(int index, int columns)
        {
            int column = index % columns;
            int row = index / columns;
            float x = column * (CARD_SIZE + CARD_MARGIN);
            float y = row * (CARD_SIZE + CARD_MARGIN);
            return new Rect(x, y, CARD_SIZE, CARD_SIZE);
        }

        public static float CalculateGridHeight(int itemCount, int columns)
        {
            int rows = Mathf.CeilToInt((float)itemCount / columns);
            return rows * (CARD_SIZE + CARD_MARGIN);
        }

        public static Rect GetContentRect(Rect rect)
        {
            return new Rect(rect.x, rect.y + HEADER_OFFSET, rect.width, rect.height - HEADER_OFFSET);
        }

        public static Rect GetScrollViewRect(Rect contentRect)
        {
            return new Rect(0f, 0f, contentRect.width - SCROLLBAR_WIDTH, contentRect.height);
        }
    }
}

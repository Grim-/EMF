using UnityEngine;
using Verse;

namespace EMF
{
    public static class ColorWidgets
    {
        public static Color ColorField(Rect rect, Color color)
        {
            Rect colorPreviewRect = new Rect(rect.x, rect.y, 30f, rect.height);
            Rect rRect = new Rect(rect.x + 35f, rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect gRect = new Rect(rect.x + 35f + (rect.width - 35f) / 4, rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect bRect = new Rect(rect.x + 35f + 2 * ((rect.width - 35f) / 4), rect.y, (rect.width - 35f) / 4 - 5f, rect.height);
            Rect aRect = new Rect(rect.x + 35f + 3 * ((rect.width - 35f) / 4), rect.y, (rect.width - 35f) / 4 - 5f, rect.height);

            // Draw color preview
            Widgets.DrawBoxSolid(colorPreviewRect, color);

            int rInt = Mathf.RoundToInt(color.r * 255f);
            int gInt = Mathf.RoundToInt(color.g * 255f);
            int bInt = Mathf.RoundToInt(color.b * 255f);
            int aInt = Mathf.RoundToInt(color.a * 255f);

            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Widgets.Label(new Rect(rRect.x, rRect.y - 14f, rRect.width, 14f), "R");
            Widgets.Label(new Rect(gRect.x, gRect.y - 14f, gRect.width, 14f), "G");
            Widgets.Label(new Rect(bRect.x, bRect.y - 14f, bRect.width, 14f), "B");
            Widgets.Label(new Rect(aRect.x, aRect.y - 14f, aRect.width, 14f), "A");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            string rStr = rInt.ToString();
            GUI.SetNextControlName("ColorR");
            rStr = Widgets.TextField(rRect, rStr);
            if (int.TryParse(rStr, out int newR))
            {
                rInt = Mathf.Clamp(newR, 0, 255);
            }

            string gStr = gInt.ToString();
            GUI.SetNextControlName("ColorG");
            gStr = Widgets.TextField(gRect, gStr);
            if (int.TryParse(gStr, out int newG))
            {
                gInt = Mathf.Clamp(newG, 0, 255);
            }

            string bStr = bInt.ToString();
            GUI.SetNextControlName("ColorB");
            bStr = Widgets.TextField(bRect, bStr);
            if (int.TryParse(bStr, out int newB))
            {
                bInt = Mathf.Clamp(newB, 0, 255);
            }

            string aStr = aInt.ToString();
            GUI.SetNextControlName("ColorA");
            aStr = Widgets.TextField(aRect, aStr);
            if (int.TryParse(aStr, out int newA))
            {
                aInt = Mathf.Clamp(newA, 0, 255);
            }

            float r = rInt / 255f;
            float g = gInt / 255f;
            float b = bInt / 255f;
            float a = aInt / 255f;

            if (r != color.r || g != color.g || b != color.b || a != color.a)
            {
                return new Color(r, g, b, a);
            }

            return color;
        }
    }
}

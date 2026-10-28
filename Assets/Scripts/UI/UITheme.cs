using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Palette from Assets/Art/01_Scenario_1/DESIGN_SYSTEM.md, plus semantic tokens. Use the tokens
    /// (Ink, Surface, SurfaceDark, Positive...) in new UI so every screen shares one look.
    /// </summary>
    public static class UITheme
    {
        // ---- palette
        public static readonly Color TealDark = Hex("1B423A");
        public static readonly Color Teal = Hex("2E7D6B");
        public static readonly Color TealLight = Hex("A3D6C8");
        public static readonly Color Cream = Hex("F7F2E7");
        public static readonly Color Sand = Hex("E8DDC4");
        public static readonly Color Amber = Hex("F2A73B");
        public static readonly Color Coral = Hex("D95A4F");
        public static readonly Color LeafGreen = Hex("4CAF7D");
        public static readonly Color Navy = Hex("1F2A44");

        // ---- semantic tokens
        /// <summary>Body text on light surfaces.</summary>
        public static readonly Color Ink = TealDark;
        /// <summary>Secondary text on light surfaces (5.9:1 on white).</summary>
        public static readonly Color InkMuted = Hex("4F6A63");
        public static readonly Color Surface = Color.white;
        public static readonly Color SurfaceWarm = Cream;
        /// <summary>The one dark surface colour: HUD chips, pause panel, name tab, tooltips, pills.</summary>
        public static readonly Color SurfaceDark = Navy;
        /// <summary>Dim behind modal overlays.</summary>
        public static readonly Color Scrim = WithAlpha(TealDark, 0.55f);
        /// <summary>Good outcome (Trust up, Resistance Pressure down). Outcomes only, never spending.</summary>
        public static readonly Color Positive = LeafGreen;
        /// <summary>Bad outcome (Trust down, Resistance Pressure up). Outcomes only, never spending.</summary>
        public static readonly Color Negative = Coral;
        /// <summary>Lighter outcome colours for text on SurfaceDark (at least 6:1).</summary>
        public static readonly Color PositiveOnDark = Hex("7FD6A4");
        public static readonly Color NegativeOnDark = Hex("FF8A80");
        /// <summary>Spending Money, Time or Capacity: neutral, not "bad" (5.7:1 on white).</summary>
        public static readonly Color Spend = Hex("9A5516");
        public static readonly Color SpendOnDark = Hex("F2C27B");
        /// <summary>Keyboard focus and hover ring.</summary>
        public static readonly Color Focus = Amber;
        /// <summary>Disabled control surface and text (4.5:1).</summary>
        public static readonly Color DisabledSurface = Hex("E1E2E6");
        public static readonly Color DisabledInk = Hex("5B6670");

        public static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString("#" + hex, out c);
            return c;
        }

        public static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        public static string ToHex(Color c) { return "#" + ColorUtility.ToHtmlStringRGB(c); }
    }
}

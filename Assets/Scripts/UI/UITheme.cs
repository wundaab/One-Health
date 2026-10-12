using UnityEngine;

namespace OneHealth
{
    /// <summary>Palette from Assets/Art/01_Scenario_1/DESIGN_SYSTEM.md.</summary>
    public static class UITheme
    {
        public static readonly Color TealDark = Hex("1B423A");
        public static readonly Color Teal = Hex("2E7D6B");
        public static readonly Color TealLight = Hex("A3D6C8");
        public static readonly Color Cream = Hex("F7F2E7");
        public static readonly Color Sand = Hex("E8DDC4");
        public static readonly Color Amber = Hex("F2A73B");
        public static readonly Color Coral = Hex("D95A4F");
        public static readonly Color LeafGreen = Hex("4CAF7D");
        public static readonly Color Navy = Hex("1F2A44");

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
    }
}

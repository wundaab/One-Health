using System.IO;
using UnityEditor;
using UnityEngine;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Plain shape sprites drawn in code (circle, rounded rectangle, outline ring, soft shadow,
    /// gradient). They have no baked colour, border or shadow, so one sprite can be tinted to any
    /// design-system colour (pills, name tab, focus rings, portrait circle, contact shadows).
    /// Re-running regenerates the PNGs in place; the asset GUIDs are kept.
    /// </summary>
    public static class GeneratedSprites
    {
        public const string Dir = "Assets/Art/UI_Generated/";
        public const string RoundFill = Dir + "round_fill.png";     // 64x64, radius 24, 9-sliced
        public const string RoundRing = Dir + "round_ring.png";     // 64x64, radius 24, 4 px stroke, 9-sliced
        public const string Circle = Dir + "circle.png";            // 256x256 filled circle
        public const string CircleRing = Dir + "circle_ring.png";   // 256x256, 12 px stroke
        public const string SoftShadow = Dir + "soft_shadow.png";   // 256x64 soft ellipse (world sprite)
        public const string GradientH = Dir + "gradient_h.png";     // 256x4, alpha 0 -> 1 left to right
        public const string Chevron = Dir + "chevron.png";          // 64x64 right-pointing chevron, round caps

        [MenuItem("One Health/Build UI/Generated Sprites")]
        public static void EnsureAll()
        {
            Directory.CreateDirectory(Dir);
            Write(RoundFill, 64, 64, (x, y) => RoundRectAlpha(x, y, 64, 64, 24f, -1f), new Vector4(26, 26, 26, 26), 100f);
            Write(RoundRing, 64, 64, (x, y) => RoundRectAlpha(x, y, 64, 64, 24f, 4f), new Vector4(26, 26, 26, 26), 100f);
            Write(Circle, 256, 256, (x, y) => CircleAlpha(x, y, 256, -1f), Vector4.zero, 100f);
            Write(CircleRing, 256, 256, (x, y) => CircleAlpha(x, y, 256, 12f), Vector4.zero, 100f);
            Write(SoftShadow, 256, 64, ShadowAlpha, Vector4.zero, 100f);
            Write(GradientH, 256, 4, (x, y) => (x + 0.5f) / 256f, Vector4.zero, 100f);
            Write(Chevron, 64, 64, ChevronAlpha, Vector4.zero, 100f);
        }

        public static Sprite Load(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) { EnsureAll(); s = AssetDatabase.LoadAssetAtPath<Sprite>(path); }
            return s;
        }

        // Signed distance to a rounded rectangle (pixel centres); negative inside.
        static float RoundRectDistance(float px, float py, int w, int h, float r)
        {
            float cx = w / 2f, cy = h / 2f;
            float qx = Mathf.Abs(px - cx) - (cx - r - 1f);
            float qy = Mathf.Abs(py - cy) - (cy - r - 1f);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }

        static float RoundRectAlpha(int x, int y, int w, int h, float r, float stroke)
        {
            float d = RoundRectDistance(x + 0.5f, y + 0.5f, w, h, r);
            float a = Mathf.Clamp01(0.5f - d);
            if (stroke > 0f) a *= Mathf.Clamp01(d + stroke + 0.5f);
            return a;
        }

        static float CircleAlpha(int x, int y, int size, float stroke)
        {
            float c = size / 2f;
            float d = new Vector2(x + 0.5f - c, y + 0.5f - c).magnitude - (c - 1.5f);
            float a = Mathf.Clamp01(0.5f - d);
            if (stroke > 0f) a *= Mathf.Clamp01(d + stroke + 0.5f);
            return a;
        }

        static float ShadowAlpha(int x, int y)
        {
            float nx = (x + 0.5f - 128f) / 128f, ny = (y + 0.5f - 32f) / 32f;
            float r = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
            float k = 1f - r * r;
            return k * k;
        }

        static float ChevronAlpha(int x, int y)
        {
            var p = new Vector2(x + 0.5f, y + 0.5f);
            float d = Mathf.Min(SegmentDistance(p, new Vector2(24, 10), new Vector2(44, 32)),
                                SegmentDistance(p, new Vector2(44, 32), new Vector2(24, 54)));
            return Mathf.Clamp01(4.5f - d);
        }

        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return (p - (a + ab * t)).magnitude;
        }

        static void Write(string path, int w, int h, System.Func<int, int, float> alpha, Vector4 border, float ppu)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(x, y)) * 255f));
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.wrapMode = TextureWrapMode.Clamp;
            imp.filterMode = FilterMode.Bilinear;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spritePixelsPerUnit = ppu;
            imp.spriteBorder = border;
            imp.SaveAndReimport();
        }
    }
}

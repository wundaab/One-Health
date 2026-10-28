using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Art-pipeline mitigations that need no new art (UI critique U28, U29, section 3.2):
    /// uncompressed import for backgrounds and characters, and a per-scene grade and crop on the
    /// background sprite (pushes the painted art back, crops the worst garbled signage, and gives the
    /// reused community scene a different mood per scenario).
    /// </summary>
    public static class ArtPipeline
    {
        static readonly string[] ArtFolders =
        {
            "Assets/Art/01_Scenario_1/02_Environments", "Assets/Art/01_Scenario_1/01_Characters",
            "Assets/Art/02_Scenario_2/01_Characters", "Assets/Art/02_Scenario_2/sector_contact",
        };
        const string KeyArt = "Assets/Backgrounds/mainMenuBackgroun.jpeg";

        // Grades (multiplied into the background sprite colour).
        public static readonly Color ClinicGrade = UITheme.Hex("F4EEE6");      // slight warm darkening: art sits back
        public static readonly Color CommunityGrade = UITheme.Hex("F4EEE6");
        public static readonly Color OvercastGrade = UITheme.Hex("E3E8EC");    // Scenario 3: heavy rain, cooler
        public static readonly Color LateAfternoonGrade = UITheme.Hex("FFE9CF"); // Scenario 4: warm turning point

        /// <summary>Screen height in world units for the scenario views (Fungus View size 1.8 = half height).</summary>
        const float ViewHeight = 3.6f;

        [MenuItem("One Health/Art/Apply Import Quality")]
        public static string ApplyImportQuality()
        {
            var paths = AssetDatabase.FindAssets("t:Texture2D", ArtFolders).Select(AssetDatabase.GUIDToAssetPath).ToList();
            paths.Add(KeyArt);
            int changed = 0;
            foreach (var path in paths)
            {
                var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp == null || imp.textureCompression == TextureImporterCompression.Uncompressed) continue;
                // No block-compression artefacts on top of the upscale; mipmaps stay off, filter stays bilinear.
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
                changed++;
            }
            return changed + " of " + paths.Count + " textures set to uncompressed";
        }

        /// <summary>
        /// Tints, scales and shifts a background sprite about its view centre. Offset is in screen pixels
        /// at 1080p (positive x moves the art right, positive y moves it up).
        /// </summary>
        public static void Grade(SpriteRenderer bg, Vector3 viewCentre, Color tint, float scale, Vector2 offsetPx)
        {
            float unitsPerPixel = ViewHeight / 1080f;
            Undo.RecordObject(bg, "Grade background");
            Undo.RecordObject(bg.transform, "Grade background");
            bg.color = tint;
            bg.transform.localScale = new Vector3(scale, scale, 1f);
            bg.transform.position = new Vector3(viewCentre.x + offsetPx.x * unitsPerPixel, viewCentre.y + offsetPx.y * unitsPerPixel, bg.transform.position.z);
            EditorUtility.SetDirty(bg);
            EditorUtility.SetDirty(bg.transform);
        }

        /// <summary>Clinic crop: the garbled "WASHING… ES LIVES" poster at the left edge goes off-frame.</summary>
        public static void GradeClinic(SpriteRenderer bg, Vector3 viewCentre) { Grade(bg, viewCentre, ClinicGrade, 1.1f, new Vector2(-60f, 0f)); }

        /// <summary>Community crop: the easel and presenter move down toward the dialog box, away from mid-frame.</summary>
        public static void GradeCommunity(SpriteRenderer bg, Vector3 viewCentre, Color tint) { Grade(bg, viewCentre, tint, 1.12f, new Vector2(0f, -230f)); }

        /// <summary>Scenarios 1 and 2 are hand-authored: grade their background sprites in place.</summary>
        [MenuItem("One Health/Art/Grade Scenario 1 and 2 Backgrounds")]
        public static string GradeHandAuthoredScenes()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var path in new[] { "Assets/Scenes/SC01.unity", "Assets/Scenes/SC02.unity" })
            {
                var scene = EditorSceneManager.OpenScene(path);
                var views = Object.FindObjectsByType<Fungus.View>(FindObjectsInactive.Include);
                foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
                {
                    if (sr.sprite == null) continue;
                    var view = views.FirstOrDefault(v => (v.name == "View_Clinic" && sr.sprite.name.StartsWith("Kivu_Clinic"))
                        || (v.name == "View_Community" && sr.sprite.name.StartsWith("Kivu_Community")));
                    if (view == null) continue;
                    if (view.name == "View_Clinic") GradeClinic(sr, view.transform.position);
                    else GradeCommunity(sr, view.transform.position, CommunityGrade);
                    sb.AppendLine(path + ": " + sr.name + " graded for " + view.name);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            return sb.ToString();
        }
    }
}

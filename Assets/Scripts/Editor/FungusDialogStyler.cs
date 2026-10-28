using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Restyles the Fungus say and menu dialog prefabs (Assets/Prefabs/UI/Dialogs) to the design
    /// system. Edits the existing objects in place and finds-or-creates added children by name, so
    /// it can be re-run safely and scene instances (and the object references Fungus keeps) survive.
    /// </summary>
    public static class FungusDialogStyler
    {
        const string MenuPath = "Assets/Prefabs/UI/Dialogs/CustomSelectionMenuDialog.prefab";
        const string SayPath = "Assets/Prefabs/UI/Dialogs/CustomSayDialogBox.prefab";
        const string UI = "Assets/Art/01_Scenario_1/04_UI/";
        const string BoldTtf = "Assets/Art/Fonts/Nunito-Bold.ttf";
        const string ClickPath = "Assets/Fungus/Audio/Click.wav";
        const string LockPath = "Assets/Art/02_Scenario_2/10_General_UI_States/lock_overlay.png";

        /// <summary>Canvas both Fungus dialogs share with GameUI and the main menu (U32).</summary>
        static readonly Vector2 Reference = new Vector2(1920, 1080);

        /// <summary>Bottom of the option list: just above the say dialog (box top 300 + name tab), so the question stays readable.</summary>
        public const float OptionsBottom = 348f;

        [MenuItem("One Health/Build UI/Fungus Dialogs")]
        public static void StyleAll()
        {
            GeneratedSprites.EnsureAll();
            OneHealthUIBuilder.ApplySpriteBorders();
            // The lock was auto-sliced into shackle and body; use it as one sprite (closed lock, U6/U19).
            SingleSprite(LockPath);
            StyleMenuDialog();
            StyleSayDialog();
            AssetDatabase.SaveAssets();
        }

        // ------------------------------------------------------------------ helpers

        static Sprite S(string path)
        {
            var full = path.StartsWith("Assets/") ? path : UI + path;
            var s = AssetDatabase.LoadAllAssetsAtPath(full).OfType<Sprite>().FirstOrDefault();
            if (s == null) Debug.LogWarning("[OHI] Sprite not found: " + full);
            return s;
        }

        public static void SingleSprite(string path)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp == null || imp.spriteImportMode == SpriteImportMode.Single) return;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
        }

        static RectTransform Child(Transform parent, string name, int siblingIndex = -1)
        {
            var t = parent.Find(name);
            if (t == null)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.layer = LayerMask.NameToLayer("UI");
                t = go.transform;
                t.SetParent(parent, false);
            }
            if (siblingIndex >= 0) t.SetSiblingIndex(siblingIndex);
            return (RectTransform)t;
        }

        static T Get<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void Fill(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        static Image Img(RectTransform rt, Sprite sprite, Color color, bool sliced, bool raycast = false)
        {
            var img = Get<Image>(rt.gameObject);
            img.sprite = sprite;
            img.color = color;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.preserveAspect = !sliced;
            img.raycastTarget = raycast;
            return img;
        }

        static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[OHI] No field " + field + " on " + target.GetType().Name); return; }
            if (value is Object || value == null) p.objectReferenceValue = (Object)value;
            else if (value is float) p.floatValue = (float)value;
            else if (value is bool) p.boolValue = (bool)value;
            else if (value is int) p.intValue = (int)value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void UseReferenceCanvas(GameObject root)
        {
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Reference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // ------------------------------------------------------------------ say dialog

        static void StyleSayDialog()
        {
            var root = PrefabUtility.LoadPrefabContents(SayPath);
            try
            {
                UseReferenceCanvas(root);
                // Above the menu dialog and its scrim: the question stays readable under the options (U4).
                root.GetComponent<Canvas>().sortingOrder = 2;
                var say = root.GetComponent<Fungus.SayDialog>();
                Set(say, "fitTextWithImage", false);   // SayDialogStyle sets the text margin instead

                // Box: plain white card (no drawn-in tab), lifted off the screen edge (U9, U10).
                var panel = (RectTransform)root.transform.Find("Panel");
                panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0f);
                panel.pivot = new Vector2(0.5f, 0f);
                panel.anchoredPosition = new Vector2(0f, 24f);
                panel.sizeDelta = new Vector2(1280f, 290f);
                var box = Img(panel, S("Panels/rounded_panel_white.png"), Color.white, true, true);
                box.pixelsPerUnitMultiplier = 1f;

                // Portrait: circular bust at the left, overlapping the top edge (U11).
                var slot = Child(panel, "PortraitSlot", 0);
                slot.anchorMin = slot.anchorMax = new Vector2(0f, 1f);
                slot.pivot = new Vector2(0.5f, 0.5f);
                slot.anchoredPosition = new Vector2(118f, -46f);
                slot.sizeDelta = new Vector2(164f, 164f);
                var backdrop = Child(slot, "Backdrop", 0);
                Fill(backdrop, Vector2.zero, Vector2.zero);
                Img(backdrop, GeneratedSprites.Load(GeneratedSprites.Circle), UITheme.Cream, false);
                var mask = Child(slot, "Mask", 1);
                Fill(mask, new Vector2(6f, 6f), new Vector2(-6f, -6f));
                Img(mask, GeneratedSprites.Load(GeneratedSprites.Circle), Color.white, false);
                Get<Mask>(mask.gameObject).showMaskGraphic = false;
                var character = (Image)new SerializedObject(say).FindProperty("characterImage").objectReferenceValue;
                character.transform.SetParent(mask, false);
                character.raycastTarget = false;
                var characterLe = character.GetComponent<LayoutElement>();
                if (characterLe != null) characterLe.ignoreLayout = true;
                var ring = Child(slot, "Ring", 2);
                Fill(ring, Vector2.zero, Vector2.zero);
                Img(ring, GeneratedSprites.Load(GeneratedSprites.CircleRing), UITheme.SurfaceDark, false);

                // Story text: fills the box, margins keep it clear of the name tab, portrait and arrow.
                var story = panel.Find("StoryText");
                var storyRt = (RectTransform)story;
                Fill(storyRt, Vector2.zero, Vector2.zero);
                var storyLe = story.GetComponent<LayoutElement>();
                if (storyLe != null) storyLe.ignoreLayout = true;
                var tmp = story.GetComponent<TMPro.TMP_Text>();
                tmp.margin = new Vector4(48f, 52f, 112f, 26f);
                tmp.fontSize = 36f;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 28f;
                tmp.fontSizeMax = 36f;
                tmp.lineSpacing = 8f;
                tmp.color = UITheme.Ink;
                tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;

                // Name tab: navy, grows with the name; white name (U9).
                var tab = Child(panel, "NameTab");
                tab.anchorMin = tab.anchorMax = new Vector2(0f, 1f);
                tab.pivot = new Vector2(0f, 0.5f);
                tab.anchoredPosition = new Vector2(36f, 0f);
                tab.sizeDelta = new Vector2(200f, 54f);
                var tabImg = Img(tab, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.SurfaceDark, true);
                tabImg.pixelsPerUnitMultiplier = 1.4f;
                var hl = Get<HorizontalLayoutGroup>(tab.gameObject);
                hl.padding = new RectOffset(26, 26, 4, 6);
                hl.childAlignment = TextAnchor.MiddleLeft;
                hl.childControlWidth = hl.childControlHeight = true;
                hl.childForceExpandWidth = hl.childForceExpandHeight = false;
                var fit = Get<ContentSizeFitter>(tab.gameObject);
                fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fit.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                var name = (Text)new SerializedObject(say).FindProperty("nameText").objectReferenceValue;
                name.transform.SetParent(tab, false);
                name.font = AssetDatabase.LoadAssetAtPath<Font>(BoldTtf);
                name.fontSize = 30;
                name.resizeTextForBestFit = false;
                name.alignment = TextAnchor.MiddleLeft;
                name.horizontalOverflow = HorizontalWrapMode.Overflow;
                name.verticalOverflow = VerticalWrapMode.Overflow;
                name.color = Color.white;
                name.raycastTarget = false;

                // Continue: fixed at the box's bottom-right in every scenario, teal circle with a chevron.
                var cont = (RectTransform)root.GetComponentsInChildren<Button>(true).First(b => b.name == "Continue").transform;
                cont.SetParent(panel, false);
                cont.SetAsLastSibling();
                Place(cont, new Vector2(1f, 0f), new Vector2(-56f, 52f), new Vector2(60f, 60f));
                Img(cont, GeneratedSprites.Load(GeneratedSprites.Circle), UITheme.Teal, false, true);
                var glyph = Child(cont, "Glyph");
                Place(glyph, new Vector2(0.5f, 0.5f), new Vector2(2f, 0f), new Vector2(28f, 28f));
                Img(glyph, GeneratedSprites.Load(GeneratedSprites.Chevron), Color.white, false);

                var style = Get<SayDialogStyle>(root);
                Set(style, "nameText", name);
                Set(style, "nameTab", tab.gameObject);
                Set(style, "nameTabRect", tab);
                Set(style, "portraitSlot", slot.gameObject);
                Set(style, "characterImage", character);
                Set(style, "portraitMask", mask);
                Set(style, "storyText", tmp);
                slot.gameObject.SetActive(false);   // shown by SayDialogStyle when the line has a portrait

                PrefabUtility.SaveAsPrefabAsset(root, SayPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // ------------------------------------------------------------------ menu dialog

        static void StyleMenuDialog()
        {
            var root = PrefabUtility.LoadPrefabContents(MenuPath);
            try
            {
                UseReferenceCanvas(root);
                // Below the say dialog (order 2), so the question stays readable above the scrim.
                root.GetComponent<Canvas>().sortingOrder = 1;

                // Scrim behind the options: calms busy background art (U5).
                var scrim = Child(root.transform, "Scrim", 0);
                Fill(scrim, Vector2.zero, Vector2.zero);
                Img(scrim, null, UITheme.WithAlpha(UITheme.TealDark, 0.45f), false);
                var scrimGroup = Get<CanvasGroup>(scrim.gameObject);
                scrimGroup.blocksRaycasts = false;
                scrimGroup.interactable = false;

                var group = (RectTransform)root.transform.Find("ButtonGroup");
                group.anchorMin = group.anchorMax = new Vector2(0.5f, 0f);
                group.pivot = new Vector2(0.5f, 0f);
                group.anchoredPosition = new Vector2(0f, OptionsBottom);
                group.sizeDelta = new Vector2(1240f, group.sizeDelta.y);
                var layout = group.GetComponent<VerticalLayoutGroup>();
                layout.spacing = 12f;
                layout.childAlignment = TextAnchor.LowerCenter;

                // Five buttons: Scenario 2's reflection question has five answers.
                var template = group.Find("OptionButton3");
                for (int i = 4; i < 5; i++)
                {
                    if (group.Find("OptionButton" + i) != null) continue;
                    var copy = Object.Instantiate(template.gameObject, group);
                    copy.name = "OptionButton" + i;
                }
                for (int i = 0; i < group.childCount; i++)
                    StyleOption(group.GetChild(i).gameObject);

                var dl = Get<MenuDialogLayout>(root);
                Set(dl, "buttonGroup", group);
                Set(dl, "scrim", scrimGroup);

                PrefabUtility.SaveAsPrefabAsset(root, MenuPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void StyleOption(GameObject go)
        {
            var rt = (RectTransform)go.transform;
            var img = Img(rt, S("Choices/choice_normal.png"), Color.white, true, true);
            img.pixelsPerUnitMultiplier = 1f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.transition = Selectable.Transition.SpriteSwap;
            // Hover and focus are shown by the amber ring and a small lift (MenuOptionStyle, UIButtonFeedback),
            // so the card itself only changes when disabled.
            button.spriteState = new SpriteState { disabledSprite = S("Choices/choice_disabled.png") };
            button.colors = ColorBlock.defaultColorBlock;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.Vertical;
            button.navigation = nav;

            // Label: keep Fungus's Text object as the first child (Fungus and the playthrough driver read it).
            var label = go.GetComponentInChildren<Text>(true);
            label.transform.SetSiblingIndex(0);
            label.font = AssetDatabase.LoadAssetAtPath<Font>(BoldTtf);
            label.fontSize = 30;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 18;
            label.resizeTextMaxSize = 30;
            label.alignment = TextAnchor.MiddleLeft;
            label.supportRichText = true;
            label.lineSpacing = 1f;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.color = UITheme.Ink;
            label.raycastTarget = false;
            Fill(label.rectTransform, new Vector2(104f, 14f), new Vector2(-84f, -14f));

            // A/B/C position marker (neutral teal), replaced by a lock when the option is unavailable.
            var marker = Child(rt, "Marker");
            Place(marker, new Vector2(0f, 0.5f), new Vector2(60f, 1f), new Vector2(52f, 52f));
            Img(marker, GeneratedSprites.Load(GeneratedSprites.Circle), UITheme.Teal, false);
            var letterRt = Child(marker, "Letter");
            Fill(letterRt, Vector2.zero, new Vector2(0f, 2f));
            var letter = Get<Text>(letterRt.gameObject);
            letter.font = label.font;
            letter.fontSize = 28;
            letter.alignment = TextAnchor.MiddleCenter;
            letter.color = Color.white;
            letter.text = "A";
            letter.raycastTarget = false;

            var lockRt = Child(rt, "Lock");
            Place(lockRt, new Vector2(0f, 0.5f), new Vector2(60f, 2f), new Vector2(34f, 34f));
            Img(lockRt, S(LockPath), UITheme.DisabledInk, false);
            lockRt.gameObject.SetActive(false);

            var chevronRt = Child(rt, "Chevron");
            Place(chevronRt, new Vector2(1f, 0.5f), new Vector2(-52f, 0f), new Vector2(30f, 30f));
            var oldGlyph = chevronRt.GetComponent<Text>();
            if (oldGlyph != null) Object.DestroyImmediate(oldGlyph);
            Img(chevronRt, GeneratedSprites.Load(GeneratedSprites.Chevron), UITheme.Teal, false);

            // Focus ring over the card's visible edge (the sprite has a soft shadow margin).
            var ringRt = Child(rt, "Focus");
            Fill(ringRt, new Vector2(13f, 11f), new Vector2(-13f, -11f));
            var ring = Img(ringRt, GeneratedSprites.Load(GeneratedSprites.RoundRing), UITheme.Focus, true);
            ring.pixelsPerUnitMultiplier = 0.85f;
            ring.enabled = false;

            var fb = Get<UIButtonFeedback>(go);
            Set(fb, "clickSound", AssetDatabase.LoadAssetAtPath<AudioClip>(ClickPath));
            Set(fb, "hoverScale", 1.02f);

            var style = Get<MenuOptionStyle>(go);
            Set(style, "label", label);
            Set(style, "marker", marker.gameObject);
            Set(style, "markerLetter", letter);
            Set(style, "lockIcon", lockRt.gameObject);
            Set(style, "chevron", chevronRt.gameObject);
            Set(style, "focusRing", ring);
        }
    }
}

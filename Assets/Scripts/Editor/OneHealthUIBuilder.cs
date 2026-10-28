using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Builds the shared UI (main menu, settings, HUD, pause, visual card, results) from the
    /// art team's sprites and DESIGN_SYSTEM.md palette. Re-running regenerates the prefabs;
    /// after that the prefabs can be edited normally in Unity.
    /// </summary>
    public static class OneHealthUIBuilder
    {
        const string UI = "Assets/Art/01_Scenario_1/04_UI/";
        const string PrefabDir = "Assets/Prefabs/UI/";
        const string BoldFontPath = "Assets/Art/Fonts/Nunito-Bold SDF.asset";
        const string RegularFontPath = "Assets/Art/Fonts/Nunito-Regular SDF.asset";
        const string KeyArtPath = "Assets/Backgrounds/mainMenuBackgroun.jpeg";
        const string ClickPath = "Assets/Fungus/Audio/Click.wav";
        const string MenuMusicPath = "Assets/Audio/M01_Opening.wav.mp3";
        /// <summary>The closed lock used for every locked state (the Icons/lock_icon shackle reads as open).</summary>
        const string LockPath = "Assets/Art/02_Scenario_2/10_General_UI_States/lock_overlay.png";

        static TMP_FontAsset bold, regular;
        static AudioClip click;

        // ------------------------------------------------------------------ entry points

        [MenuItem("One Health/Build UI/All")]
        public static void BuildAll()
        {
            Prepare();
            BuildSharedPrefabs();
            BuildGameUIPrefab();
            BuildMainMenuInOpenScene();
        }

        [MenuItem("One Health/Build UI/Shared Prefabs")]
        public static void BuildPrefabsOnly()
        {
            Prepare();
            BuildSharedPrefabs();
            BuildGameUIPrefab();
        }

        static void Prepare()
        {
            EnsureBoldFont();
            ApplySpriteBorders();
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
            regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);
            click = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickPath);
            Directory.CreateDirectory(PrefabDir);
        }

        // ------------------------------------------------------------------ assets

        static void EnsureBoldFont()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath) != null) return;
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/Nunito-Bold.ttf");
            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
            asset.name = "Nunito-Bold SDF";
            AssetDatabase.CreateAsset(asset, BoldFontPath);
            asset.atlasTextures[0].name = "Nunito-Bold SDF Atlas";
            asset.material.name = "Nunito-Bold SDF Material";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            AssetDatabase.SaveAssets();
        }

        /// <summary>9-slice borders so rounded panels and buttons scale without stretching corners.</summary>
        internal static void ApplySpriteBorders()
        {
            var borders = new Dictionary<string, int>
            {
                { "Panels/rounded_panel_white.png", 48 }, { "Panels/card_background.png", 48 },
                { "Panels/header_panel_green.png", 40 }, { "Panels/generic_panel_dark.png", 48 },
                { "Panels/info_card_white.png", 44 }, { "Settings/panel.png", 48 },
                { "Menus/Pause_Menu/panel.png", 48 }, { "HUD/stat_background.png", 40 },
                { "Settings/slider_background.png", 22 }, { "Settings/slider_fill.png", 22 },
                { "Choices/choice_normal.png", 40 }, { "Choices/choice_disabled.png", 40 },
                { "Choices/choice_hover.png", 40 }, { "Choices/choice_selected.png", 40 },
            };
            foreach (var color in new[] { "Blue", "Gold", "Green", "Navy", "Orange", "Slate" })
                foreach (var state in new[] { "normal", "hover", "pressed", "selected", "disabled" })
                    borders["Buttons/" + color + "/" + state + ".png"] = 44;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            foreach (var pair in borders)
            {
                var importer = AssetImporter.GetAtPath(UI + pair.Key) as TextureImporter;
                if (importer == null) { Debug.LogWarning("[OHI] Missing sprite " + pair.Key); continue; }
                var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
                provider.InitSpriteEditorDataProvider();
                var rects = provider.GetSpriteRects();
                bool changed = false;
                foreach (var r in rects)
                {
                    float b = Mathf.Min(pair.Value, r.rect.width / 2f - 1f, r.rect.height / 2f - 1f);
                    var border = new Vector4(b, b, b, b);
                    if (r.border != border) { r.border = border; changed = true; }
                }
                if (!changed) continue;
                provider.SetSpriteRects(rects);
                provider.Apply();
                importer.SaveAndReimport();
            }
        }

        static Sprite S(string path)
        {
            var full = path.StartsWith("Assets/") ? path : UI + path;
            var sprite = AssetDatabase.LoadAllAssetsAtPath(full).OfType<Sprite>().FirstOrDefault();
            if (sprite == null) Debug.LogWarning("[OHI] Sprite not found: " + full);
            return sprite;
        }

        // ------------------------------------------------------------------ primitives

        static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static RectTransform Center(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            return Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        }

        static RectTransform Stretch(string name, Transform parent)
        {
            return Rect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        static Image Img(RectTransform rt, Sprite sprite, Color color, bool sliced = true)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sliced && sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            if (img.type == Image.Type.Simple) img.preserveAspect = true;
            return img;
        }

        static TextMeshProUGUI Txt(string name, Transform parent, string text, bool isBold, float size, Color color,
            TextAlignmentOptions align, Vector2 pos, Vector2 boxSize)
        {
            var rt = Center(name, parent, pos, boxSize);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = isBold ? bold : regular;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }

        static Image Overlay(Transform parent, float alpha)
        {
            var rt = Stretch("Dim", parent);
            return Img(rt, null, UITheme.WithAlpha(UITheme.TealDark, alpha));
        }

        /// <summary>Button using the art team's 5-state sprites (normal/hover/pressed/selected/disabled).</summary>
        static Button Btn(string name, Transform parent, string color, string label, string icon, Vector2 pos, Vector2 size,
            float fontSize, out TextMeshProUGUI labelText)
        {
            var rt = Center(name, parent, pos, size);
            var img = Img(rt, S("Buttons/" + color + "/normal.png"), Color.white);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.spriteState = new SpriteState
            {
                highlightedSprite = S("Buttons/" + color + "/hover.png"),
                pressedSprite = S("Buttons/" + color + "/pressed.png"),
                selectedSprite = S("Buttons/" + color + "/hover.png"),
                disabledSprite = S("Buttons/" + color + "/disabled.png"),
            };
            var fb = rt.gameObject.AddComponent<UIButtonFeedback>();
            Set(fb, "clickSound", click);

            float iconSize = icon != null ? Mathf.Min(size.y * 0.5f, 52f) : 0f;
            float gap = icon != null ? 14f : 0f;
            labelText = Txt("Label", rt, label, true, fontSize, Color.white, TextAlignmentOptions.Center,
                new Vector2((iconSize + gap) / 2f, 2f), new Vector2(size.x - 60f - iconSize, size.y - 16f));
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = fontSize * 0.6f;
            labelText.fontSizeMax = fontSize;

            if (icon != null)
            {
                var ic = Center("Icon", rt, new Vector2(-size.x / 2f + 30f + iconSize / 2f, 2f), new Vector2(iconSize, iconSize));
                Img(ic, S(icon), Color.white, false).raycastTarget = false;
            }
            return btn;
        }

        static Button Btn(string name, Transform parent, string color, string label, string icon, Vector2 pos, Vector2 size, float fontSize = 34f)
        {
            TextMeshProUGUI unused;
            return Btn(name, parent, color, label, icon, pos, size, fontSize, out unused);
        }

        static UIPanel PanelRoot(string name, Transform parent, out RectTransform content, Vector2 contentSize, Sprite panelSprite, float dim = 0.6f)
        {
            var root = Stretch(name, parent);
            root.gameObject.AddComponent<CanvasGroup>();
            Overlay(root, dim);
            content = Center("Content", root, Vector2.zero, contentSize);
            Img(content, panelSprite, Color.white);
            var panel = root.gameObject.AddComponent<UIPanel>();
            Set(panel, "content", content);
            return panel;
        }

        static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[OHI] No field " + field + " on " + target.GetType().Name); return; }
            if (value is Object || value == null) p.objectReferenceValue = (Object)value;
            else if (value is string) p.stringValue = (string)value;
            else if (value is int) { if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = (int)value; else p.intValue = (int)value; }
            else if (value is bool) p.boolValue = (bool)value;
            else if (value is float) p.floatValue = (float)value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Canvas MakeCanvas(string name, Transform parent, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null) go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            var path = PrefabDir + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        // ------------------------------------------------------------------ shared prefabs

        static void BuildSharedPrefabs()
        {
            SavePrefab(BuildSettingsPanel(null), "SettingsPanel");
            SavePrefab(BuildConfirmDialog(null), "ConfirmDialog");
        }

        static GameObject BuildSettingsPanel(Transform parent)
        {
            RectTransform c;
            var panel = PanelRoot("SettingsPanel", parent, out c, new Vector2(680, 620), S("Settings/panel.png"));
            Txt("Title", c, "Settings", true, 52, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 228), new Vector2(560, 70));
            var divider = Center("Divider", c, new Vector2(0, 182), new Vector2(520, 6));
            Img(divider, S("Decorative/divider_line.png"), UITheme.WithAlpha(UITheme.TealLight, 0.9f), false).preserveAspect = false;

            TMP_Text musicState, sfxState;
            var music = ToggleRow(c, "Music", "Settings/music_icon.png", new Vector2(0, 102), out musicState, Color.white);
            // The two icon tiles are different blues; this tint brings the SFX tile onto the music tile's colour (U26).
            var sfx = ToggleRow(c, "Sound Effects", "Settings/sfx_icon.png", new Vector2(0, -2), out sfxState, new Color(0.585f, 0.621f, 0.694f));
            TextMeshProUGUI tipsLabel;
            var tips = Btn("TipsButton", c, "Slate", "Show tutorial tips again", null, new Vector2(0, -110), new Vector2(520, 80), 28f, out tipsLabel);
            var back = Btn("BackButton", c, "Navy", "Back", "Icons/back_arrow_icon.png", new Vector2(0, -228), new Vector2(320, 96));

            var sp = panel.gameObject.AddComponent<SettingsPanel>();
            Set(sp, "panel", panel);
            Set(sp, "musicToggle", music);
            Set(sp, "sfxToggle", sfx);
            Set(sp, "musicState", musicState);
            Set(sp, "sfxState", sfxState);
            Set(sp, "backButton", back);
            Set(sp, "tipsButton", tips);
            Set(sp, "tipsLabel", tipsLabel);
            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        static Toggle ToggleRow(Transform parent, string label, string icon, Vector2 pos, out TMP_Text state, Color iconTint)
        {
            var row = Center(label.Replace(" ", "") + "Row", parent, pos, new Vector2(560, 88));
            var ic = Center("Icon", row, new Vector2(-236, 0), new Vector2(76, 76));
            Img(ic, S(icon), iconTint, false);
            Txt("Label", row, label, true, 36, UITheme.TealDark, TextAlignmentOptions.MidlineLeft, new Vector2(-30, 0), new Vector2(300, 60));
            state = Txt("State", row, "On", false, 28, UITheme.Teal, TextAlignmentOptions.MidlineRight, new Vector2(116, 0), new Vector2(76, 60));

            var t = Center("Toggle", row, new Vector2(232, 0), new Vector2(112, 56));
            var bg = Img(t, S("Settings/toggle_off.png"), Color.white, false);
            var onRt = Stretch("On", t);
            var on = Img(onRt, S("Settings/toggle_on.png"), Color.white, false);
            on.raycastTarget = false;
            var toggle = t.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = bg;
            toggle.graphic = on;
            toggle.toggleTransition = Toggle.ToggleTransition.Fade;
            toggle.isOn = true;
            var fb = t.gameObject.AddComponent<UIButtonFeedback>();
            Set(fb, "clickSound", click);
            return toggle;
        }

        static GameObject BuildConfirmDialog(Transform parent)
        {
            RectTransform c;
            var panel = PanelRoot("ConfirmDialog", parent, out c, new Vector2(760, 420), S("Panels/rounded_panel_white.png"), 0.7f);
            var title = Txt("Title", c, "Are you sure?", true, 44, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 120), new Vector2(640, 64));
            var msg = Txt("Message", c, "", false, 28, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 20), new Vector2(640, 120));
            var cancel = Btn("CancelButton", c, "Slate", "Cancel", null, new Vector2(-160, -124), new Vector2(280, 92));
            TextMeshProUGUI confirmLabel;
            var ok = Btn("ConfirmButton", c, "Orange", "Confirm", null, new Vector2(160, -124), new Vector2(280, 92), 34f, out confirmLabel);

            var cd = panel.gameObject.AddComponent<ConfirmDialog>();
            Set(cd, "panel", panel);
            Set(cd, "title", title);
            Set(cd, "message", msg);
            Set(cd, "confirmButton", ok);
            Set(cd, "confirmLabel", confirmLabel);
            Set(cd, "cancelButton", cancel);
            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        static GameObject InstantiatePrefab(string name, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + name + ".prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            return go;
        }

        // ------------------------------------------------------------------ in-game UI

        static void BuildGameUIPrefab()
        {
            var canvas = MakeCanvas("GameUI", null, 100);
            var root = canvas.transform;

            BuildHUD(root);
            var card = BuildVisualCard(root);
            var results = BuildResults(root);
            var pause = BuildPause(root);

            var settings = InstantiatePrefab("SettingsPanel", root);
            var confirm = InstantiatePrefab("ConfirmDialog", root);
            Set(pause, "settings", settings.GetComponent<SettingsPanel>());
            Set(pause, "confirm", confirm.GetComponent<ConfirmDialog>());

            card.transform.SetSiblingIndex(1);
            results.transform.SetSiblingIndex(2);
            BuildTutorial(root).transform.SetAsLastSibling();
            SavePrefab(canvas.gameObject, "GameUI");
        }

        static void BuildHUD(Transform root)
        {
            var hud = Rect("HUD", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -76), new Vector2(1320, 128));
            hud.gameObject.AddComponent<CanvasGroup>();
            var layout = hud.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;

            Chip(hud, StatKind.Money, "Money", "HUD/money_icon.png", "How much can I afford to do?", GameState.MoneyMax, false, false);
            Chip(hud, StatKind.Time, "Time", "HUD/time_icon.png", "How much time do I have left?", GameState.TimeMax, false, false);
            Chip(hud, StatKind.Trust, "Trust", "HUD/trust_icon.png", "Will people work with me? (0–100)", GameState.TrustMax, true, false);
            // Tooltip wording from the Scenario 3 blueprint.
            Chip(hud, StatKind.Resistance, "Resistance Pressure", "HUD/resistance_icon.png", "A simplified gameplay indicator of conditions that can increase or reduce AMR risk. It is not a clinical measurement and does not show whether a person or animal has a resistant infection.", GameState.ResistanceMax, true, true);
            var capacity = Chip(hud, StatKind.Capacity, "Capacity", "Assets/Art/02_Scenario_2/06_HUD/capacity_icon_normal.png", "How much work your team can still take on. (0–100)", GameState.CapacityMax, true, false);
            // Scenario 3/4 outcome rules use Capacity >= 10 and >= 5; warn at 10 and below.
            Set(capacity.GetComponent<StatChip>(), "lowThreshold", 10);

            var h = hud.gameObject.AddComponent<StatHUD>();
            Set(h, "visibleOnStart", false);
        }

        static RectTransform Chip(Transform parent, StatKind kind, string label, string icon, string tooltip, int max, bool bar, bool higherIsWorse)
        {
            // Resistance Pressure is wider so its label fits at the same size as the others (U13).
            float w = kind == StatKind.Resistance ? 300f : 248f;
            float dx = (w - 248f) / 2f;   // extra width goes to the text column
            var rt = Center(label.Replace(" ", "") + "Chip", parent, Vector2.zero, new Vector2(w, 116));
            var bg = Img(rt, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.SurfaceDark);
            bg.pixelsPerUnitMultiplier = 1.5f;
            bg.raycastTarget = true;
            // Selectable so a tap or keyboard selection can show the tooltip; no navigation, so it never takes menu focus.
            var sel = rt.gameObject.AddComponent<Selectable>();
            sel.transition = Selectable.Transition.None;
            sel.navigation = new Navigation { mode = Navigation.Mode.None };
            sel.targetGraphic = bg;

            Sprite iconSprite = S(icon);
            var ic = Center("Icon", rt, new Vector2(-78 - dx, 0), new Vector2(64, 64));
            var iconImg = Img(ic, iconSprite, Color.white, false);
            iconImg.raycastTarget = false;

            var labelText = Txt("Label", rt, label.ToUpperInvariant(), true, 15, UITheme.TealLight, TextAlignmentOptions.MidlineLeft, new Vector2(34 - dx + dx, 28), new Vector2(150 + 2 * dx, 24));
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.enableAutoSizing = false;
            labelText.characterSpacing = 2f;
            var value = Txt("Value", rt, "0", true, 34, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(34, -4), new Vector2(150 + 2 * dx, 42));

            // Info hint: the chip has a tooltip (U16).
            var info = Center("Info", rt, new Vector2(w / 2f - 20f, -38), new Vector2(18, 18));
            var infoImg = Img(info, S("Icons/information_icon.png"), UITheme.WithAlpha(Color.white, 0.75f), false);
            infoImg.raycastTarget = false;

            // Status as a small pill on the chip's top edge (no longer clipped at the right edge).
            var pill = Rect("StatusPill", rt, Vector2.one, Vector2.one, new Vector2(-10, 0), new Vector2(64, 24));
            pill.pivot = new Vector2(1f, 0.5f);
            var pillImg = Img(pill, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.Coral);
            pillImg.pixelsPerUnitMultiplier = 2.2f;
            pillImg.raycastTarget = false;
            var pillLayout = pill.gameObject.AddComponent<HorizontalLayoutGroup>();
            pillLayout.padding = new RectOffset(10, 10, 1, 2);
            pillLayout.childControlWidth = pillLayout.childControlHeight = true;
            pillLayout.childForceExpandWidth = pillLayout.childForceExpandHeight = false;
            pill.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var status = Txt("Status", pill, "", true, 14, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(48, 20));
            status.textWrappingMode = TextWrappingModes.NoWrap;
            pill.gameObject.SetActive(false);

            Image fill = null;
            if (bar)
            {
                var track = Center("Bar", rt, new Vector2(34 - 14, -38), new Vector2(150 + 2 * dx - 28, 6));
                Img(track, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.WithAlpha(Color.white, 0.14f)).pixelsPerUnitMultiplier = 8f;
                var f = Stretch("Fill", track);
                // Filled images need a sprite to render the fill amount.
                fill = Img(f, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                    higherIsWorse ? UITheme.Coral : UITheme.LeafGreen, false);
                fill.preserveAspect = false;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.raycastTarget = false;
            }

            // Change pill under the chip: white text on navy, so it reads over any background art (U14).
            var dp = Rect("DeltaPill", rt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -26), new Vector2(96, 34));
            var dpImg = Img(dp, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.WithAlpha(UITheme.SurfaceDark, 0.95f));
            dpImg.pixelsPerUnitMultiplier = 1.6f;
            dpImg.raycastTarget = false;
            var dpLayout = dp.gameObject.AddComponent<HorizontalLayoutGroup>();
            dpLayout.padding = new RectOffset(14, 14, 2, 3);
            dpLayout.childControlWidth = dpLayout.childControlHeight = true;
            dpLayout.childForceExpandWidth = dpLayout.childForceExpandHeight = false;
            dp.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var dpGroup = dp.gameObject.AddComponent<CanvasGroup>();
            dpGroup.alpha = 0f;
            dpGroup.blocksRaycasts = false;
            var delta = Txt("Delta", dp, "", true, 22, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(80, 30));
            delta.textWrappingMode = TextWrappingModes.NoWrap;

            bool longTip = tooltip.Length > 60;
            var tip = Rect("Tooltip", rt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, longTip ? -100 : -78), longTip ? new Vector2(420, 120) : new Vector2(330, 74));
            Img(tip, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.SurfaceDark).pixelsPerUnitMultiplier = 1.5f;
            var tipText = Txt("Text", tip, tooltip, false, 20, Color.white, TextAlignmentOptions.Center, Vector2.zero, longTip ? new Vector2(384, 104) : new Vector2(296, 60));
            tipText.enableAutoSizing = true;
            tipText.fontSizeMin = 14;
            tipText.fontSizeMax = 20;
            tip.gameObject.SetActive(false);

            var chip = rt.gameObject.AddComponent<StatChip>();
            Set(chip, "kind", (int)kind);
            Set(chip, "maxValue", max);
            Set(chip, "higherIsWorse", higherIsWorse);
            Set(chip, "valueText", value);
            Set(chip, "statusText", status);
            Set(chip, "statusPill", pill.gameObject);
            Set(chip, "fill", fill);
            Set(chip, "deltaText", delta);
            Set(chip, "deltaPill", dpGroup);
            Set(chip, "tooltip", tip.gameObject);
            Set(chip, "icon", iconImg);
            if (kind == StatKind.Capacity)
            {
                var so = new SerializedObject(chip);
                var p = so.FindProperty("stateIcons");
                var states = new[] { "normal", "low", "critical" };
                p.arraySize = 3;
                for (int i = 0; i < 3; i++)
                    p.GetArrayElementAtIndex(i).objectReferenceValue = S("Assets/Art/02_Scenario_2/06_HUD/capacity_icon_" + states[i] + ".png");
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            return rt;
        }

        static PauseMenu BuildPause(Transform root)
        {
            // Always-active host: PauseMenu must run (Esc key, pause button) while its panel is hidden.
            var host = Stretch("Pause", root);
            // Pause glyph drawn in code (two bars): the gear means Settings (U18).
            TextMeshProUGUI pauseLabel;
            var pb = Btn("PauseButton", host, "Navy", "Pause", null, Vector2.zero, new Vector2(168, 72), 26f, out pauseLabel);
            var prt = (RectTransform)pb.transform;
            prt.anchorMin = prt.anchorMax = Vector2.one;
            prt.anchoredPosition = new Vector2(-112, -76);
            pauseLabel.rectTransform.anchoredPosition = new Vector2(18, 2);
            pauseLabel.rectTransform.sizeDelta = new Vector2(100, 56);
            var glyph = Center("Glyph", prt, new Vector2(-44, 1), new Vector2(26, 26));
            foreach (var x in new[] { -8f, 8f })
            {
                var bar = Center("Bar", glyph, new Vector2(x, 0), new Vector2(8, 26));
                var barImg = Img(bar, GeneratedSprites.Load(GeneratedSprites.RoundFill), Color.white);
                barImg.pixelsPerUnitMultiplier = 6f;
                barImg.raycastTarget = false;
            }

            // Opaque navy panel (no ghosting), short enough to clear the HUD (U19).
            RectTransform c;
            var panel = PanelRoot("PausePanel", host, out c, new Vector2(560, 780), GeneratedSprites.Load(GeneratedSprites.RoundFill), 0.65f);
            var panelImg = c.GetComponent<Image>();
            panelImg.color = UITheme.SurfaceDark;
            panelImg.pixelsPerUnitMultiplier = 0.6f;
            Txt("Title", c, "Paused", true, 52, Color.white, TextAlignmentOptions.Center, new Vector2(0, 322), new Vector2(460, 70));
            var size = new Vector2(420, 84);
            // One primary action (Resume); everything else neutral. Restart is guarded by a confirm dialog.
            var resume = Btn("ResumeButton", c, "Green", "Resume", null, new Vector2(0, 232), size);
            var settings = Btn("SettingsButton", c, "Slate", "Settings", "Icons/settings_icon.png", new Vector2(0, 132), size);
            var restart = Btn("RestartButton", c, "Slate", "Restart Scenario", null, new Vector2(0, 32), size);
            TextMeshProUGUI nextLabel;
            var next = Btn("NextButton", c, "Gold", "Next", LockPath, new Vector2(0, -68), size, 30f, out nextLabel);
            nextLabel.textWrappingMode = TextWrappingModes.Normal;
            nextLabel.lineSpacing = -10f;
            var lockIcon = next.transform.Find("Icon");
            var menu = Btn("MainMenuButton", c, "Slate", "Main Menu", null, new Vector2(0, -168), size);
            var exit = Btn("ExitButton", c, "Slate", "Exit Game", null, new Vector2(0, -268), size);
            var hint = Txt("Hint", c, "Press Esc to resume", false, 22, UITheme.TealLight, TextAlignmentOptions.Center, new Vector2(0, -340), new Vector2(460, 40));

            var pm = host.gameObject.AddComponent<PauseMenu>();
            Set(pm, "panel", panel);
            Set(pm, "pauseButton", pb);
            Set(pm, "resumeButton", resume);
            Set(pm, "settingsButton", settings);
            Set(pm, "mainMenuButton", menu);
            Set(pm, "exitButton", exit);
            Set(pm, "restartButton", restart);
            Set(pm, "nextButton", next);
            Set(pm, "nextLabel", nextLabel);
            Set(pm, "nextLockIcon", lockIcon.gameObject);
            Set(pm, "hintText", hint);

            panel.gameObject.SetActive(false);
            return pm;
        }

        /// <summary>
        /// Onboarding overlay: four dim panels framing a hole around the highlighted element, an amber ring,
        /// and a white explanation card. Its own canvas sorts above the Fungus dialogs and every panel.
        /// </summary>
        static TutorialOverlay BuildTutorial(Transform root)
        {
            var rt = Stretch("Tutorial", root);
            var canvas = rt.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;
            rt.gameObject.AddComponent<GraphicRaycaster>();
            rt.gameObject.AddComponent<CanvasGroup>();

            // Blocks every click while a tip is open, including through the highlighted hole.
            Img(Stretch("Blocker", rt), null, new Color(0f, 0f, 0f, 0f));
            var dims = new RectTransform[4];
            string[] names = { "DimTop", "DimBottom", "DimLeft", "DimRight" };
            for (int i = 0; i < 4; i++)
            {
                dims[i] = Center(names[i], rt, Vector2.zero, Vector2.zero);
                Img(dims[i], null, UITheme.WithAlpha(UITheme.TealDark, 0.62f)).raycastTarget = false;
            }
            var ring = Center("Ring", rt, Vector2.zero, Vector2.zero);
            var ringImg = Img(ring, GeneratedSprites.Load(GeneratedSprites.RoundRing), UITheme.Focus);
            ringImg.pixelsPerUnitMultiplier = 0.6f;   // thicker stroke
            ringImg.raycastTarget = false;

            var callout = Center("Callout", rt, Vector2.zero, new Vector2(720, 300));
            Img(callout, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.Surface).pixelsPerUnitMultiplier = 1.5f;
            var v = callout.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(44, 44, 34, 30);
            v.spacing = 10;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            callout.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var step = Txt("Step", callout, "HOW TO PLAY  ·  1 OF 8", true, 20, UITheme.Teal, TextAlignmentOptions.Left, Vector2.zero, new Vector2(632, 28));
            step.characterSpacing = 6f;
            var title = Txt("Title", callout, "Title", true, 38, UITheme.Ink, TextAlignmentOptions.Left, Vector2.zero, new Vector2(632, 50));
            var body = Txt("Body", callout, "Body", false, 27, UITheme.Ink, TextAlignmentOptions.TopLeft, Vector2.zero, new Vector2(632, 120));
            body.lineSpacing = 8f;

            var row = Center("Buttons", callout, Vector2.zero, new Vector2(632, 84));
            var rowLe = row.gameObject.AddComponent<LayoutElement>();
            rowLe.preferredHeight = rowLe.minHeight = 84;
            TextMeshProUGUI nextLabel;
            var next = Btn("NextButton", row, "Green", "Next", null, Vector2.zero, new Vector2(230, 72), 30f, out nextLabel);
            Pin(next, new Vector2(1f, 0.5f), Vector2.zero);
            var back = Btn("BackButton", row, "Slate", "Back", null, Vector2.zero, new Vector2(160, 72), 30f);
            Pin(back, new Vector2(1f, 0.5f), new Vector2(-246, 0));

            // "Skip tips" is a quiet text button so it never competes with Next.
            var skipRt = Rect("SkipButton", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(170, 60));
            skipRt.pivot = new Vector2(0f, 0.5f);
            var skipText = Txt("Label", skipRt, "<u>Skip tips</u>", false, 26, UITheme.InkMuted, TextAlignmentOptions.MidlineLeft, Vector2.zero, new Vector2(170, 60));
            skipText.raycastTarget = true;
            var skip = skipRt.gameObject.AddComponent<Button>();
            skip.targetGraphic = skipText;
            skip.colors = new ColorBlock
            {
                normalColor = Color.white, highlightedColor = new Color(0.75f, 0.85f, 0.82f), pressedColor = new Color(0.6f, 0.7f, 0.68f),
                selectedColor = new Color(0.75f, 0.85f, 0.82f), disabledColor = new Color(1f, 1f, 1f, 0.5f), colorMultiplier = 1f, fadeDuration = 0.1f,
            };
            Set(skipRt.gameObject.AddComponent<UIButtonFeedback>(), "clickSound", click);

            var overlay = rt.gameObject.AddComponent<TutorialOverlay>();
            Set(overlay, "dimTop", dims[0]);
            Set(overlay, "dimBottom", dims[1]);
            Set(overlay, "dimLeft", dims[2]);
            Set(overlay, "dimRight", dims[3]);
            Set(overlay, "ring", ring);
            Set(overlay, "callout", callout);
            Set(overlay, "stepLabel", step);
            Set(overlay, "title", title);
            Set(overlay, "body", body);
            Set(overlay, "backButton", back);
            Set(overlay, "nextButton", next);
            Set(overlay, "nextLabel", nextLabel);
            Set(overlay, "skipButton", skip);
            return overlay;
        }

        static void Pin(Component c, Vector2 anchor, Vector2 pos)
        {
            var r = (RectTransform)c.transform;
            r.anchorMin = r.anchorMax = r.pivot = anchor;
            r.anchoredPosition = pos;
        }

        static VisualCard BuildVisualCard(Transform root)
        {
            RectTransform c;
            var panel = PanelRoot("VisualCard", root, out c, new Vector2(1080, 820), S("Panels/rounded_panel_white.png"), 0.55f);
            var eyebrow = Txt("Eyebrow", c, "", true, 20, UITheme.Teal, TextAlignmentOptions.Center, new Vector2(0, 350), new Vector2(940, 30));
            eyebrow.characterSpacing = 6f;
            var title = Txt("Title", c, "", true, 44, UITheme.Ink, TextAlignmentOptions.Center, new Vector2(0, 310), new Vector2(940, 60));
            // Image well: a soft frame so an image's own off-white background reads as intentional (U21).
            var well = Center("ImageWell", c, new Vector2(0, 60), new Vector2(924, 444));
            Img(well, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.Hex("F3F1EA")).pixelsPerUnitMultiplier = 1.2f;
            var imgRt = Center("Image", c, new Vector2(0, 60), new Vector2(900, 420));
            var img = Img(imgRt, null, Color.white, false);
            img.preserveAspect = true;
            // Small celebratory sparkles around badge icons (shown only for small icon cards).
            var sparkles = Center("Sparkles", c, new Vector2(0, 60), new Vector2(300, 300));
            var sparkle = S("Decorative/sparkle.png");
            var spots = new[] { new Vector3(-118, 70, 14), new Vector3(122, 92, -18), new Vector3(104, -86, 32) };
            foreach (var spot in spots)
            {
                var sp = Center("Sparkle", sparkles, new Vector2(spot.x, spot.y), new Vector2(44, 44));
                sp.localRotation = Quaternion.Euler(0, 0, spot.z);
                Img(sp, sparkle, UITheme.Amber, false).raycastTarget = false;   // white art: tint so it shows on the white card
            }
            var caption = Txt("Caption", c, "", false, 30, UITheme.Ink, TextAlignmentOptions.Center, new Vector2(0, -230), new Vector2(940, 120));
            var cont = Btn("ContinueButton", c, "Green", "Continue", null, new Vector2(0, -338), new Vector2(340, 92));

            var vc = panel.gameObject.AddComponent<VisualCard>();
            Set(vc, "panel", panel);
            Set(vc, "eyebrow", eyebrow);
            Set(vc, "title", title);
            Set(vc, "image", img);
            Set(vc, "imageWell", well.GetComponent<Image>());
            Set(vc, "sparkles", sparkles.gameObject);
            Set(vc, "caption", caption);
            Set(vc, "continueButton", cont);
            Set(vc, "card", c.GetComponent<Image>());
            Set(vc, "dim", panel.transform.Find("Dim").GetComponent<Image>());
            panel.gameObject.SetActive(false);
            return vc;
        }

        static ResultsPanel BuildResults(Transform root)
        {
            RectTransform c;
            var panel = PanelRoot("ResultsPanel", root, out c, new Vector2(1320, 900), S("Panels/rounded_panel_white.png"), 0.7f);

            // Title, not a pill (a green pill reads as a button): heading with a short green rule (U24).
            var header = Txt("Header", c, "Scenario Complete", true, 48, UITheme.Ink, TextAlignmentOptions.Center, new Vector2(0, 376), new Vector2(900, 64));
            var rule = Center("Rule", c, new Vector2(0, 336), new Vector2(120, 4));
            Img(rule, GeneratedSprites.Load(GeneratedSprites.RoundFill), UITheme.LeafGreen).pixelsPerUnitMultiplier = 12f;
            var title = Txt("ScenarioTitle", c, "", true, 30, UITheme.Teal, TextAlignmentOptions.Center, new Vector2(0, 300), new Vector2(1180, 44));
            var summary = Txt("Summary", c, "", false, 26, UITheme.Ink, TextAlignmentOptions.Center, new Vector2(0, 232), new Vector2(1160, 96));
            summary.enableAutoSizing = true;
            summary.fontSizeMin = 18;
            summary.fontSizeMax = 26;
            summary.overflowMode = TextOverflowModes.Ellipsis;

            // Left column: final state
            var left = Center("FinalState", c, new Vector2(-300, -40), new Vector2(580, 440));
            Img(left, S("Panels/info_card_white.png"), Color.white);
            Txt("Heading", left, "Your resources", true, 30, UITheme.Ink, TextAlignmentOptions.MidlineLeft, new Vector2(14, 168), new Vector2(480, 44));
            var money = StatRow(left, "Money", "HUD/money_icon.png", 104);
            var time = StatRow(left, "Time", "HUD/time_icon.png", 44);
            var trust = StatRow(left, "Trust", "HUD/trust_icon.png", -16);
            var res = StatRow(left, "Resistance Pressure", "HUD/resistance_icon.png", -76);
            var cap = StatRow(left, "Capacity", "Assets/Art/02_Scenario_2/06_HUD/capacity_icon_normal.png", -136);

            // Right column: learning and unlocks
            var right = Center("Learning", c, new Vector2(300, -40), new Vector2(580, 440));
            Img(right, S("Panels/info_card_white.png"), Color.white);
            Txt("LearnedHeading", right, "What you learned", true, 30, UITheme.Ink, TextAlignmentOptions.MidlineLeft, new Vector2(14, 168), new Vector2(480, 44));
            var learned = Txt("Learned", right, "", false, 26, UITheme.Ink, TextAlignmentOptions.TopLeft, new Vector2(14, 80), new Vector2(480, 130));
            Txt("UnlockedHeading", right, "Unlocked & badges", true, 30, UITheme.Ink, TextAlignmentOptions.MidlineLeft, new Vector2(14, -16), new Vector2(480, 44));
            var unlocked = Txt("Unlocked", right, "", false, 26, UITheme.Ink, TextAlignmentOptions.TopLeft, new Vector2(14, -114), new Vector2(480, 150));
            foreach (var t in new[] { learned, unlocked })
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = 18;
                t.fontSizeMax = 26;
                t.overflowMode = TextOverflowModes.Ellipsis;
            }

            // Buttons in a centred row, so they stay centred when Next is hidden (U23).
            var row = Center("Buttons", c, new Vector2(0, -376), new Vector2(1200, 96));
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 40;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = hl.childControlHeight = false;
            hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            var size = new Vector2(330, 96);
            TextMeshProUGUI replayLabel, nextLabel;
            var replay = Btn("ReplayButton", row, "Slate", "Replay", null, Vector2.zero, size, 34f, out replayLabel);
            var menu = Btn("MainMenuButton", row, "Navy", "Main Menu", null, Vector2.zero, size);
            var next = Btn("NextButton", row, "Green", "Next", null, Vector2.zero, size, 34f, out nextLabel);

            var rp = panel.gameObject.AddComponent<ResultsPanel>();
            Set(rp, "panel", panel);
            Set(rp, "header", header);
            Set(rp, "scenarioTitle", title);
            Set(rp, "summary", summary);
            Set(rp, "moneyRow", money);
            Set(rp, "timeRow", time);
            Set(rp, "trustRow", trust);
            Set(rp, "resistanceRow", res);
            Set(rp, "capacityRow", cap);
            Set(rp, "learnedText", learned);
            Set(rp, "unlockedText", unlocked);
            Set(rp, "replayButton", replay);
            Set(rp, "replayLabel", replayLabel);
            Set(rp, "menuButton", menu);
            Set(rp, "nextButton", next);
            Set(rp, "nextLabel", nextLabel);
            SetSprites(rp, "primarySprites", "Green");
            SetSprites(rp, "neutralSprites", "Navy");
            panel.gameObject.SetActive(false);
            return rp;
        }

        static void SetSprites(Object target, string field, string color)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            var states = new[] { "normal", "hover", "pressed", "hover", "disabled" };
            p.arraySize = states.Length;
            for (int i = 0; i < states.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = S("Buttons/" + color + "/" + states[i] + ".png");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Icon, label (fixed size, may wrap to two lines) and a value laid out on tab stops by ResultsPanel.</summary>
        static TMP_Text StatRow(Transform parent, string label, string icon, float y)
        {
            var id = label.Replace(" ", "");
            var ic = Center(id + "Icon", parent, new Vector2(-246, y), new Vector2(46, 46));
            Img(ic, S(icon), Color.white, false);
            var l = Txt(id + "Label", parent, label, true, 22, UITheme.Teal, TextAlignmentOptions.MidlineLeft, new Vector2(-142, y), new Vector2(150, 52));
            l.textWrappingMode = TextWrappingModes.Normal;
            l.lineSpacing = -14f;
            var v = Txt(id + "Value", parent, "", false, 24, UITheme.Ink, TextAlignmentOptions.MidlineLeft, new Vector2(106, y), new Vector2(330, 44));
            v.textWrappingMode = TextWrappingModes.NoWrap;
            v.enableAutoSizing = true;
            v.fontSizeMin = 18;
            v.fontSizeMax = 24;
            return v;
        }

        // ------------------------------------------------------------------ main menu

        static void BuildMainMenuInOpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "MainMenu")
            {
                Debug.LogError("[OHI] Open the MainMenu scene to rebuild it.");
                return;
            }

            // Retire the old canvas and the old one-off managers (replaced by MainMenuController/SettingsPanel).
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "Canvas" || go.name == "MainMenuUI" || go.name == "SoundManager" || go.name == "ExitManager" || go.name == "MenuFlowchart")
                    Object.DestroyImmediate(go);

            var canvas = MakeCanvas("MainMenuUI", null, 0);
            var root = canvas.transform;

            // Key art, cropped to the illustration (inside its white frame) and to 16:9 so the
            // logo on the left is always fully visible; envelope-fits any screen aspect.
            var art = Stretch("KeyArt", root);
            var raw = art.gameObject.AddComponent<RawImage>();
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(KeyArtPath);
            raw.color = UITheme.Hex("F0F0F0");   // slightly calmer key art
            float tw = raw.texture.width, th = raw.texture.height;
            float cropH = th * 0.87f, cropW = cropH * 16f / 9f;
            raw.uvRect = new Rect(0.04f, (th - cropH) / 2f / th, cropW / tw, cropH / th);
            raw.raycastTarget = false;
            var fit = art.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 16f / 9f;

            // A soft cream wash over the right of the art so the menu card sits on a calmer zone (3.2 item 9).
            var wash = Rect("Wash", root, new Vector2(0.55f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            var washImg = Img(wash, GeneratedSprites.Load(GeneratedSprites.GradientH), UITheme.WithAlpha(UITheme.Cream, 0.35f), false);
            washImg.preserveAspect = false;
            washImg.raycastTarget = false;

            // Menu card
            var menuRoot = Rect("Menu", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-420, 0), new Vector2(600, 760));
            menuRoot.gameObject.AddComponent<CanvasGroup>();
            var card = Stretch("Card", menuRoot);
            Img(card, S("Panels/rounded_panel_white.png"), UITheme.WithAlpha(UITheme.Cream, 0.96f));
            var menuPanel = menuRoot.gameObject.AddComponent<UIPanel>();
            Set(menuPanel, "content", menuRoot);

            Txt("Welcome", menuRoot, "Welcome, Innovator", true, 46, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 302), new Vector2(520, 64));
            Txt("Tagline", menuRoot, "Your decisions shape what happens next.", false, 27, UITheme.Teal, TextAlignmentOptions.Center, new Vector2(0, 246), new Vector2(520, 44));
            var divider = Center("Divider", menuRoot, new Vector2(0, 206), new Vector2(440, 6));
            Img(divider, S("Decorative/divider_line.png"), UITheme.WithAlpha(UITheme.TealLight, 0.9f), false).preserveAspect = false;

            // Icons only where they add meaning (Settings, Back); text buttons need none (U27).
            var size = new Vector2(460, 104);
            var newGame = Btn("NewGameButton", menuRoot, "Green", "New Game", null, new Vector2(0, 116), size, 38f);
            TextMeshProUGUI contLabel;
            var cont = Btn("ContinueButton", menuRoot, "Blue", "Continue", null, new Vector2(0, 0), size, 38f, out contLabel);
            contLabel.textWrappingMode = TextWrappingModes.Normal;
            contLabel.lineSpacing = -12f;
            var settingsBtn = Btn("SettingsButton", menuRoot, "Navy", "Settings", "Icons/settings_icon.png", new Vector2(0, -116), size, 38f);
            var exit = Btn("ExitButton", menuRoot, "Slate", "Exit", null, new Vector2(0, -232), size, 38f);
            // Disclaimer: opaque muted ink (5.3:1 on cream), clear of the card's bottom edge (U25).
            Txt("Footer", menuRoot, "An educational game, not medical advice.", false, 22, UITheme.InkMuted, TextAlignmentOptions.Center, new Vector2(0, -334), new Vector2(540, 32));

            var settings = InstantiatePrefab("SettingsPanel", root);
            var confirm = InstantiatePrefab("ConfirmDialog", root);

            var controller = canvas.gameObject.AddComponent<MainMenuController>();
            Set(controller, "menuMusic", AssetDatabase.LoadAssetAtPath<AudioClip>(MenuMusicPath));
            Set(controller, "newGameButton", newGame);
            Set(controller, "continueButton", cont);
            Set(controller, "continueLabel", contLabel);
            Set(controller, "settingsButton", settingsBtn);
            Set(controller, "exitButton", exit);
            Set(controller, "settings", settings.GetComponent<SettingsPanel>());
            Set(controller, "confirm", confirm.GetComponent<ConfirmDialog>());
            Set(controller, "menuPanel", menuPanel);

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                es.transform.SetAsLastSibling();
            }

            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}

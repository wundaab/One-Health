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
        static void ApplySpriteBorders()
        {
            var borders = new Dictionary<string, int>
            {
                { "Panels/rounded_panel_white.png", 48 }, { "Panels/card_background.png", 48 },
                { "Panels/header_panel_green.png", 40 }, { "Panels/generic_panel_dark.png", 48 },
                { "Panels/info_card_white.png", 44 }, { "Settings/panel.png", 48 },
                { "Menus/Pause_Menu/panel.png", 48 }, { "HUD/stat_background.png", 40 },
                { "Settings/slider_background.png", 22 }, { "Settings/slider_fill.png", 22 },
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
            var panel = PanelRoot("SettingsPanel", parent, out c, new Vector2(680, 520), S("Settings/panel.png"));
            Txt("Title", c, "Settings", true, 52, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 178), new Vector2(560, 70));
            var divider = Center("Divider", c, new Vector2(0, 132), new Vector2(520, 6));
            Img(divider, S("Decorative/divider_line.png"), UITheme.WithAlpha(UITheme.TealLight, 0.9f), false).preserveAspect = false;

            TMP_Text musicState, sfxState;
            var music = ToggleRow(c, "Music", "Settings/music_icon.png", new Vector2(0, 52), out musicState);
            var sfx = ToggleRow(c, "Sound Effects", "Settings/sfx_icon.png", new Vector2(0, -52), out sfxState);
            var back = Btn("BackButton", c, "Navy", "Back", "Icons/back_arrow_icon.png", new Vector2(0, -178), new Vector2(320, 96));

            var sp = panel.gameObject.AddComponent<SettingsPanel>();
            Set(sp, "panel", panel);
            Set(sp, "musicToggle", music);
            Set(sp, "sfxToggle", sfx);
            Set(sp, "musicState", musicState);
            Set(sp, "sfxState", sfxState);
            Set(sp, "backButton", back);
            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        static Toggle ToggleRow(Transform parent, string label, string icon, Vector2 pos, out TMP_Text state)
        {
            var row = Center(label.Replace(" ", "") + "Row", parent, pos, new Vector2(560, 88));
            var ic = Center("Icon", row, new Vector2(-236, 0), new Vector2(76, 76));
            Img(ic, S(icon), Color.white, false);
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
            Chip(hud, StatKind.Resistance, "Resistance", "HUD/resistance_icon.png", "How serious is the resistance problem? Higher is worse. (0–100)", GameState.ResistanceMax, true, true);
            var capacity = Chip(hud, StatKind.FlowchartInteger, "Capacity", "Assets/Art/02_Scenario_2/06_HUD/capacity_icon_normal.png","How much the clinic team can still take on. (0–100)", 100, true, false);
            Set(capacity.GetComponent<StatChip>(), "variableKey", "ClinicResponseCapacity");   // shown only in scenes that track it

            var h = hud.gameObject.AddComponent<StatHUD>();
            Set(h, "visibleOnStart", false);
        }

        static RectTransform Chip(Transform parent, StatKind kind, string label, string icon, string tooltip, int max, bool bar, bool higherIsWorse)
        {
            var rt = Center(label.Replace(" ", "") + "Chip", parent, Vector2.zero, new Vector2(248, 116));
            var bg = Img(rt, S("HUD/stat_background.png"), Color.white);
            bg.raycastTarget = true;

            Sprite iconSprite = S(icon);
            var ic = Center("Icon", rt, new Vector2(-78, 0), new Vector2(64, 64));
            Img(ic, iconSprite, Color.white, false).raycastTarget = false;

            Txt("Label", rt, label.ToUpperInvariant(), true, 17, UITheme.TealLight, TextAlignmentOptions.MidlineLeft, new Vector2(34, 28), new Vector2(150, 26));
            var value = Txt("Value", rt, "0", true, 34, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(34, -4), new Vector2(150, 42));
            var status = Txt("Status", rt, "", true, 16, UITheme.Coral, TextAlignmentOptions.MidlineRight, new Vector2(60, 28), new Vector2(98, 24));
            status.gameObject.SetActive(false);

            Image fill = null;
            if (bar)
            {
                var track = Center("Bar", rt, new Vector2(34, -36), new Vector2(150, 10));
                Img(track, null, UITheme.Sand);
                var f = Stretch("Fill", track);
                // Filled images need a sprite to render the fill amount.
                fill = Img(f, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                    higherIsWorse ? UITheme.Coral : UITheme.LeafGreen, false);
                fill.preserveAspect = false;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.raycastTarget = false;
            }

            var delta = Txt("Delta", rt, "", true, 26, UITheme.LeafGreen, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(200, 36));
            var drt = delta.rectTransform;
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0f);
            drt.anchoredPosition = new Vector2(0, -24);

            var tip = Rect("Tooltip", rt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, -78), new Vector2(330, 74));
            Img(tip, S("Panels/generic_panel_dark.png"), Color.white);
            Txt("Text", tip, tooltip, false, 20, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(296, 60));
            tip.gameObject.SetActive(false);

            var chip = rt.gameObject.AddComponent<StatChip>();
            Set(chip, "kind", (int)kind);
            Set(chip, "maxValue", max);
            Set(chip, "higherIsWorse", higherIsWorse);
            Set(chip, "valueText", value);
            Set(chip, "statusText", status);
            Set(chip, "fill", fill);
            Set(chip, "deltaText", delta);
            Set(chip, "tooltip", tip.gameObject);
            return rt;
        }

        static PauseMenu BuildPause(Transform root)
        {
            // Always-active host: PauseMenu must run (Esc key, pause button) while its panel is hidden.
            var host = Stretch("Pause", root);
            var pb = Btn("PauseButton", host, "Navy", "Pause", "Icons/settings_icon.png", Vector2.zero, new Vector2(196, 84), 28f);
            var prt = (RectTransform)pb.transform;
            prt.anchorMin = prt.anchorMax = Vector2.one;
            prt.anchoredPosition = new Vector2(-122, -76);

            RectTransform c;
            var panel = PanelRoot("PausePanel", host, out c, new Vector2(560, 760), S("Menus/Pause_Menu/panel.png"), 0.65f);
            Txt("Title", c, "Paused", true, 54, Color.white, TextAlignmentOptions.Center, new Vector2(0, 300), new Vector2(460, 70));
            var size = new Vector2(420, 92);
            var resume = Btn("ResumeButton", c, "Green", "Resume", "Icons/continue_arrow_icon.png", new Vector2(0, 186), size);
            var settings = Btn("SettingsButton", c, "Navy", "Settings", "Icons/settings_icon.png", new Vector2(0, 80), size);
            TextMeshProUGUI nextLabel;
            var next = Btn("NextButton", c, "Gold", "Next", "Icons/lock_icon.png", new Vector2(0, -26), size, 30f, out nextLabel);
            var lockIcon = next.transform.Find("Icon");
            var menu = Btn("MainMenuButton", c, "Blue", "Main Menu", "Icons/back_arrow_icon.png", new Vector2(0, -132), size);
            var exit = Btn("ExitButton", c, "Slate", "Exit Game", null, new Vector2(0, -238), size);
            var hint = Txt("Hint", c, "Press Esc to resume", false, 22, UITheme.TealLight, TextAlignmentOptions.Center, new Vector2(0, -318), new Vector2(460, 60));

            var pm = host.gameObject.AddComponent<PauseMenu>();
            Set(pm, "panel", panel);
            Set(pm, "pauseButton", pb);
            Set(pm, "resumeButton", resume);
            Set(pm, "settingsButton", settings);
            Set(pm, "mainMenuButton", menu);
            Set(pm, "exitButton", exit);
            Set(pm, "nextButton", next);
            Set(pm, "nextLabel", nextLabel);
            Set(pm, "nextLockIcon", lockIcon.gameObject);
            Set(pm, "hintText", hint);

            panel.gameObject.SetActive(false);
            return pm;
        }

        static VisualCard BuildVisualCard(Transform root)
        {
            RectTransform c;
            var panel = PanelRoot("VisualCard", root, out c, new Vector2(1080, 820), S("Panels/rounded_panel_white.png"), 0.55f);
            var title = Txt("Title", c, "", true, 46, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 334), new Vector2(940, 64));
            var imgRt = Center("Image", c, new Vector2(0, 70), new Vector2(900, 440));
            var img = Img(imgRt, null, Color.white, false);
            img.preserveAspect = true;
            var caption = Txt("Caption", c, "", false, 30, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, -230), new Vector2(940, 120));
            var cont = Btn("ContinueButton", c, "Green", "Continue", "Icons/continue_arrow_icon.png", new Vector2(0, -338), new Vector2(340, 92));

            var vc = panel.gameObject.AddComponent<VisualCard>();
            Set(vc, "panel", panel);
            Set(vc, "title", title);
            Set(vc, "image", img);
            Set(vc, "caption", caption);
            Set(vc, "continueButton", cont);
            panel.gameObject.SetActive(false);
            return vc;
        }

        static ResultsPanel BuildResults(Transform root)
        {
            RectTransform c;
            var panel = PanelRoot("ResultsPanel", root, out c, new Vector2(1320, 900), S("Panels/rounded_panel_white.png"), 0.7f);

            var ribbon = Center("Header", c, new Vector2(0, 382), new Vector2(560, 100));
            Img(ribbon, S("Panels/header_panel_green.png"), Color.white);
            var header = Txt("Text", ribbon, "Scenario Complete", true, 42, Color.white, TextAlignmentOptions.Center, new Vector2(0, 4), new Vector2(520, 70));
            var title = Txt("ScenarioTitle", c, "", true, 36, UITheme.Teal, TextAlignmentOptions.Center, new Vector2(0, 292), new Vector2(1180, 50));
            var summary = Txt("Summary", c, "", false, 28, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 198), new Vector2(1160, 130));
            summary.enableAutoSizing = true;
            summary.fontSizeMin = 18;
            summary.fontSizeMax = 28;
            summary.overflowMode = TextOverflowModes.Ellipsis;

            // Left column: final state
            var left = Center("FinalState", c, new Vector2(-300, -66), new Vector2(560, 360));
            Img(left, S("Panels/info_card_white.png"), Color.white);
            Txt("Heading", left, "Your resources", true, 30, UITheme.TealDark, TextAlignmentOptions.MidlineLeft, new Vector2(10, 140), new Vector2(460, 44));
            var money = StatRow(left, "Money", "HUD/money_icon.png", 72);
            var time = StatRow(left, "Time", "HUD/time_icon.png", 6);
            var trust = StatRow(left, "Trust", "HUD/trust_icon.png", -60);
            var res = StatRow(left, "Resistance", "HUD/resistance_icon.png", -126);

            // Right column: learning and unlocks
            var right = Center("Learning", c, new Vector2(300, -66), new Vector2(560, 360));
            Img(right, S("Panels/info_card_white.png"), Color.white);
            Txt("LearnedHeading", right, "What you learned", true, 30, UITheme.TealDark, TextAlignmentOptions.MidlineLeft, new Vector2(10, 140), new Vector2(460, 44));
            var learned = Txt("Learned", right, "", false, 26, UITheme.TealDark, TextAlignmentOptions.TopLeft, new Vector2(10, 52), new Vector2(460, 128));
            Txt("UnlockedHeading", right, "Unlocked", true, 30, UITheme.TealDark, TextAlignmentOptions.MidlineLeft, new Vector2(10, -42), new Vector2(460, 44));
            var unlocked = Txt("Unlocked", right, "", false, 26, UITheme.TealDark, TextAlignmentOptions.TopLeft, new Vector2(10, -114), new Vector2(460, 96));
            foreach (var t in new[] { learned, unlocked })
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = 16;
                t.fontSizeMax = 26;
                t.overflowMode = TextOverflowModes.Truncate;
            }

            var size = new Vector2(330, 96);
            TextMeshProUGUI replayLabel, nextLabel;
            var replay = Btn("ReplayButton", c, "Blue", "Replay", null, new Vector2(-380, -362), size, 34f, out replayLabel);
            var menu = Btn("MainMenuButton", c, "Slate", "Main Menu", null, new Vector2(0, -362), size);
            var next = Btn("NextButton", c, "Green", "Next", "Icons/continue_arrow_icon.png", new Vector2(380, -362), size, 34f, out nextLabel);

            var rp = panel.gameObject.AddComponent<ResultsPanel>();
            Set(rp, "panel", panel);
            Set(rp, "header", header);
            Set(rp, "scenarioTitle", title);
            Set(rp, "summary", summary);
            Set(rp, "moneyRow", money);
            Set(rp, "timeRow", time);
            Set(rp, "trustRow", trust);
            Set(rp, "resistanceRow", res);
            Set(rp, "learnedText", learned);
            Set(rp, "unlockedText", unlocked);
            Set(rp, "replayButton", replay);
            Set(rp, "replayLabel", replayLabel);
            Set(rp, "menuButton", menu);
            Set(rp, "nextButton", next);
            Set(rp, "nextLabel", nextLabel);
            panel.gameObject.SetActive(false);
            return rp;
        }

        static TMP_Text StatRow(Transform parent, string label, string icon, float y)
        {
            var ic = Center(label + "Icon", parent, new Vector2(-216, y), new Vector2(50, 50));
            Img(ic, S(icon), Color.white, false);
            Txt(label + "Label", parent, label, true, 26, UITheme.Teal, TextAlignmentOptions.MidlineLeft, new Vector2(-110, y), new Vector2(160, 40));
            var v = Txt(label + "Value", parent, "", false, 26, UITheme.TealDark, TextAlignmentOptions.MidlineRight, new Vector2(90, y), new Vector2(340, 40));
            v.textWrappingMode = TextWrappingModes.NoWrap;
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
            float tw = raw.texture.width, th = raw.texture.height;
            float cropH = th * 0.87f, cropW = cropH * 16f / 9f;
            raw.uvRect = new Rect(0.04f, (th - cropH) / 2f / th, cropW / tw, cropH / th);
            raw.raycastTarget = false;
            var fit = art.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 16f / 9f;

            // Menu card
            var menuRoot = Rect("Menu", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-420, 0), new Vector2(600, 720));
            menuRoot.gameObject.AddComponent<CanvasGroup>();
            var card = Stretch("Card", menuRoot);
            Img(card, S("Panels/rounded_panel_white.png"), UITheme.WithAlpha(UITheme.Cream, 0.96f));
            var menuPanel = menuRoot.gameObject.AddComponent<UIPanel>();
            Set(menuPanel, "content", menuRoot);

            Txt("Welcome", menuRoot, "Welcome, Innovator", true, 46, UITheme.TealDark, TextAlignmentOptions.Center, new Vector2(0, 282), new Vector2(520, 64));
            Txt("Tagline", menuRoot, "Your decisions shape what happens next.", false, 27, UITheme.Teal, TextAlignmentOptions.Center, new Vector2(0, 226), new Vector2(520, 44));
            var divider = Center("Divider", menuRoot, new Vector2(0, 186), new Vector2(440, 6));
            Img(divider, S("Decorative/divider_line.png"), UITheme.WithAlpha(UITheme.TealLight, 0.9f), false).preserveAspect = false;

            var size = new Vector2(460, 104);
            var newGame = Btn("NewGameButton", menuRoot, "Green", "New Game", "Icons/continue_arrow_icon.png", new Vector2(0, 100), size, 38f);
            var cont = Btn("ContinueButton", menuRoot, "Blue", "Continue", "Icons/clipboard_icon.png", new Vector2(0, -16), size, 38f);
            var settingsBtn = Btn("SettingsButton", menuRoot, "Navy", "Settings", "Icons/settings_icon.png", new Vector2(0, -132), size, 38f);
            var exit = Btn("ExitButton", menuRoot, "Slate", "Exit", "Icons/back_arrow_icon.png", new Vector2(0, -248), size, 38f);
            Txt("Footer", menuRoot, "Commercialization Lab MVP", false, 20, UITheme.WithAlpha(UITheme.TealDark, 0.55f), TextAlignmentOptions.Center, new Vector2(0, -326), new Vector2(520, 30));

            var settings = InstantiatePrefab("SettingsPanel", root);
            var confirm = InstantiatePrefab("ConfirmDialog", root);

            var controller = canvas.gameObject.AddComponent<MainMenuController>();
            Set(controller, "menuMusic", AssetDatabase.LoadAssetAtPath<AudioClip>(MenuMusicPath));
            Set(controller, "newGameButton", newGame);
            Set(controller, "continueButton", cont);
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

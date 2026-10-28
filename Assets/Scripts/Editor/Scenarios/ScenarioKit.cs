using System.Collections.Generic;
using System.Linq;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Builds scenario scenes and flowcharts from code: creates the scene from the Scenario 2
    /// scene (camera, views, stage, custom dialogs, GameUI), adds characters, and offers short
    /// helpers for Say / Costed Option / Apply Decision / flags / conditions.
    /// </summary>
    public class ScenarioKit
    {
        // Fungus enum indices
        public const int Assign = 0, Add = 2, Sub = 3;
        public const int Eq = 0, Ne = 1, Lt = 2, Gt = 3, Le = 4, Ge = 5;

        public Flowchart fc;
        public SayDialog sayDialog;
        public MenuDialog menuDialog;
        readonly Dictionary<string, Character> characters = new Dictionary<string, Character>();
        int column, row;

        // ------------------------------------------------------------------ scene

        /// <summary>Creates (or recreates) a scenario scene by copying SC02 and clearing its story.</summary>
        /// <param name="grade">Colour multiplied into the background art (per-scenario mood, UI critique U29).</param>
        public static ScenarioKit CreateScene(string scenePath, string backgroundSpritePath, Color? grade = null)
        {
            EditorSceneManager.SaveOpenScenes();
            const string template = "Assets/Scenes/SC02.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            {
                // Overwrite the scene file but keep its .meta, so the scene GUID (used by Build Settings
                // and version control) stays the same across rebuilds.
                EditorSceneManager.OpenScene(template, OpenSceneMode.Single);
                System.IO.File.Copy(template, scenePath, true);
                AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);
            }
            else
                AssetDatabase.CopyAsset(template, scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var kit = new ScenarioKit();
            kit.fc = Object.FindAnyObjectByType<Flowchart>();
            kit.sayDialog = Object.FindAnyObjectByType<SayDialog>(FindObjectsInactive.Include);
            kit.menuDialog = Object.FindAnyObjectByType<MenuDialog>(FindObjectsInactive.Include);

            // Clear the copied story: blocks, commands, variables, event handlers.
            foreach (var b in kit.fc.GetComponents<Block>().ToList()) E.DeleteBlock(b);
            foreach (var c in kit.fc.GetComponents<Command>().ToList()) Undo.DestroyObjectImmediate(c);
            foreach (var v in kit.fc.Variables.ToList()) { kit.fc.Variables.Remove(v); Undo.DestroyObjectImmediate(v); }
            foreach (var h in kit.fc.GetComponents<EventHandler>().ToList()) Undo.DestroyObjectImmediate(h);

            // Scenario 2's case board is specific to that scenario.
            var board = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "CaseBoardPanel" && g.scene.IsValid());
            if (board != null) Undo.DestroyObjectImmediate(board);

            // Background
            var bg = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include).FirstOrDefault(r => r.transform.parent != null && r.transform.parent.name == "Backgrounds");
            var sprite = AssetDatabase.LoadAllAssetsAtPath(backgroundSpritePath).OfType<Sprite>().FirstOrDefault();
            if (bg != null && sprite != null)
            {
                bg.sprite = sprite;
                bg.gameObject.name = sprite.name;
                // Grade and crop about the view the scenario fades to (the scene copies Scenario 2's clinic crop).
                var view = Object.FindAnyObjectByType<View>(FindObjectsInactive.Include);
                var centre = view != null ? view.transform.position : bg.transform.position;
                if (sprite.name.StartsWith("Kivu_Community"))
                    ArtPipeline.GradeCommunity(bg, centre, grade ?? ArtPipeline.CommunityGrade);
                else
                    ArtPipeline.GradeClinic(bg, centre);
            }
            return kit;
        }

        public void Save()
        {
            // Questions stay on screen while their options are shown (UI critique U4).
            E.KeepPromptsWithMenus(fc);
            // Inline all-caps headings ("WATER REPORT — ...") shown as a small label line (U31).
            E.HeadSayLines(fc);
            foreach (var b in fc.GetComponents<Block>()) { b.UpdateIndentLevels(); EditorUtility.SetDirty(b); }
            foreach (var c in fc.GetComponents<Command>()) EditorUtility.SetDirty(c);
            EditorUtility.SetDirty(fc);
            EditorSceneManager.MarkSceneDirty(fc.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
        }

        /// <summary>Adds a scene to the end of Build Settings, or refreshes its entry (GUID) if already listed.</summary>
        public static void AddToBuild(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == scenePath)) scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            // Re-resolve every GUID from its path. Assigning an edited list keeps a stale GUID for a path that is
            // already listed, so clear the list first.
            var fresh = scenes.Select(s => new EditorBuildSettingsScene(new GUID(AssetDatabase.AssetPathToGUID(s.path)), s.enabled)).ToArray();
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
            EditorBuildSettings.scenes = fresh;
        }

        // ------------------------------------------------------------------ characters

        /// <summary>Portraits reused for characters the art team has not drawn yet (MVP placeholders).</summary>
        static readonly Dictionary<string, string> PortraitArt = new Dictionary<string, string>
        {
            // Samuel speaks for the community in Scenario 1; he returns as its representative.
            { "Samuel", "Assets/Art/01_Scenario_1/01_Characters/Samuel/samuel_neutral.png" },
            { "Animal-health worker", "Assets/Art/02_Scenario_2/sector_contact/Sector_contact.png" },
            // No character art yet: a sector icon in the portrait circle tells the player who is speaking
            // (people, animals, environment) until the art team draws these characters.
            { "Radio operator", "Assets/Art/01_Scenario_1/04_UI/Icons/information_icon.png" },
            { "Farmer", "Assets/Art/01_Scenario_1/04_UI/Icons/animals_icon.png" },
            { "Water collector", "Assets/Art/01_Scenario_1/04_UI/Icons/community_icon.png" },
            { "Environmental officer", "Assets/Art/01_Scenario_1/04_UI/Icons/environment_icon.png" },
        };

        /// <summary>Character by display name; created from an existing character if missing.</summary>
        public Character Who(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            Character c;
            if (characters.TryGetValue(name, out c)) return c;
            c = Object.FindObjectsByType<Character>(FindObjectsInactive.Include).FirstOrDefault(x => x.NameText == name);
            if (c == null)
            {
                var template = Object.FindObjectsByType<Character>(FindObjectsInactive.Include).First();
                var go = Object.Instantiate(template.gameObject, template.transform.parent);
                go.name = name;
                c = go.GetComponent<Character>();
                E.Set(c, "nameText", name);
                var so = new SerializedObject(c);
                var portraits = so.FindProperty("portraits");
                portraits.arraySize = 0;
                string path;
                var sprite = PortraitArt.TryGetValue(name, out path) ? AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault() : null;
                if (sprite != null)
                {
                    portraits.arraySize = 1;
                    portraits.GetArrayElementAtIndex(0).objectReferenceValue = sprite;
                }
                so.FindProperty("nameColor").colorValue = NameColor(name);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            characters[name] = c;
            return c;
        }

        static Color NameColor(string name)
        {
            switch (name)
            {
                case "Radio operator": return UITheme.Navy;
                case "Farmer": return UITheme.Hex("8A5A2B");
                case "Animal-health worker": return UITheme.LeafGreen;
                case "Environmental officer": return UITheme.Hex("2F6FB0");
                case "Water collector": return UITheme.Hex("2F8FB0");
                case "Samuel": return UITheme.Amber;
                default: return UITheme.TealDark;
            }
        }

        // ------------------------------------------------------------------ blocks

        /// <summary>New block, laid out in columns of 12 for readability in the Fungus window.</summary>
        public Block Block(string name)
        {
            var b = E.NewBlock(fc, name, new Vector2(260 * column, 90 * row));
            if (++row >= 12) { row = 0; column++; }
            return b;
        }

        /// <summary>The block that runs when the scene starts (Fungus Game Started event).</summary>
        public Block StartBlock(string name)
        {
            var b = Block(name);
            var h = Undo.AddComponent<GameStarted>(fc.gameObject);
            h.hideFlags = HideFlags.HideInInspector;
            h.ParentBlock = b;
            b._EventHandler = h;
            return b;
        }

        public Say Say(Block b, string text, string who = null)
        {
            var s = E.Add<Say>(b);
            var c = Who(who);
            E.Set(s, "storyText", text);
            E.Set(s, "character", c);
            // Fungus shows a portrait only when the Say names one: use the character's first (neutral) portrait.
            if (c != null && c.Portraits != null && c.Portraits.Count > 0) E.Set(s, "portrait", c.Portraits[0]);
            E.Set(s, "setSayDialog", sayDialog);
            return s;
        }

        public void Card(Block b, string title, string caption, Sprite image = null)
        {
            var c = E.Add<ShowVisualCard>(b);
            E.Set(c, "title", title);
            E.Set(c, "caption", caption);
            E.Set(c, "image", image);
        }

        public Fungus.Menu Plain(Block b, string text, Block target, BooleanVariable hide = null)
        {
            var m = E.Add<Fungus.Menu>(b);
            E.Set(m, "text", text);
            E.Set(m, "targetBlock", target);
            E.Set(m, "setMenuDialog", menuDialog);
            if (hide != null) E.Set(m, "hideThisOption.booleanRef", hide);
            return m;
        }

        public CostedOption Option(Block b, string text, Block target, StatDelta d, BooleanVariable hide = null)
        {
            var m = E.Add<CostedOption>(b);
            E.Set(m, "text", text);
            E.Set(m, "targetBlock", target);
            E.Set(m, "setMenuDialog", menuDialog);
            E.Set(m, "money", d.money);
            E.Set(m, "timeWeeks", d.timeWeeks);
            E.Set(m, "trust", d.trust);
            E.Set(m, "resistance", d.resistance);
            E.Set(m, "capacity", d.capacity);
            if (hide != null) E.Set(m, "hideThisOption.booleanRef", hide);
            return m;
        }

        public void FailIfNone(Block b, Block failure)
        {
            var f = E.Add<FailIfNoneAffordable>(b);
            E.Set(f, "failureBlock", failure);
        }

        public ApplyDecision Apply(Block b, string id, string action, StatDelta d)
        {
            var a = E.Add<ApplyDecision>(b);
            E.Set(a, "decisionId", id);
            E.Set(a, "actionName", action);
            E.Set(a, "money", d.money);
            E.Set(a, "timeWeeks", d.timeWeeks);
            E.Set(a, "trust", d.trust);
            E.Set(a, "resistance", d.resistance);
            E.Set(a, "capacity", d.capacity);
            return a;
        }

        /// <summary>Shows Trust and Resistance Pressure before the player commits (only where a blueprint asks for it).</summary>
        public CostedOption Preview(CostedOption o) { E.Set(o, "previewOutcomes", true); return o; }

        /// <summary>
        /// Capability bonus on a Costed Option or Apply Decision (same fields on both): Community Coordination
        /// adds Trust to community-engagement choices; Cross-Sector Response lowers a Capacity cost.
        /// </summary>
        public static void Boost(Command c, string capability, int trust, int capacity)
        {
            E.Set(c, "capability", capability);
            E.Set(c, "capabilityTrust", trust);
            E.Set(c, "capabilityCapacity", capacity);
        }

        /// <summary>Tells the player when an unlocked capability changed the result of the last decision.</summary>
        public void CapabilityNote(Block b, string text)
        {
            IfBool(b, "CapabilityUsed", true);
            Say(b, text);
            End(b);
        }

        public void CheckAffordable(Block b, BooleanVariable result, StatDelta d)
        {
            var c = E.Add<OneHealth.CheckAffordable>(b);
            E.Set(c, "result", result);
            E.Set(c, "money", d.money);
            E.Set(c, "timeWeeks", d.timeWeeks);
            E.Set(c, "capacity", d.capacity);
        }

        public void Flag(Block b, string key, string value)
        {
            Var<StringVariable>(key);
            var f = E.Add<SetCampaignFlag>(b);
            E.Set(f, "key", key);
            E.Set(f, "value", value);
        }

        // The star from the flat icon family (the medal's ribbon and ring are misaligned).
        public const string BadgeIconPath = "Assets/Art/01_Scenario_1/04_UI/Icons/star_icon.png";
        public const string NotifySoundPath = "Assets/Fungus/Audio/Click2.wav";

        /// <summary>Awards a badge (recognition only) and announces it on a card with the medal icon.</summary>
        public void Badge(Block b, string id, string reason)
        {
            var a = E.Add<AwardBadge>(b);
            E.Set(a, "badgeId", id);
            Sound(b, NotifySoundPath);
            Card(b, "Badge earned — " + id, reason, AssetDatabase.LoadAllAssetsAtPath(BadgeIconPath).OfType<Sprite>().FirstOrDefault());
        }

        public void Sound(Block b, string path)
        {
            var s = E.Add<PlaySound>(b);
            E.Set(s, "soundClip", AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            E.Set(s, "volume", 1f);
        }

        public void Knowledge(Block b, string id)
        {
            var k = E.Add<RecordKnowledge>(b);
            E.Set(k, "knowledgeId", id);
        }

        public void Call(Block b, Block target) { E.Call(b, -1, target); }

        /// <summary>Moves the camera onto the scene's background view (fading from black).</summary>
        public void FadeToBackground(Block b)
        {
            var view = Object.FindAnyObjectByType<View>(FindObjectsInactive.Include);
            var f = E.Add<FadeToView>(b);
            E.Set(f, "duration", 1f);
            E.Set(f, "targetView", view);
            E.Set(f, "waitUntilFinished", true);
        }

        public void Music(Block b, string path)
        {
            var pm = E.Add<PlayMusic>(b);
            E.Set(pm, "musicClip", AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            E.Set(pm, "loop", true);
            E.Set(pm, "fadeDuration", 1f);
        }

        // ------------------------------------------------------------------ variables

        public T Var<T>(string key) where T : Variable { return E.Var<T>(fc, key); }

        public void SetBool(Block b, string key, bool value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", Var<BooleanVariable>(key));
            E.Set(s, "anyVar.data.booleanData.booleanVal", value);
        }

        public void SetInt(Block b, string key, int op, int value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", Var<IntegerVariable>(key));
            E.Set(s, "setOperator", op);
            E.Set(s, "anyVar.data.integerData.integerVal", value);
        }

        public void SetString(Block b, string key, string value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", Var<StringVariable>(key));
            E.Set(s, "anyVar.data.stringData.stringVal", value);
        }

        // ------------------------------------------------------------------ conditions

        public VariableCondition IfBool(Block b, string key, bool value, bool elseIf = false)
        {
            var c = elseIf ? (VariableCondition)E.Add<ElseIf>(b) : E.Add<If>(b);
            E.SetBoolCondition(c, Var<BooleanVariable>(key), value);
            return c;
        }

        public VariableCondition IfString(Block b, string key, string value, bool elseIf = false, int op = Eq)
        {
            var c = elseIf ? (VariableCondition)E.Add<ElseIf>(b) : E.Add<If>(b);
            E.SetStringCondition(c, Var<StringVariable>(key), value);
            E.Set(c, "conditions.Array.data[0].compareOperator", op);
            return c;
        }

        public VariableCondition IfInt(Block b, string key, int op, int value, bool elseIf = false)
        {
            var c = elseIf ? (VariableCondition)E.Add<ElseIf>(b) : E.Add<If>(b);
            E.EnsureCondition(c);
            E.Set(c, "conditions.Array.data[0].anyVar.variable", Var<IntegerVariable>(key));
            E.Set(c, "conditions.Array.data[0].compareOperator", op);
            E.Set(c, "conditions.Array.data[0].anyVar.data.integerData.integerVal", value);
            return c;
        }

        /// <summary>Compare one integer variable with another, e.g. Resistance &lt;= Entry_Resistance.</summary>
        public VariableCondition IfIntVar(Block b, string key, int op, string otherKey)
        {
            var c = E.Add<If>(b);
            E.EnsureCondition(c);
            E.Set(c, "conditions.Array.data[0].anyVar.variable", Var<IntegerVariable>(key));
            E.Set(c, "conditions.Array.data[0].compareOperator", op);
            E.Set(c, "conditions.Array.data[0].anyVar.data.integerData.integerRef", Var<IntegerVariable>(otherKey));
            return c;
        }

        public void Else(Block b) { E.Add<Else>(b); }
        public void End(Block b) { E.Add<End>(b); }
        public void Stop(Block b) { E.Add<Fungus.Stop>(b); }
    }
}

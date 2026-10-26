using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fungus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Static checks for every scenario flowchart: dead ends, unreachable or empty blocks, broken
    /// targets, placeholder text, undefined {$Variables}, missing scripts and say/menu dialogs.
    /// Menu: One Health/Audit Scenes. Writes Temp/Preview/audit.txt.
    /// </summary>
    public static class ScenarioAudit
    {
        static readonly string[] Scenes =
        {
            "Assets/Scenes/MainMenu.unity", "Assets/Scenes/SC01.unity", "Assets/Scenes/SC02.unity",
            "Assets/Scenes/SC03.unity", "Assets/Scenes/SC04.unity",
        };

        [MenuItem("One Health/Audit Scenes")]
        public static string Run()
        {
            EditorSceneManager.SaveOpenScenes();
            var sb = new StringBuilder();
            foreach (var path in Scenes)
            {
                EditorSceneManager.OpenScene(path);
                sb.AppendLine("=== " + path);
                int missing = 0;
                foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
                    missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missing > 0) sb.AppendLine("  ! missing scripts: " + missing);

                foreach (var fc in Object.FindObjectsByType<Flowchart>(FindObjectsInactive.Include))
                    AuditFlowchart(fc, sb);
            }
            var report = sb.ToString();
            System.IO.Directory.CreateDirectory("Temp/Preview");
            System.IO.File.WriteAllText("Temp/Preview/audit.txt", report);
            return report;
        }

        static void AuditFlowchart(Flowchart fc, StringBuilder sb)
        {
            var blocks = fc.GetComponents<Block>().ToList();
            var varKeys = new HashSet<string>(fc.Variables.Where(v => v != null).Select(v => v.Key));
            var referenced = new HashSet<Block>();
            var buttonTargets = new HashSet<string>();

            // Blocks executed from UI buttons (Flowchart.ExecuteBlock persistent calls)
            foreach (var b in Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include))
                for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                    if (b.onClick.GetPersistentMethodName(i) == "ExecuteBlock" && b.onClick.GetPersistentTarget(i) == fc)
                    {
                        var so = new SerializedObject(b);
                        buttonTargets.Add(so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls.Array.data[" + i + "].m_Arguments.m_StringArgument").stringValue);
                    }

            foreach (var b in blocks)
            {
                if (b.CommandList.Count == 0) sb.AppendLine("  ! empty block: " + b.BlockName);
                for (int i = 0; i < b.CommandList.Count; i++)
                {
                    var c = b.CommandList[i];
                    if (c == null) { sb.AppendLine("  ! null command in " + b.BlockName + "[" + i + "]"); continue; }
                    var targetProp = new SerializedObject(c).FindProperty("targetBlock");
                    if (targetProp != null && targetProp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var t = targetProp.objectReferenceValue as Block;
                        if (t == null && (c is Call || c is Fungus.Menu)) sb.AppendLine("  ! " + c.GetType().Name + " without target in " + b.BlockName + "[" + i + "]");
                        if (t != null) referenced.Add(t);
                    }
                    var fail = c as FailIfNoneAffordable;
                    if (fail != null)
                    {
                        var fb = new SerializedObject(fail).FindProperty("failureBlock").objectReferenceValue as Block;
                        if (fb == null) sb.AppendLine("  ! FailIfNoneAffordable without block in " + b.BlockName);
                        else referenced.Add(fb);
                    }

                    // Text checks
                    var text = TextOf(c);
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (Regex.IsMatch(text, @"\[[A-Z][^\]]*\]") || text.Contains("Option Text") || text.Contains("TODO") || text.Contains("Story text"))
                            sb.AppendLine("  ! placeholder text in " + b.BlockName + ": " + Trim(text));
                        foreach (Match m in Regex.Matches(text, @"\{\$([A-Za-z0-9_]+)\}"))
                            if (!varKeys.Contains(m.Groups[1].Value))
                                sb.AppendLine("  ! undefined variable {$" + m.Groups[1].Value + "} in " + b.BlockName);
                        if (Regex.IsMatch(text, @"\bS0[1-5]\b|SC0[1-5]"))
                            sb.AppendLine("  ? internal scenario code in player text (" + b.BlockName + "): " + Trim(text));
                    }
                    var say = c as Say;
                    if (say != null && new SerializedObject(say).FindProperty("setSayDialog").objectReferenceValue == null && Object.FindAnyObjectByType<SayDialog>(FindObjectsInactive.Include) == null)
                        sb.AppendLine("  ! Say without any SayDialog in scene: " + b.BlockName);
                }

                // Dead end: last command neither moves on nor ends the scenario nor waits on UI.
                var last = b.CommandList.LastOrDefault(x => x != null);
                bool endsWithMenu = b.CommandList.Any(x => x is Fungus.Menu);
                bool terminal = last is Call || last is CompleteScenario || last is FailScenario || last is LoadScene || endsWithMenu
                                || last is SetActive || last is PlaySound || b._EventHandler is ButtonClicked;
                if (last != null && !terminal)
                    sb.AppendLine("  ? block ends without moving on: " + b.BlockName + " (last: " + last.GetType().Name + ")");
            }

            foreach (var b in blocks)
                if (b._EventHandler == null && !referenced.Contains(b) && !buttonTargets.Contains(b.BlockName))
                    sb.AppendLine("  ? unreachable block: " + b.BlockName);

            // Variables never used
            sb.AppendLine("  blocks=" + blocks.Count + " commands=" + fc.GetComponents<Command>().Length + " variables=" + varKeys.Count);
        }

        static string TextOf(Command c)
        {
            if (c is Say) return ((Say)c).GetStandardText();
            if (c is Fungus.Menu)
            {
                var p = new SerializedObject(c).FindProperty("text");
                return p != null ? p.stringValue : null;
            }
            var so = new SerializedObject(c);
            foreach (var name in new[] { "caption", "summary", "reason", "title", "scenarioTitle" })
            {
                var p = so.FindProperty(name);
                if (p != null && p.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(p.stringValue)) return p.stringValue;
            }
            return null;
        }

        static string Trim(string s)
        {
            s = s.Replace("\n", " ");
            return s.Length > 90 ? s.Substring(0, 90) + "…" : s;
        }
    }
}

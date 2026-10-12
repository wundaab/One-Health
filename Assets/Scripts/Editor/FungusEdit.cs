using System.Linq;
using System.Text;
using Fungus;
using UnityEditor;
using UnityEngine;

namespace OneHealth.EditorTools
{
    /// <summary>Small helpers for editing Fungus flowcharts from editor code (insert/move/remove commands, set fields).</summary>
    public static class FungusEdit
    {
        public static Flowchart Find(string goName)
        {
            var go = GameObject.Find(goName);
            return go != null ? go.GetComponent<Flowchart>() : Object.FindAnyObjectByType<Flowchart>();
        }

        public static Block B(Flowchart fc, string name)
        {
            var b = fc.FindBlock(name);
            if (b == null) throw new System.Exception("Block not found: " + name);
            return b;
        }

        public static Block NewBlock(Flowchart fc, string name, Vector2 position)
        {
            var b = fc.CreateBlock(position);
            b.BlockName = fc.GetUniqueBlockKey(name, b);
            return b;
        }

        public static T Insert<T>(Block block, int index) where T : Command
        {
            var fc = block.GetFlowchart();
            var cmd = Undo.AddComponent<T>(fc.gameObject);
            cmd.ItemId = fc.NextItemId();
            cmd.ParentBlock = block;
            cmd.hideFlags = HideFlags.HideInInspector;
            if (index < 0 || index > block.CommandList.Count) index = block.CommandList.Count;
            block.CommandList.Insert(index, cmd);
            cmd.OnCommandAdded(block);
            return cmd;
        }

        public static T Add<T>(Block block) where T : Command { return Insert<T>(block, -1); }

        public static void Remove(Command cmd)
        {
            var block = cmd.ParentBlock;
            block.CommandList.Remove(cmd);
            Undo.DestroyObjectImmediate(cmd);
        }

        public static void Move(Command cmd, Block to, int index)
        {
            cmd.ParentBlock.CommandList.Remove(cmd);
            cmd.ParentBlock = to;
            if (index < 0 || index > to.CommandList.Count) index = to.CommandList.Count;
            to.CommandList.Insert(index, cmd);
        }

        public static void Set(Object target, string path, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(path);
            if (p == null) throw new System.Exception("No property '" + path + "' on " + target.GetType().Name);
            if (value == null || value is Object) p.objectReferenceValue = (Object)value;
            else if (value is string) p.stringValue = (string)value;
            else if (value is bool) p.boolValue = (bool)value;
            else if (value is float) p.floatValue = (float)value;
            else if (value is int)
            {
                if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = (int)value;
                else p.intValue = (int)value;
            }
            else throw new System.Exception("Unsupported value type " + value.GetType());
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static object Get(Object target, string path)
        {
            var p = new SerializedObject(target).FindProperty(path);
            switch (p.propertyType)
            {
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.ObjectReference: return p.objectReferenceValue;
                case SerializedPropertyType.Boolean: return p.boolValue;
                case SerializedPropertyType.Enum: return p.enumValueIndex;
                default: return p.intValue;
            }
        }

        /// <summary>Copies the say/menu dialog choice from a template so new commands use the custom dialogs.</summary>
        public static Say Say(Block block, int index, string text, Character character, Say template)
        {
            var s = Insert<Say>(block, index);
            Set(s, "storyText", text);
            Set(s, "character", character);
            if (template != null) Set(s, "setSayDialog", Get(template, "setSayDialog"));
            return s;
        }

        public static Fungus.Menu Menu(Block block, int index, string text, Block target, Fungus.Menu template)
        {
            var m = Insert<Fungus.Menu>(block, index);
            Set(m, "text", text);
            Set(m, "targetBlock", target);
            if (template != null) Set(m, "setMenuDialog", Get(template, "setMenuDialog"));
            return m;
        }

        public static Call Call(Block block, int index, Block target)
        {
            var c = Insert<Call>(block, index);
            Set(c, "targetBlock", target);
            return c;
        }

        /// <summary>If/ElseIf on a string variable equality.</summary>
        public static void EnsureCondition(VariableCondition cond)
        {
            var so = new SerializedObject(cond);
            var list = so.FindProperty("conditions");
            if (list.arraySize == 0) { list.arraySize = 1; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        public static void SetBoolCondition(VariableCondition cond, Variable variable, bool value)
        {
            EnsureCondition(cond);
            Set(cond, "conditions.Array.data[0].anyVar.variable", variable);
            Set(cond, "conditions.Array.data[0].anyVar.data.booleanData.booleanVal", value);
        }

        public static void SetStringCondition(VariableCondition cond, Variable variable, string value)
        {
            EnsureCondition(cond);
            Set(cond, "conditions.Array.data[0].anyVar.variable", variable);
            Set(cond, "conditions.Array.data[0].anyVar.data.stringData.stringVal", value);
        }

        public static void Finish(Flowchart fc)
        {
            foreach (var b in fc.GetComponents<Block>())
            {
                b.UpdateIndentLevels();
                EditorUtility.SetDirty(b);
            }
            foreach (var c in fc.GetComponents<Command>()) EditorUtility.SetDirty(c);
            EditorUtility.SetDirty(fc);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fc.gameObject.scene);
        }

        /// <summary>Readable dump of a block for verification.</summary>
        public static string Dump(Block b)
        {
            var sb = new StringBuilder("== " + b.BlockName + "\n");
            for (int i = 0; i < b.CommandList.Count; i++)
            {
                var c = b.CommandList[i];
                sb.Append("  [" + i + "] " + new string(' ', c.IndentLevel * 2) + c.GetType().Name + ": " + c.GetSummary());
                var say = c as Say;
                if (say != null && say._Character != null) sb.Append("  (" + say._Character.NameText + ")");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string DumpAll(Flowchart fc)
        {
            var sb = new StringBuilder();
            foreach (var b in fc.GetComponents<Block>().OrderBy(x => x._NodeRect.y).ThenBy(x => x._NodeRect.x))
                sb.Append(Dump(b));
            return sb.ToString();
        }
    }
}

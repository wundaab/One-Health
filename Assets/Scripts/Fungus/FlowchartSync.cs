using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Copies the persistent GameState into a flowchart's variables so If/Say commands can use it.</summary>
    public static class FlowchartSync
    {
        public static void Push(Flowchart flowchart)
        {
            var s = GameSession.State;
            SetInt(flowchart, "Money", s.money);
            SetInt(flowchart, "Time", s.timeWeeks);
            SetInt(flowchart, "Trust", s.trust);
            SetInt(flowchart, "Resistance", s.resistance);

            // Inherited knowledge and capabilities, e.g. Unlocked_CommunityEngagement, for Menu
            // "interactable" conditions and If checks in later scenarios.
            foreach (var id in s.knowledge) SetBool(flowchart, "Knowledge_" + id.Replace(" ", ""), true);
            foreach (var id in s.unlocks) SetBool(flowchart, "Unlocked_" + id.Replace(" ", ""), true);
        }

        public static void SetInt(Flowchart flowchart, string key, int value)
        {
            var v = flowchart.GetVariable<IntegerVariable>(key);
            if (v != null) v.Value = value;
        }

        public static void SetString(Flowchart flowchart, string key, string value)
        {
            var v = flowchart.GetVariable<StringVariable>(key);
            if (v != null) v.Value = value;
        }

        public static void SetBool(Flowchart flowchart, string key, bool value)
        {
            var v = flowchart.GetVariable<BooleanVariable>(key);
            if (v != null) v.Value = value;
        }
    }
}

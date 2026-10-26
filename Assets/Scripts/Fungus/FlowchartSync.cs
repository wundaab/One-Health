using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Copies the persistent GameState into a flowchart's variables so If/Say commands can use it.</summary>
    public static class FlowchartSync
    {
        /// <summary>Scenario-specific capacity counter used by Scenario 2's flowchart.</summary>
        public const string ScenarioCapacityKey = "ClinicResponseCapacity";

        public static void Push(Flowchart flowchart)
        {
            var s = GameSession.State;
            SetInt(flowchart, "Money", s.money);
            SetInt(flowchart, "Time", s.timeWeeks);
            SetInt(flowchart, "Trust", s.trust);
            SetInt(flowchart, "Resistance", s.resistance);
            SetInt(flowchart, "Capacity", s.capacity);

            // Values at scenario entry, for outcome rules such as "Resistance Pressure at the end is
            // no higher than at entry".
            var entry = GameSession.ScenarioStartState ?? s;
            SetInt(flowchart, "Entry_Money", entry.money);
            SetInt(flowchart, "Entry_Trust", entry.trust);
            SetInt(flowchart, "Entry_Resistance", entry.resistance);
            SetInt(flowchart, "Entry_Capacity", entry.capacity);

            // Inherited knowledge, capabilities, badges and story flags (e.g. s04_ending) for If
            // checks and Menu conditions in later scenarios.
            foreach (var id in s.knowledge) SetBool(flowchart, "Knowledge_" + Key(id), true);
            foreach (var id in s.unlocks) SetBool(flowchart, "Unlocked_" + Key(id), true);
            foreach (var id in s.badges) SetBool(flowchart, "Badge_" + Key(id), true);
            foreach (var f in s.flags) SetString(flowchart, f.key, f.value);
        }

        /// <summary>Copies a scenario's own capacity counter back into the campaign Capacity.</summary>
        public static void PullCapacity(Flowchart flowchart)
        {
            if (!flowchart.HasVariable(ScenarioCapacityKey)) return;
            var v = flowchart.GetVariable<IntegerVariable>(ScenarioCapacityKey);
            if (v != null) GameSession.SetCapacity(v.Value);
        }

        public static string Key(string id)
        {
            return id.Replace(" ", "").Replace("-", "");
        }

        public static void SetInt(Flowchart flowchart, string key, int value)
        {
            if (!flowchart.HasVariable(key)) return;   // optional variables: avoid Fungus 'not found' warnings
            var v = flowchart.GetVariable<IntegerVariable>(key);
            if (v != null) v.Value = value;
        }

        public static void SetString(Flowchart flowchart, string key, string value)
        {
            if (!flowchart.HasVariable(key)) return;   // optional variables: avoid Fungus 'not found' warnings
            var v = flowchart.GetVariable<StringVariable>(key);
            if (v != null) v.Value = value;
        }

        public static void SetBool(Flowchart flowchart, string key, bool value)
        {
            if (!flowchart.HasVariable(key)) return;   // optional variables: avoid Fungus 'not found' warnings
            var v = flowchart.GetVariable<BooleanVariable>(key);
            if (v != null) v.Value = value;
        }
    }
}

using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Applies a confirmed decision: changes Money/Time/Trust/Resistance (clamped to the
    /// specified ranges), records the decision, updates the HUD, and writes a player-facing
    /// summary of the change to the "LastChange" string variable for feedback text.
    /// </summary>
    [CommandInfo("One Health", "Apply Decision",
        "Apply a confirmed decision's effects to Money, Time, Trust and Resistance. Values come from the Core Mechanics Specification.")]
    [AddComponentMenu("")]
    public class ApplyDecision : Command
    {
        [Tooltip("Decision ID, e.g. 1A. Also written to the SelectedOption string variable if the flowchart has one.")]
        [SerializeField] protected string decisionId = "";

        [Tooltip("Short description of the action, used in the debug log")]
        [SerializeField] protected string actionName = "";

        [SerializeField] protected int money;
        [Tooltip("Weeks. Negative values consume time.")]
        [SerializeField] protected int timeWeeks;
        [SerializeField] protected int trust;
        [Tooltip("Higher is worse.")]
        [SerializeField] protected int resistance;

        public override void OnEnter()
        {
            var flowchart = GetFlowchart();
            var applied = GameSession.Apply(new StatDelta(money, timeWeeks, trust, resistance),
                "Decision " + decisionId + " (" + actionName + ")");
            GameSession.RecordDecision(decisionId);

            FlowchartSync.Push(flowchart);
            FlowchartSync.SetString(flowchart, "SelectedOption", decisionId);
            FlowchartSync.SetString(flowchart, "LastChange", applied.ToDisplayString());
            Continue();
        }

        public override string GetSummary()
        {
            return decisionId + ": " + new StatDelta(money, timeWeeks, trust, resistance).ToDisplayString();
        }

        public override Color GetButtonColor() { return new Color32(242, 167, 59, 255); }
    }
}

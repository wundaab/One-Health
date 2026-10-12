using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Starts a scenario: sets the checkpoint for Continue, snapshots the inherited state for
    /// Retry/Replay, and copies Money/Time/Trust/Resistance into this flowchart.
    /// </summary>
    [CommandInfo("One Health", "Begin Scenario",
        "Start a scenario using the state inherited from the previous one. Sets the Continue checkpoint and the Retry/Replay snapshot.")]
    [AddComponentMenu("")]
    public class BeginScenario : Command
    {
        [Tooltip("Scenario ID from the design docs, e.g. S01")]
        [SerializeField] protected string scenarioId = "S01";

        [Tooltip("Weeks available for this scenario. -1 keeps the time inherited from the previous scenario.")]
        [SerializeField] protected int timeBudgetWeeks = -1;

        [Tooltip("Show Time on the HUD. Off for scenarios whose design does not use time (e.g. Scenario 2).")]
        [SerializeField] protected bool showTime = true;

        public override void OnEnter()
        {
            GameSession.ShowTimeStat = showTime;
            GameSession.BeginScenario(scenarioId, timeBudgetWeeks);
            FlowchartSync.Push(GetFlowchart());
            Continue();
        }

        public override string GetSummary()
        {
            return scenarioId + (timeBudgetWeeks >= 0 ? " (" + timeBudgetWeeks + " weeks)" : " (inherit time)");
        }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

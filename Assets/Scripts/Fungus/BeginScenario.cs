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

        [Tooltip("Weeks available for this scenario (its deadline). -1 keeps the time inherited from the previous scenario.")]
        [SerializeField] protected int timeBudgetWeeks = -1;

        [Tooltip("Weeks added to the carried-forward Time when this scenario starts. Applied once per scenario, never again on Retry.")]
        [SerializeField] protected int timeGrantWeeks = 0;

        [Tooltip("Show Time on the HUD. Off for scenarios whose design does not use time (e.g. Scenario 2).")]
        [SerializeField] protected bool showTime = true;

        [Tooltip("Show Capacity on the HUD.")]
        [SerializeField] protected bool showCapacity = false;

        [Tooltip("HUD Capacity scale: the bar's maximum (M7). Scenarios 1 and 2 use 100, Scenarios 3 and 4 use 25.")]
        [SerializeField] protected int capacityScaleMax = 100;

        [Tooltip("At or below this Capacity the HUD chip shows Low (M7). Scenarios 1 and 2 use 20, Scenarios 3 and 4 use 5.")]
        [SerializeField] protected int capacityLowThreshold = 20;

        [Tooltip("Set Capacity to this value at the start (-1 keeps the carried value). Scenario 2 starts at 100.")]
        [SerializeField] protected int capacityStart = -1;

        [Tooltip("Convert the carried Capacity to this scenario's scale: base + floor(carried / divisor), clamped to " +
                 "base..max (-1 = off). Scenario 3 converts Scenario 2's 0-100 clinic capacity to a field-team scale (D2).")]
        [SerializeField] protected int capacityRescaleBase = -1;

        [SerializeField] protected int capacityRescaleDivisor = 10;

        [Tooltip("Upper limit of the converted Capacity (-1 = the Capacity maximum).")]
        [SerializeField] protected int capacityRescaleMax = -1;

        [Tooltip("Money added once when this scenario starts, the same for every player (e.g. district response funds). " +
                 "Explain it in the story; the S05 blueprint forbids resource bonuses tied to a previous result.")]
        [SerializeField] protected int moneyGrant = 0;

        public override void OnEnter()
        {
            GameSession.ShowTimeStat = showTime;
            GameSession.ShowCapacityStat = showCapacity;
            GameSession.CapacityScaleMax = capacityScaleMax;
            GameSession.CapacityLowThreshold = capacityLowThreshold;
            GameSession.BeginScenario(scenarioId, timeBudgetWeeks, timeGrantWeeks, capacityStart,
                capacityRescaleBase, capacityRescaleDivisor, moneyGrant, capacityRescaleMax);
            FlowchartSync.Push(GetFlowchart());
            Continue();
        }

        public override string GetSummary()
        {
            return scenarioId + (timeBudgetWeeks >= 0 ? " (" + timeBudgetWeeks + " weeks)" : " (inherit time)")
                + (timeGrantWeeks > 0 ? " +" + timeGrantWeeks + " weeks" : "")
                + (capacityRescaleBase >= 0 ? ", Capacity " + capacityRescaleBase + " + carried/" + capacityRescaleDivisor
                    + (capacityRescaleMax >= 0 ? " (max " + capacityRescaleMax + ")" : "") : "")
                + (moneyGrant != 0 ? ", Money " + StatDelta.Signed(moneyGrant) : "");
        }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Completes the scenario: saves the final state for the next scenario and opens the
    /// results screen (final state, learning, unlocks) with Replay / Next.
    /// </summary>
    [CommandInfo("One Health", "Complete Scenario",
        "Save the scenario outcome for the next scenario and show the results screen with Replay and Next.")]
    [AddComponentMenu("")]
    public class CompleteScenario : Command
    {
        [SerializeField] protected string scenarioId = "S01";
        [SerializeField] protected string scenarioTitle = "";

        [Tooltip("Scene loaded by NEXT")]
        [SerializeField] protected string nextScene = "";

        [Tooltip("Name of the next stage shown on the NEXT button, e.g. BUILD")]
        [SerializeField] protected string nextLabel = "";

        [Tooltip("Outcome summary. Supports {$Variable} substitution.")]
        [TextArea(3, 6)]
        [SerializeField] protected string summary = "";

        public string ScenarioId { get { return scenarioId; } }
        public string NextScene { get { return nextScene; } }
        public string NextLabel { get { return nextLabel; } }

        public override void OnEnter()
        {
            FlowchartSync.PullCapacity(GetFlowchart());
            GameSession.CompleteScenario(scenarioId, nextScene);
            var results = Object.FindAnyObjectByType<ResultsPanel>(FindObjectsInactive.Include);
            if (results == null)
            {
                Debug.LogWarning("[OHI] No ResultsPanel in scene; loading next scene directly.");
                if (!string.IsNullOrEmpty(nextScene)) GameSession.LoadScene(nextScene);
                return;
            }
            results.ShowCompleted(scenarioTitle, GetFlowchart().SubstituteVariables(summary), nextScene, nextLabel);
        }

        public override string GetSummary() { return scenarioId + " -> " + nextScene; }

        public override Color GetButtonColor() { return new Color32(76, 175, 125, 255); }
    }
}

using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Ends the scenario as "Not Completed" with an explanation and a Retry that restores the
    /// scenario's starting state (Progression sheet: failure must teach and allow retry).
    /// </summary>
    [CommandInfo("One Health", "Fail Scenario",
        "Show the Scenario Not Completed screen with an explanation and Retry.")]
    [AddComponentMenu("")]
    public class FailScenario : Command
    {
        [SerializeField] protected string scenarioTitle = "";

        [Tooltip("What happened and why. Supports {$Variable} substitution.")]
        [TextArea(3, 6)]
        [SerializeField] protected string reason = "";

        public override void OnEnter()
        {
            var results = Object.FindAnyObjectByType<ResultsPanel>(FindObjectsInactive.Include);
            if (results != null)
                results.ShowNotCompleted(scenarioTitle, GetFlowchart().SubstituteVariables(reason));
            else
                Debug.LogWarning("[OHI] No ResultsPanel in scene.");
        }

        public override string GetSummary() { return scenarioTitle; }

        public override Color GetButtonColor() { return new Color32(217, 90, 79, 255); }
    }
}

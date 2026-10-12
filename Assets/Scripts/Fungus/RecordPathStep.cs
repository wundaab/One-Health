using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Appends a short description of the player's choice to the "Path" string variable, used by
    /// the Decision Review ("Your path"), and records it in the campaign decision history.
    /// </summary>
    [CommandInfo("One Health", "Record Path Step",
        "Add the player's choice to their decision path for the Decision Review.")]
    [AddComponentMenu("")]
    public class RecordPathStep : Command
    {
        [Tooltip("Short past-tense description, e.g. 'Supported a focused assessment'")]
        [SerializeField] protected string step = "";

        public override void OnEnter()
        {
            var path = GetFlowchart().GetVariable<StringVariable>("Path");
            if (path != null)
                path.Value = string.IsNullOrEmpty(path.Value) ? step : path.Value + "\n→ " + step;
            GameSession.RecordDecision(step);
            Continue();
        }

        public override string GetSummary() { return step; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

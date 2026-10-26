using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Records a story flag that carries into later scenarios (e.g. s04_ending = strong), and
    /// mirrors it into a same-named string variable if the flowchart has one.
    /// </summary>
    [CommandInfo("One Health", "Set Campaign Flag",
        "Record a story flag carried into later scenarios. Value supports {$Variable} substitution.")]
    [AddComponentMenu("")]
    public class SetCampaignFlag : Command
    {
        [SerializeField] protected string key = "";
        [SerializeField] protected string value = "";

        public override void OnEnter()
        {
            var v = GetFlowchart().SubstituteVariables(value);
            GameSession.State.SetFlag(key, v);
            FlowchartSync.SetString(GetFlowchart(), key, v);
            Continue();
        }

        public override string GetSummary() { return key + " = " + value; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Records a knowledge concept the player has encountered (concept records, not a raw score).</summary>
    [CommandInfo("One Health", "Record Knowledge",
        "Record that the player has learned a concept, e.g. AMR, Resistance, One Health. Persists into later scenarios.")]
    [AddComponentMenu("")]
    public class RecordKnowledge : Command
    {
        [SerializeField] protected string knowledgeId = "";

        public override void OnEnter()
        {
            GameSession.AddKnowledge(knowledgeId);
            FlowchartSync.SetBool(GetFlowchart(), "Knowledge_" + FlowchartSync.Key(knowledgeId), true);
            Debug.Log("[OHI] Knowledge recorded: " + knowledgeId);
            Continue();
        }

        public override string GetSummary() { return knowledgeId; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

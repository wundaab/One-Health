using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Unlocks a capability (action, tool or feature) that carries into later scenarios.</summary>
    [CommandInfo("One Health", "Unlock Capability",
        "Unlock a capability that persists into later scenarios, e.g. Community Engagement.")]
    [AddComponentMenu("")]
    public class UnlockCapability : Command
    {
        [SerializeField] protected string capabilityId = "";

        public override void OnEnter()
        {
            GameSession.AddUnlock(capabilityId);
            FlowchartSync.SetBool(GetFlowchart(), "Unlocked_" + FlowchartSync.Key(capabilityId), true);
            Debug.Log("[OHI] Capability unlocked: " + capabilityId);
            Continue();
        }

        public override string GetSummary() { return capabilityId; }

        public override Color GetButtonColor() { return new Color32(76, 175, 125, 255); }
    }
}

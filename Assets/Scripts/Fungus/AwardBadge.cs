using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Awards a badge once (e.g. EVIDENCE TRACKER). Retry clears badges from the abandoned attempt.</summary>
    [CommandInfo("One Health", "Award Badge", "Award a badge once. Badges show on the results screen.")]
    [AddComponentMenu("")]
    public class AwardBadge : Command
    {
        [SerializeField] protected string badgeId = "";

        public override void OnEnter()
        {
            GameSession.AwardBadge(badgeId);
            FlowchartSync.SetBool(GetFlowchart(), "Badge_" + FlowchartSync.Key(badgeId), true);
            Debug.Log("[OHI] Badge awarded: " + badgeId);
            Continue();
        }

        public override string GetSummary() { return badgeId; }

        public override Color GetButtonColor() { return new Color32(242, 167, 59, 255); }
    }
}

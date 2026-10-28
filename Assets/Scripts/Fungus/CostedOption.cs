using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// A menu option that shows its resource cost before the player commits, and is disabled
    /// with an explanation when Money, Time or Capacity cannot cover it (Scenario 3/4
    /// blueprints: "Disable unaffordable options with an explanation"). Trust and Resistance
    /// Pressure stay hidden until the decision is made, unless the blueprint asks for a full
    /// preview. Effects are applied by an Apply Decision in the target block, only after the
    /// player confirms; give that Apply Decision the same capability bonus.
    /// </summary>
    [CommandInfo("One Health", "Costed Option",
        "Menu option that previews its cost and is disabled when the player cannot pay for it.")]
    [AddComponentMenu("")]
    public class CostedOption : Fungus.Menu
    {
        [SerializeField] protected int money;
        [SerializeField] protected int timeWeeks;
        [SerializeField] protected int trust;
        [SerializeField] protected int resistance;
        [SerializeField] protected int capacity;

        [Tooltip("Show the cost line in the option label")]
        [SerializeField] protected bool showEffects = true;

        [Tooltip("Also preview Trust and Resistance Pressure. Only where the blueprint asks for a full preview.")]
        [SerializeField] protected bool previewOutcomes = false;

        [Tooltip("Capability that improves this option when unlocked, e.g. Community Coordination")]
        [SerializeField] protected string capability = "";
        [SerializeField] protected int capabilityTrust;
        [SerializeField] protected int capabilityCapacity;

        const int DetailSize = 28;

        public StatDelta Delta { get { return new StatDelta(money, timeWeeks, trust, resistance, capacity); } }

        /// <summary>Effects including the capability bonus, if the player has unlocked it.</summary>
        public StatDelta Effective
        {
            get { return GameSession.State.WithCapability(Delta, capability, new StatDelta(0, 0, capabilityTrust, 0, capabilityCapacity)); }
        }

        public bool Affordable { get { return GameSession.State.CanAfford(Effective); } }

        /// <summary>Hidden options (already used) never count as available.</summary>
        public bool Available
        {
            get
            {
                bool hidden = hideThisOption.Value || (hideIfVisited && targetBlock != null && targetBlock.GetExecutionCount() > 0);
                return !hidden && Affordable;
            }
        }

        public string Label
        {
            get
            {
                // Two lines on a 100px option button: the action, then one smaller detail line. Sizes are in
                // pixels because the menu buttons use legacy UI Text, which does not understand percentages.
                var d = Effective;
                var details = new System.Collections.Generic.List<string>();
                if (showEffects) details.Add(previewOutcomes ? d.ToDisplayString() : d.ToCostString());
                if (GameSession.State.CapabilityApplies(capability)) details.Add("<b>" + capability + ": " + BonusText() + "</b>");
                if (!Affordable) details.Add("<b>Not enough " + GameSession.State.Shortfall(d) + "</b>");
                var label = GetFlowchart().SubstituteVariables(text);
                if (details.Count > 0) label += "\n<size=" + DetailSize + ">" + string.Join("  ·  ", details.ToArray()) + "</size>";
                return label;
            }
        }

        string BonusText()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (capabilityTrust != 0) parts.Add("Trust " + StatDelta.Signed(capabilityTrust));
            if (capabilityCapacity > 0) parts.Add(capabilityCapacity + " less Capacity needed");
            return string.Join(", ", parts.ToArray());
        }

        public override string GetStandardText() { return Label; }

        public override void OnEnter()
        {
            if (setMenuDialog != null)
                MenuDialog.ActiveMenuDialog = setMenuDialog;

            bool hideOption = (hideIfVisited && targetBlock != null && targetBlock.GetExecutionCount() > 0) || hideThisOption.Value;
            var menuDialog = MenuDialog.GetMenuDialog();
            if (menuDialog != null)
            {
                menuDialog.SetActive(true);
                menuDialog.AddOption(Label, interactable.Value && Affordable, hideOption, targetBlock);
            }
            Continue();
        }

        public override string GetSummary()
        {
            return (targetBlock != null ? targetBlock.BlockName : "?") + " : " + text + " [" + Delta.ToDisplayString() + "]"
                + (string.IsNullOrEmpty(capability) ? "" : " +" + capability);
        }

        public override Color GetButtonColor() { return new Color32(242, 167, 59, 255); }
    }
}

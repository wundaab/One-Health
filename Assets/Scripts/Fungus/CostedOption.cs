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

        [Tooltip("Disable the option when Money, Time or Capacity cannot cover it. Off = the cost is shown for "
            + "information only and the option stays as interactable as its Interactable setting (Scenarios 1 and 2).")]
        [SerializeField] protected bool gateOnCost = true;

        // Label sizes in pixels on the 1920x1080 menu canvas: legacy UI Text does not understand percentages.
        const int DetailSize = 23;
        const int NoteSize = 22;

        public StatDelta Delta { get { return new StatDelta(money, timeWeeks, trust, resistance, capacity); } }

        /// <summary>Effects including the capability bonus, if the player has unlocked it.</summary>
        public StatDelta Effective
        {
            get { return GameSession.State.WithCapability(Delta, capability, new StatDelta(0, 0, capabilityTrust, 0, capabilityCapacity)); }
        }

        /// <summary>
        /// The resources the player can spend. Scenario 2 keeps its own clinic capacity counter
        /// (ClinicResponseCapacity) in the flowchart, so read Capacity from there when it exists.
        /// </summary>
        GameState Wallet
        {
            get
            {
                var s = GameSession.State;
                var fc = GetFlowchart();
                if (fc == null || !fc.HasVariable(FlowchartSync.ScenarioCapacityKey)) return s;
                var w = s.Clone();
                w.capacity = fc.GetIntegerVariable(FlowchartSync.ScenarioCapacityKey);
                return w;
            }
        }

        public bool Affordable { get { return Wallet.CanAfford(Effective); } }

        /// <summary>Raised for each visible option added to a menu (true = can be chosen). Drives the onboarding tips.</summary>
        public static event System.Action<bool> OptionShown;

        /// <summary>Hidden options (already used) never count as available.</summary>
        public bool Available
        {
            get
            {
                bool hidden = hideThisOption.Value || (hideIfVisited && targetBlock != null && targetBlock.GetExecutionCount() > 0);
                return !hidden && (!gateOnCost || Affordable);
            }
        }

        public string Label { get { return BuildLabel(true); } }

        /// <summary>
        /// Up to three lines: the action; the cost (or full preview) in teal; then, when it applies, the
        /// capability bonus or why the option is unavailable, in amber-brown. Same format in every scenario.
        /// </summary>
        string BuildLabel(bool enabled)
        {
            var d = Effective;
            var label = GetFlowchart().SubstituteVariables(text);
            if (showEffects)
                label += Line(d.ToOptionString(previewOutcomes), DetailSize, enabled ? UITheme.Teal : UITheme.DisabledInk, false);
            if (GameSession.State.CapabilityApplies(capability))
                label += Line(capability + ": " + BonusText(), NoteSize, enabled ? UITheme.Spend : UITheme.DisabledInk, true);
            if (!enabled && !Affordable)
                label += Line(ShortfallText(d), NoteSize, UITheme.Hex("8A4B12"), true);
            return label;
        }

        static string Line(string body, int size, Color color, bool bold)
        {
            if (bold) body = "<b>" + body + "</b>";
            return "\n<size=" + size + "><color=" + UITheme.ToHex(color) + ">" + body + "</color></size>";
        }

        /// <summary>"Needs 15 Capacity · you have 5" for each resource that cannot cover the cost.</summary>
        string ShortfallText(StatDelta d)
        {
            var w = Wallet;
            var parts = new System.Collections.Generic.List<string>();
            if (d.money < 0 && w.money + d.money < GameState.MoneyMin)
                parts.Add("Needs " + (-d.money) + " Money · you have " + w.money);
            if (d.timeWeeks < 0 && w.timeWeeks + d.timeWeeks < GameState.TimeMin)
                parts.Add("Needs " + Weeks(-d.timeWeeks) + " · you have " + Weeks(w.timeWeeks));
            if (d.capacity < 0 && w.capacity + d.capacity < GameState.CapacityMin)
                parts.Add("Needs " + (-d.capacity) + " Capacity · you have " + w.capacity);
            return parts.Count > 0 ? string.Join("   ", parts.ToArray()) : "Not available right now";
        }

        static string Weeks(int n) { return n + (n == 1 ? " week" : " weeks"); }

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
                bool enabled = interactable.Value && (!gateOnCost || Affordable);
                menuDialog.AddOption(BuildLabel(enabled), enabled, hideOption, targetBlock);
                if (!hideOption && OptionShown != null) OptionShown(enabled);
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

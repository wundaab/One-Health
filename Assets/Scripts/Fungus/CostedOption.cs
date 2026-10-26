using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// A menu option that shows its parameter effects before the player commits, and is disabled
    /// with an explanation when Money, Time or Capacity cannot cover its costs (Scenario 3/4
    /// blueprints: "Disable unaffordable options with an explanation"). Effects are applied by an
    /// Apply Decision in the target block, only after the player confirms.
    /// </summary>
    [CommandInfo("One Health", "Costed Option",
        "Menu option that previews its costs/effects and is disabled when the player cannot pay for it.")]
    [AddComponentMenu("")]
    public class CostedOption : Fungus.Menu
    {
        [SerializeField] protected int money;
        [SerializeField] protected int timeWeeks;
        [SerializeField] protected int trust;
        [SerializeField] protected int resistance;
        [SerializeField] protected int capacity;

        [Tooltip("Show the parameter effects in the option label")]
        [SerializeField] protected bool showEffects = true;

        public StatDelta Delta { get { return new StatDelta(money, timeWeeks, trust, resistance, capacity); } }

        public bool Affordable { get { return GameSession.State.CanAfford(Delta); } }

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
                var label = GetFlowchart().SubstituteVariables(text);
                if (showEffects && !Delta.IsEmpty) label += "\n<size=75%>" + Delta.ToDisplayString() + "</size>";
                if (!Affordable) label += "\n<size=75%>Not enough " + GameSession.State.Shortfall(Delta) + "</size>";
                return label;
            }
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
            return (targetBlock != null ? targetBlock.BlockName : "?") + " : " + text + " [" + Delta.ToDisplayString() + "]";
        }

        public override Color GetButtonColor() { return new Color32(242, 167, 59, 255); }
    }
}

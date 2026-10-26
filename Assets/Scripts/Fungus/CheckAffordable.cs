using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Sets a boolean variable to whether the current Money/Time/Capacity can pay a cost.</summary>
    [CommandInfo("One Health", "Check Affordable", "Set a boolean variable to whether a cost can currently be paid.")]
    [AddComponentMenu("")]
    public class CheckAffordable : Command
    {
        [SerializeField] protected BooleanVariable result;
        [SerializeField] protected int money;
        [SerializeField] protected int timeWeeks;
        [SerializeField] protected int capacity;

        public override void OnEnter()
        {
            if (result != null)
                result.Value = GameSession.State.CanAfford(new StatDelta(money, timeWeeks, 0, 0, capacity));
            Continue();
        }

        public override string GetSummary()
        {
            return (result != null ? result.Key : "?") + " = can pay " + new StatDelta(money, timeWeeks, 0, 0, capacity).ToDisplayString();
        }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

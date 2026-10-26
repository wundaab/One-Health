using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Place before the Costed Options of a mandatory activity. If none of the following options
    /// in this block can be paid for, runs the failure block instead (Scenario 3/4 blueprints:
    /// "If no viable option remains for a mandatory activity, show the failure panel").
    /// </summary>
    [CommandInfo("One Health", "Fail If None Affordable",
        "If none of the Costed Options that follow in this block can be paid for, run the failure block.")]
    [AddComponentMenu("")]
    public class FailIfNoneAffordable : Command
    {
        [SerializeField] protected Block failureBlock;

        public override void OnEnter()
        {
            var commands = ParentBlock.CommandList;
            bool any = false, found = false;
            for (int i = CommandIndex + 1; i < commands.Count; i++)
            {
                var option = commands[i] as CostedOption;
                if (option == null) continue;
                found = true;
                if (option.Available) { any = true; break; }
            }
            if (found && !any && failureBlock != null)
            {
                Debug.Log("[OHI] No affordable option in " + ParentBlock.BlockName + "; response blocked.");
                StopParentBlock();
                GetFlowchart().ExecuteBlock(failureBlock);
                return;
            }
            Continue();
        }

        public override string GetSummary() { return failureBlock != null ? "-> " + failureBlock.BlockName : "No failure block"; }

        public override Color GetButtonColor() { return new Color32(217, 90, 79, 255); }
    }
}

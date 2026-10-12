using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Shows an image card (concept visual) in the centre of the screen until the player
    /// presses Continue, e.g. the People / Animals / Environment visual.
    /// </summary>
    [CommandInfo("One Health", "Show Visual Card",
        "Show a full-screen visual card with a caption and wait for Continue.")]
    [AddComponentMenu("")]
    public class ShowVisualCard : Command
    {
        [SerializeField] protected string title = "";
        [SerializeField] protected Sprite image;
        [TextArea(2, 5)]
        [SerializeField] protected string caption = "";

        public override void OnEnter()
        {
            var card = Object.FindAnyObjectByType<VisualCard>(FindObjectsInactive.Include);
            if (card == null)
            {
                Debug.LogWarning("[OHI] No VisualCard in scene; skipping " + title);
                Continue();
                return;
            }
            card.Show(title, image, GetFlowchart().SubstituteVariables(caption), Continue);
        }

        public override string GetSummary() { return title; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

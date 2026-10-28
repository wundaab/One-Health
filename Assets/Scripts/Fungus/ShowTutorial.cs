using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Shows an onboarding sequence (see <see cref="Tutorial"/>) and waits until the player closes it.
    /// Does nothing if the player has already seen it or chose "Skip tips".
    /// </summary>
    [CommandInfo("One Health", "Show Tutorial", "Show an onboarding sequence (e.g. hud) the first time; waits until it is closed.")]
    [AddComponentMenu("")]
    public class ShowTutorial : Command
    {
        [Tooltip("Sequence id from Tutorial: hud, choices, locked or capacity")]
        [SerializeField] protected string tutorialId = Tutorial.Hud;

        public override void OnEnter()
        {
            var overlay = TutorialOverlay.Instance;
            if (overlay == null || Tutorial.Seen(tutorialId)) { Continue(); return; }
            overlay.Show(tutorialId, Continue);
        }

        public override string GetSummary() { return tutorialId; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Shows or hides the Money/Time/Trust/Resistance HUD.</summary>
    [CommandInfo("One Health", "Show HUD", "Show or hide the persistent resource HUD.")]
    [AddComponentMenu("")]
    public class ShowHUD : Command
    {
        [SerializeField] protected bool visible = true;

        public override void OnEnter()
        {
            var hud = Object.FindAnyObjectByType<StatHUD>(FindObjectsInactive.Include);
            if (hud != null) hud.SetVisible(visible);
            Continue();
        }

        public override string GetSummary() { return visible ? "Show" : "Hide"; }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

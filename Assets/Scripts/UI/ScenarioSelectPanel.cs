using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Scenario Select, offered on the main menu once the campaign is complete (M11): lists the four
    /// scenarios; completed ones can be replayed from the state the player entered them with. Replays
    /// never write the save, so the completed campaign stays as it is.
    /// </summary>
    public class ScenarioSelectPanel : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [Tooltip("One button per scenario, in campaign order (GameSession.Scenarios)")]
        [SerializeField] Button[] scenarioButtons;
        [SerializeField] TMP_Text[] scenarioLabels;
        [SerializeField] Button backButton;

        System.Action onClosed;
        bool wired;

        public bool IsOpen { get { return panel.IsOpen; } }

        void Awake() { Wire(); }

        // Buttons are wired on first use as well as in Awake, so the overlay works however it is activated.
        void Wire()
        {
            if (wired) return;
            wired = true;
            backButton.onClick.AddListener(Close);
            for (int i = 0; i < scenarioButtons.Length && i < GameSession.Scenarios.Length; i++)
            {
                var info = GameSession.Scenarios[i];
                scenarioButtons[i].onClick.AddListener(() => GameSession.ReplayScenario(info));
            }
        }

        public void Open(System.Action closed)
        {
            Wire();
            onClosed = closed;
            var state = GameSession.State;
            Button first = null;
            for (int i = 0; i < scenarioButtons.Length && i < GameSession.Scenarios.Length; i++)
            {
                var info = GameSession.Scenarios[i];
                bool done = state.HasCompleted(info.id);
                scenarioButtons[i].interactable = done;
                // The reason sits on the button itself, in text that stays readable when disabled (U25).
                scenarioLabels[i].text = info.title + "\n<size=60%>" + (done ? "Completed · Replay" : "Not completed yet") + "</size>";
                scenarioLabels[i].color = done ? Color.white : UITheme.DisabledInk;
                if (done && first == null) first = scenarioButtons[i];
            }
            panel.Open();
            (first != null ? first : backButton).Select();
        }

        public void Close()
        {
            panel.Close();
            if (onClosed != null) onClosed();
        }
    }
}

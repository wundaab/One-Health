using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Title screen: New Game, Continue (only with a save), Settings, Exit. After Game Complete, Continue
    /// becomes Review Campaign / Scenario Select (M11).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] string firstScenarioScene = "SC01";
        [SerializeField] AudioClip menuMusic;

        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] TMP_Text continueLabel;
        [SerializeField] Button settingsButton;
        [SerializeField] Button exitButton;

        [SerializeField] SettingsPanel settings;
        [SerializeField] ConfirmDialog confirm;
        [SerializeField] UIPanel menuPanel;
        [SerializeField] ScenarioSelectPanel scenarioSelect;

        void Start()
        {
            Time.timeScale = 1f;
            newGameButton.onClick.AddListener(OnNewGame);
            continueButton.onClick.AddListener(OnContinue);
            // Settings replaces the menu card while open, so two cards never overlap (U26).
            settingsButton.onClick.AddListener(() =>
            {
                menuPanel.Close();
                settings.Open(() => { menuPanel.Open(); settingsButton.Select(); });
            });
            exitButton.onClick.AddListener(GameSession.QuitGame);

            continueButton.interactable = CanContinue() || CanReview();
            if (CanReview() && continueLabel != null)
                continueLabel.text = "Review Campaign\n<size=55%>Scenario Select</size>";
            else if (!CanContinue() && continueLabel != null)
            {
                // Say why it is unavailable, in text that stays readable on the grey disabled button (U25):
                // navy is 5.5:1 there (DisabledInk was 2.3:1).
                continueLabel.text = "Continue\n<size=55%>No saved game yet</size>";
                continueLabel.color = UITheme.SurfaceDark;
            }
#if UNITY_WEBGL
            exitButton.gameObject.SetActive(false);
#endif
            if (menuMusic != null)
                FungusManager.Instance.MusicManager.PlayMusic(menuMusic, true, 1f, 0f);

            menuPanel.Open();
            (continueButton.interactable ? continueButton : newGameButton).Select();
        }

        static bool CanContinue()
        {
            return GameSession.HasSave && !string.IsNullOrEmpty(GameSession.State.checkpointScene);
        }

        /// <summary>The saved campaign is complete: Continue opens Scenario Select instead.</summary>
        bool CanReview()
        {
            return scenarioSelect != null && GameSession.HasSave && GameSession.CampaignCompleted;
        }

        void OnNewGame()
        {
            if (GameSession.HasSave)
            {
                confirm.Show("Start a new campaign?",
                    "Your completed campaign history will be reset.",
                    "New Game", StartNewGame);
            }
            else
            {
                StartNewGame();
            }
        }

        void StartNewGame()
        {
            GameSession.StartNewCampaign();
            GameSession.LoadScene(firstScenarioScene);
        }

        void OnContinue()
        {
            if (CanReview())
            {
                // Scenario Select replaces the menu card while open, as Settings does (U26).
                menuPanel.Close();
                scenarioSelect.Open(() => { menuPanel.Open(); continueButton.Select(); });
                return;
            }
            GameSession.LoadSavedCampaign();
            GameSession.LoadScene(GameSession.State.checkpointScene);
        }
    }
}

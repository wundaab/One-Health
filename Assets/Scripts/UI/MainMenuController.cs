using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>Title screen: New Game, Continue (only with a save), Settings, Exit.</summary>
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

            continueButton.interactable = CanContinue();
            if (!CanContinue() && continueLabel != null)
            {
                // Say why it is unavailable, in text that stays readable on the grey disabled button (U25).
                continueLabel.text = "Continue\n<size=55%>No saved game yet</size>";
                continueLabel.color = UITheme.DisabledInk;
            }
#if UNITY_WEBGL
            exitButton.gameObject.SetActive(false);
#endif
            if (menuMusic != null)
                FungusManager.Instance.MusicManager.PlayMusic(menuMusic, true, 1f, 0f);

            menuPanel.Open();
            (CanContinue() ? continueButton : newGameButton).Select();
        }

        static bool CanContinue()
        {
            return GameSession.HasSave && !string.IsNullOrEmpty(GameSession.State.checkpointScene);
        }

        void OnNewGame()
        {
            if (CanContinue())
            {
                confirm.Show("Start a new game?",
                    "Your saved progress will be replaced. Your decisions so far will not carry forward.",
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
            GameSession.LoadSavedCampaign();
            GameSession.LoadScene(GameSession.State.checkpointScene);
        }
    }
}

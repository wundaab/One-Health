using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Always-available navigation (S01 blueprint): Resume, Settings, Main Menu, Exit.
    /// NEXT is shown but locked until the scenario is completed. The panel and the Settings and Confirm dialogs it
    /// opens live on their own canvas (PauseCanvas, sorting order 999), so cards and the results screen can never
    /// cover them (M2); while paused, the rest of the game UI ignores clicks.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] Button pauseButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] GameObject nextLockIcon;
        [SerializeField] TMP_Text hintText;
        [SerializeField] SettingsPanel settings;
        [SerializeField] ConfirmDialog confirm;
        [Tooltip("CanvasGroup on the GameUI root: stops clicks reaching the HUD, cards and results while paused")]
        [SerializeField] CanvasGroup gameUiGroup;

        [Tooltip("Scene NEXT leads to once this scenario is completed")]
        [SerializeField] string nextScene = "";
        [SerializeField] string nextStageLabel = "";

        float previousTimeScale = 1f;

        public bool IsPaused { get { return panel.IsOpen; } }

        void Awake()
        {
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(() => settings.Open(() => settingsButton.Select()));
            mainMenuButton.onClick.AddListener(() => confirm.Show("Return to Main Menu?",
                "Progress in this scenario since it started will not be saved. You can Continue from the start of the scenario.",
                "Main Menu", GameSession.ReturnToMainMenu));
            exitButton.onClick.AddListener(() => confirm.Show("Exit the game?",
                "Progress in this scenario since it started will not be saved.",
                "Exit", GameSession.QuitGame));
            nextButton.onClick.AddListener(Next);
            if (restartButton != null)
                restartButton.onClick.AddListener(() => confirm.Show("Restart this scenario?",
                    "You will return to the start of this scenario with the values you entered it with. Progress and badges from this attempt will be lost.",
                    "Restart", () =>
                    {
                        GameSession.RestoreScenarioStart();
                        GameSession.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    }));
#if UNITY_WEBGL
            exitButton.gameObject.SetActive(false);
#endif
        }

        // Esc is a LIFO stack (M3): a tip, Settings or Confirm above the pause menu takes Esc first and closes
        // alone; the pause menu resumes only when it is the topmost modal, and opens only when nothing is open.
        void Update()
        {
            if (Tutorial.IsOpen) return;   // Esc closes the tip instead
            if (IsPaused)
            {
                if (ModalStack.TryConsumeEscape(this)) Resume();
            }
            else if (ModalStack.Top == null && ModalStack.EscapePressed)
            {
                ModalStack.ConsumeEscape();
                Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused) return;
            ResolveNext();
            nextButton.gameObject.SetActive(!string.IsNullOrEmpty(nextScene));
            bool unlocked = !string.IsNullOrEmpty(nextScene) && GameSession.State.HasCompleted(GameSession.CurrentScenarioId);
            nextButton.interactable = unlocked;
            nextLockIcon.SetActive(!unlocked);
            string label = string.IsNullOrEmpty(nextStageLabel) ? "Next" : nextStageLabel;
            // The reason sits on the locked button itself, so the hint line only explains Esc (U19).
            nextLabel.text = unlocked ? label : label + "\n<size=65%>Complete this scenario first</size>";
            if (hintText != null)
                hintText.text = "Press Esc to resume";

            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            SetDialogInput(false);
            if (gameUiGroup != null) gameUiGroup.blocksRaycasts = false;
            panel.Open();
            ModalStack.Push(this);
            resumeButton.Select();
        }

        /// <summary>NEXT comes from the scenario's Complete Scenario command unless set explicitly.</summary>
        void ResolveNext()
        {
            if (!string.IsNullOrEmpty(nextScene)) return;
            var complete = FindAnyObjectByType<CompleteScenario>();
            if (complete == null) return;
            nextScene = complete.NextScene;
            nextStageLabel = complete.NextLabel;
        }

        void Next()
        {
            // During a replay the campaign continues from the saved (original) outcome, so the replay's
            // progress never reaches the next scenario's checkpoint save (H3; same rule as the results Next).
            if (GameSession.IsReplay)
                GameSession.LoadSavedCampaign();
            GameSession.LoadScene(nextScene);
        }

        public void Resume()
        {
            if (!IsPaused) return;
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            SetDialogInput(true);
            if (gameUiGroup != null) gameUiGroup.blocksRaycasts = true;
            ModalStack.Remove(this);
            panel.Close();
        }

        // Fungus "click anywhere" input would otherwise advance dialogue behind the pause menu. The say and
        // menu dialogs are hidden too, so their text never shows through behind the pause panel.
        static void SetDialogInput(bool enabled)
        {
            foreach (var input in FindObjectsByType<Fungus.DialogInput>())
                input.enabled = enabled;
            foreach (var say in FindObjectsByType<Fungus.SayDialog>(FindObjectsInactive.Include))
                SetCanvas(say, enabled);
            foreach (var menu in FindObjectsByType<Fungus.MenuDialog>(FindObjectsInactive.Include))
                SetCanvas(menu, enabled);
        }

        static void SetCanvas(Component dialog, bool visible)
        {
            var canvas = dialog.GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = visible;
        }

        void OnDestroy()
        {
            if (IsPaused) Time.timeScale = 1f;
            ModalStack.Remove(this);
        }
    }
}

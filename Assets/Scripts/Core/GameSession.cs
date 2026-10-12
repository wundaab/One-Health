using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHealth
{
    /// <summary>
    /// Owns the current GameState for the running game: applying changes, scenario
    /// snapshots for Retry/Replay, and save/load. Static so it survives scene loads.
    /// </summary>
    public static class GameSession
    {
        const string SaveKey = "OHI_Save_v1";

        public const string MainMenuScene = "MainMenu";

        static GameState current;
        static GameState scenarioStart;
        static string scenarioId = "";

        /// <summary>Raised after any parameter change. Carries the change actually applied (after clamping).</summary>
        public static event Action<StatDelta> StatsChanged;

        /// <summary>Raised when the state is replaced wholesale (new game, load, retry).</summary>
        public static event Action StateReset;

        public static GameState State
        {
            get
            {
                if (current == null)
                    current = HasSave ? LoadFromDisk() : GameState.NewCampaign();
                return current;
            }
        }

        public static string CurrentScenarioId { get { return scenarioId; } }

        /// <summary>Whether the current scenario shows Time on the HUD (set by Begin Scenario).</summary>
        public static bool ShowTimeStat { get; set; }

        public static bool HasSave { get { return PlayerPrefs.HasKey(SaveKey); } }

        // ---------------------------------------------------------------- campaign

        public static void StartNewCampaign()
        {
            current = GameState.NewCampaign();
            scenarioStart = null;
            IsReplay = false;
            PlayerPrefs.DeleteKey(SaveKey);
            if (StateReset != null) StateReset();
        }

        public static void LoadSavedCampaign()
        {
            current = LoadFromDisk();
            if (StateReset != null) StateReset();
        }

        /// <summary>
        /// Marks the start of a scenario: records the checkpoint and snapshots the inherited
        /// state so Retry/Replay restore exactly what the player entered with.
        /// </summary>
        public static void BeginScenario(string id, int timeBudgetWeeks)
        {
            var s = State;
            if (timeBudgetWeeks >= 0)
                s.timeWeeks = Mathf.Clamp(timeBudgetWeeks, GameState.TimeMin, GameState.TimeMax);
            scenarioId = id;
            scenarioStart = s.Clone();

            // Replaying a completed scenario is experimentation: it never writes the save.
            IsReplay = s.HasCompleted(id);
            if (!IsReplay)
            {
                s.checkpointScene = SceneManager.GetActiveScene().name;
                scenarioStart.checkpointScene = s.checkpointScene;
                Save();
            }
            Debug.Log(string.Format("[OHI] Begin {0}{5}: Money {1}, Time {2}w, Trust {3}, Resistance {4}",
                id, s.money, s.timeWeeks, s.trust, s.resistance, IsReplay ? " (replay)" : ""));
            if (StateReset != null) StateReset();
        }

        /// <summary>True while replaying a scenario that was already completed in this campaign.</summary>
        public static bool IsReplay { get; private set; }

        /// <summary>State the player entered the current scenario with (null outside a scenario).</summary>
        public static GameState ScenarioStartState { get { return scenarioStart; } }

        /// <summary>
        /// Retry/Replay: restores the parameters the player had when the current scenario began.
        /// Completions, knowledge and unlocks already earned are kept, so replaying can never
        /// remove or duplicate campaign progress (Progression sheet: Replay integrity).
        /// </summary>
        public static void RestoreScenarioStart()
        {
            if (scenarioStart != null)
            {
                var earned = State;
                current = scenarioStart.Clone();
                foreach (var id in earned.completedScenarios) current.AddUnique(current.completedScenarios, id);
                foreach (var id in earned.knowledge) current.AddUnique(current.knowledge, id);
                foreach (var id in earned.unlocks) current.AddUnique(current.unlocks, id);
            }
            if (StateReset != null) StateReset();
        }

        // ---------------------------------------------------------------- state changes

        /// <summary>
        /// Applies a confirmed action. Values are clamped to the specified ranges and the
        /// change actually applied is returned (and logged, per the S01 debug requirement).
        /// </summary>
        public static StatDelta Apply(StatDelta requested, string reason)
        {
            var s = State;
            int m0 = s.money, t0 = s.timeWeeks, tr0 = s.trust, r0 = s.resistance;

            s.money += requested.money;
            s.timeWeeks += requested.timeWeeks;
            s.trust += requested.trust;
            s.resistance += requested.resistance;
            s.Clamp();

            var applied = new StatDelta(s.money - m0, s.timeWeeks - t0, s.trust - tr0, s.resistance - r0);
            Debug.Log(string.Format("[OHI] {0}: Money {1}->{2}, Time {3}->{4}w, Trust {5}->{6}, Resistance {7}->{8}",
                reason, m0, s.money, t0, s.timeWeeks, tr0, s.trust, r0, s.resistance));

            if (StatsChanged != null) StatsChanged(applied);
            return applied;
        }

        public static void RecordDecision(string decisionId)
        {
            if (!string.IsNullOrEmpty(decisionId))
                State.decisions.Add(scenarioId + ":" + decisionId);
        }

        public static void AddKnowledge(string id) { State.AddUnique(State.knowledge, id); }

        public static void AddUnlock(string id) { State.AddUnique(State.unlocks, id); }

        /// <summary>
        /// Records completion and saves the final state as the checkpoint for the next scenario,
        /// so Continue resumes there with the inherited state.
        /// </summary>
        public static void CompleteScenario(string id, string nextScene)
        {
            if (IsReplay) return;
            State.AddUnique(State.completedScenarios, id);
            // Continue resumes at the next scenario; after the last one there is nothing to resume.
            State.checkpointScene = string.IsNullOrEmpty(nextScene) || nextScene == MainMenuScene ? "" : nextScene;
            Save();
        }

        // ---------------------------------------------------------------- persistence

        /// <summary>
        /// Saves are checkpoints: written at scenario start and completion only. Mid-scenario
        /// changes are not saved, so Continue always resumes from a clean scenario start.
        /// </summary>
        static void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(State));
            PlayerPrefs.Save();
        }

        static GameState LoadFromDisk()
        {
            try
            {
                var json = PlayerPrefs.GetString(SaveKey, "");
                if (!string.IsNullOrEmpty(json))
                    return JsonUtility.FromJson<GameState>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[OHI] Could not read save, starting fresh: " + e.Message);
            }
            return GameState.NewCampaign();
        }

        // ---------------------------------------------------------------- navigation

        public static void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public static void ReturnToMainMenu()
        {
            LoadScene(MainMenuScene);
        }

        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

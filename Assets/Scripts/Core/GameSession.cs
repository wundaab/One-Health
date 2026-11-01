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
        const string EntryKeyPrefix = "OHI_Entry_v1_";

        // L6: minimum Money and Capacity at the start of every scenario.
        public const int StartMoneyFloor = 15;
        public const int StartCapacityFloor = 5;

        /// <summary>The campaign's scenarios in order: ID, scene and title (Scenario Select lists these).</summary>
        public static readonly ScenarioInfo[] Scenarios =
        {
            new ScenarioInfo("S01", "SC01", "Scenario 1 — The First Response"),
            new ScenarioInfo("S02", "SC02", "Scenario 2 — The Fever That Won’t Wait"),
            new ScenarioInfo("S03", "SC03", "Scenario 3 — The Warning Signs"),
            new ScenarioInfo("S04", "SC04", "Scenario 4 — Kivu’s Turning Point"),
        };

        /// <summary>Story flag set when the final scenario is completed (main menu offers Scenario Select).</summary>
        public const string CampaignCompletedFlag = "campaign_completed";

        /// <summary>Story flag holding the Money actually added by a scenario's entry grant, e.g. money_grant_S04.</summary>
        public static string MoneyGrantFlag(string scenarioId) { return "money_grant_" + scenarioId; }

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

        /// <summary>Whether the current scenario shows Capacity on the HUD (set by Begin Scenario).</summary>
        public static bool ShowCapacityStat { get; set; }

        /// <summary>Capacity scale of the current scenario (M7, set by Begin Scenario): the HUD bar's maximum...</summary>
        public static int CapacityScaleMax { get; set; } = 100;

        /// <summary>...and the value at or below which the Capacity chip shows Low.</summary>
        public static int CapacityLowThreshold { get; set; } = 20;

        public static bool HasSave { get { return PlayerPrefs.HasKey(SaveKey); } }

        /// <summary>True once the saved campaign has completed Scenario 4.</summary>
        public static bool CampaignCompleted { get { return State.GetFlag(CampaignCompletedFlag) == "true"; } }

        // ---------------------------------------------------------------- campaign

        public static void StartNewCampaign()
        {
            current = GameState.NewCampaign();
            scenarioStart = null;
            IsReplay = false;
            PlayerPrefs.DeleteKey(SaveKey);
            foreach (var info in Scenarios) PlayerPrefs.DeleteKey(EntryKeyPrefix + info.id);
            if (StateReset != null) StateReset();
        }

        /// <summary>
        /// Scenario Select: replays a completed scenario from the state the player entered it with. Completions,
        /// knowledge and unlocks earned since are kept (as for Retry), and replays never write the save.
        /// </summary>
        public static void ReplayScenario(ScenarioInfo info)
        {
            LoadSavedCampaign();
            var json = PlayerPrefs.GetString(EntryKeyPrefix + info.id, "");
            if (!string.IsNullOrEmpty(json))
            {
                var saved = current;
                current = JsonUtility.FromJson<GameState>(json);
                foreach (var id in saved.completedScenarios) current.AddUnique(current.completedScenarios, id);
                foreach (var id in saved.knowledge) current.AddUnique(current.knowledge, id);
                foreach (var id in saved.unlocks) current.AddUnique(current.unlocks, id);
                if (StateReset != null) StateReset();
            }
            LoadScene(info.scene);
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
        public static void BeginScenario(string id, int timeBudgetWeeks, int timeGrantWeeks = 0, int capacityStart = -1,
            int capacityRescaleBase = -1, int capacityRescaleDivisor = 10, int moneyGrant = 0, int capacityRescaleMax = -1)
        {
            var s = State;
            if (timeBudgetWeeks >= 0)
                s.timeWeeks = Mathf.Clamp(timeBudgetWeeks, GameState.TimeMin, GameState.TimeMax);
            if (capacityStart >= 0)
                s.capacity = Mathf.Clamp(capacityStart, GameState.CapacityMin, GameState.CapacityMax);
            // Relative entry adjustments (see BeginScenario) are applied once per scenario: Retry/Continue
            // reload the scene with the snapshot, which already carries the flag, so they never stack.
            if (s.GetFlag("entry_applied_" + id) != "1")
            {
                if (timeGrantWeeks > 0)
                    s.timeWeeks = Mathf.Clamp(s.timeWeeks + timeGrantWeeks, GameState.TimeMin, GameState.TimeMax);
                if (capacityRescaleBase >= 0)
                {
                    // D2: base + floor(carried / divisor), never below the base nor above the maximum.
                    int max = capacityRescaleMax >= 0 ? capacityRescaleMax : GameState.CapacityMax;
                    s.capacity = Mathf.Clamp(capacityRescaleBase + s.capacity / Mathf.Max(1, capacityRescaleDivisor),
                        Mathf.Max(GameState.CapacityMin, capacityRescaleBase), Mathf.Min(GameState.CapacityMax, max));
                }
                if (moneyGrant != 0)
                {
                    // The story shows the amount actually added, which the Money cap can reduce (D3).
                    int before = s.money;
                    s.money = Mathf.Clamp(s.money + moneyGrant, GameState.MoneyMin, GameState.MoneyMax);
                    s.SetFlag(MoneyGrantFlag(id), (s.money - before).ToString());
                }
                s.SetFlag("entry_applied_" + id, "1");
            }
            // L6: every scenario starts with a minimum of Money and Capacity, so an earlier scenario can
            // never leave the player unable to act. Idempotent, so Retry and Continue give the same values.
            s.money = Mathf.Max(s.money, Mathf.Min(StartMoneyFloor, GameState.MoneyMax));
            s.capacity = Mathf.Max(s.capacity, Mathf.Min(StartCapacityFloor, GameState.CapacityMax));
            scenarioId = id;
            scenarioStart = s.Clone();

            // Replaying a completed scenario is experimentation: it never writes the save.
            IsReplay = s.HasCompleted(id);
            if (!IsReplay)
            {
                s.checkpointScene = SceneManager.GetActiveScene().name;
                scenarioStart.checkpointScene = s.checkpointScene;
                Save();
                // Kept so Scenario Select can replay this scenario from the state the player entered it with.
                PlayerPrefs.SetString(EntryKeyPrefix + id, JsonUtility.ToJson(scenarioStart));
                PlayerPrefs.Save();
            }
            Debug.Log(string.Format("[OHI] Begin {0}{6}: Money {1}, Time {2}w, Trust {3}, Resistance {4}, Capacity {5}",
                id, s.money, s.timeWeeks, s.trust, s.resistance, s.capacity, IsReplay ? " (replay)" : ""));
            if (StateReset != null) StateReset();
        }

        /// <summary>True while replaying a scenario that was already completed in this campaign.</summary>
        public static bool IsReplay { get; private set; }

        /// <summary>State the player entered the current scenario with (null outside a scenario).</summary>
        public static GameState ScenarioStartState { get { return scenarioStart; } }

        /// <summary>
        /// Retry/Replay: restores the parameters, flags and badges the player had when the current
        /// scenario began. Completions, knowledge and unlocks already earned are kept, so replaying
        /// can never remove or duplicate campaign progress (Progression sheet: Replay integrity).
        /// Badges earned during the abandoned attempt are cleared (Scenario 3/4 blueprints: Retry).
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
            int m0 = s.money, t0 = s.timeWeeks, tr0 = s.trust, r0 = s.resistance, c0 = s.capacity;

            s.money += requested.money;
            s.timeWeeks += requested.timeWeeks;
            s.trust += requested.trust;
            s.resistance += requested.resistance;
            s.capacity += requested.capacity;
            s.Clamp();

            var applied = new StatDelta(s.money - m0, s.timeWeeks - t0, s.trust - tr0, s.resistance - r0, s.capacity - c0);
            Debug.Log(string.Format("[OHI] {0}: Money {1}->{2}, Time {3}->{4}w, Trust {5}->{6}, Resistance {7}->{8}, Capacity {9}->{10}",
                reason, m0, s.money, t0, s.timeWeeks, tr0, s.trust, r0, s.resistance, c0, s.capacity));

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

        public static void AwardBadge(string id) { State.AddUnique(State.badges, id); }

        /// <summary>Sets an absolute Capacity value (e.g. syncing a scenario's own capacity counter).</summary>
        public static void SetCapacity(int value)
        {
            int before = State.capacity;
            State.capacity = Mathf.Clamp(value, GameState.CapacityMin, GameState.CapacityMax);
            if (StatsChanged != null && before != State.capacity)
                StatsChanged(new StatDelta(0, 0, 0, 0, State.capacity - before));
        }

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

    /// <summary>One scenario of the campaign: its ID (S01), scene (SC01) and player-facing title.</summary>
    [Serializable]
    public struct ScenarioInfo
    {
        public string id;
        public string scene;
        public string title;

        public ScenarioInfo(string id, string scene, string title)
        {
            this.id = id;
            this.scene = scene;
            this.title = title;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fungus;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Automated playthrough for QA: in Play mode it advances dialogue, continues visual cards and
    /// picks menu options from a plan (0-based option index per menu, in order). Stops at the
    /// results screen and records a transcript. Start with PlaythroughDriver.Start("0,0,1").
    /// </summary>
    [InitializeOnLoad]
    public static class PlaythroughDriver
    {
        const string PlanKey = "OHI_TestPlan";
        const string LogKey = "OHI_TestLog";
        const string StepKey = "OHI_TestStep";
        const string StopAtKey = "OHI_TestStopAt";
        static double nextAction;

        static PlaythroughDriver()
        {
            EditorApplication.update += Tick;
            // Setting Application.runInBackground during Play writes Player Settings; put the project value back.
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode) SessionState.SetBool("OHI_RunInBackground", PlayerSettings.runInBackground);
                if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool("OHI_RunInBackground", PlayerSettings.runInBackground) != PlayerSettings.runInBackground)
                    PlayerSettings.runInBackground = SessionState.GetBool("OHI_RunInBackground", false);
            };
        }

        /// <param name="stopAt">Optional: stop (leaving that screen up, e.g. for a screenshot) once a logged line contains this text.</param>
        public static void Start(string plan, string stopAt = "")
        {
            SessionState.SetString(StopAtKey, stopAt ?? "");
            SessionState.SetString(PlanKey, plan);
            SessionState.SetString(PlanKey + "_Copy", plan);
            SessionState.SetInt(StepKey, 0);
            SessionState.SetString(LogKey, "");
        }

        /// <summary>Continues a run halted by stopAt, from the same step, until the next stopAt (if any).</summary>
        public static void Resume(string stopAt = "")
        {
            SessionState.SetString(StopAtKey, stopAt ?? "");
            SessionState.SetString(PlanKey, SessionState.GetString(PlanKey + "_Copy", ""));
        }

        public static void Stop() { SessionState.EraseString(PlanKey); }

        public static bool Running { get { return !string.IsNullOrEmpty(SessionState.GetString(PlanKey, "")); } }

        public static string Transcript { get { return SessionState.GetString(LogKey, ""); } }

        static void Log(string line)
        {
            SessionState.SetString(LogKey, Transcript + line + "\n");
            var stopAt = SessionState.GetString(StopAtKey, "");
            if (stopAt.Length > 0 && line.Contains(stopAt)) Stop();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            if (!Running)
            {
                // Halted at a stopAt: keep the player loop running so fades finish for screenshots.
                if (SessionState.GetString(StopAtKey, "").Length > 0) EditorApplication.QueuePlayerLoopUpdate();
                return;
            }
            Application.runInBackground = true;   // keep the player loop ticking while the editor is unfocused
            if (Time.timeScale == 1f) Time.timeScale = 40f;   // fast-forward typing and fades
            EditorApplication.QueuePlayerLoopUpdate();
            if (EditorApplication.timeSinceStartup < nextAction) return;
            nextAction = EditorApplication.timeSinceStartup + 0.12;

            // GameStarted waits for WaitForEndOfFrame, which never fires while an unfocused editor
            // isn't repainting the Game view. Kick those blocks off ourselves.
            if (Time.frameCount > 30)
            {
                foreach (var h in Object.FindObjectsByType<GameStarted>())
                {
                    if (h.ParentBlock != null && h.ParentBlock.GetExecutionCount() == 0 && !h.ParentBlock.IsExecuting())
                    {
                        Log("KICK: " + h.ParentBlock.BlockName);
                        h.ExecuteBlock();
                    }
                }
            }

            // Onboarding tips sit above everything: read each one and press Next.
            var tutorial = TutorialOverlay.Instance;
            if (tutorial != null && tutorial.IsOpen)
            {
                Log("TUTORIAL: " + tutorial.CurrentTitle);
                if (!Running) return;
                ClickNamed(tutorial.transform, "NextButton");
                nextAction += 0.3;
                return;
            }

            var results = Object.FindAnyObjectByType<ResultsPanel>();
            if (results != null && results.gameObject.activeInHierarchy)
            {
                var s = GameSession.State;
                Log(string.Format("RESULTS: Money {0}, Time {1}, Trust {2}, Resistance {3}, Capacity {8} | knowledge [{4}] | unlocks [{5}] | badges [{9}] | decisions [{6}] | completed [{7}]",
                    s.money, s.timeWeeks, s.trust, s.resistance, string.Join(", ", s.knowledge.ToArray()),
                    string.Join(", ", s.unlocks.ToArray()), string.Join(", ", s.decisions.ToArray()), string.Join(", ", s.completedScenarios.ToArray()),
                    s.capacity, string.Join(", ", s.badges.ToArray())));
                Stop();
                return;
            }

            var caseBoard = GameObject.Find("CaseBoardPanel");
            if (caseBoard != null && caseBoard.activeInHierarchy)
            {
                foreach (var b in caseBoard.GetComponentsInChildren<Button>().Where(b => b.name.EndsWith("Card") || b.name == "ContinueButton"))
                {
                    // onClick.Invoke bypasses the event system, so first check a real click would land on the button.
                    if (!ClickReaches(b))
                    {
                        Log("ERROR: " + b.name + " on the case board cannot be clicked (something blocks or disables pointer input).");
                        Stop();
                        return;
                    }
                }
                foreach (var b in caseBoard.GetComponentsInChildren<Button>().Where(b => b.name.EndsWith("Card")))
                {
                    b.onClick.Invoke();
                    var revealed = b.GetComponentsInChildren<TMPro.TMP_Text>(true).FirstOrDefault(t => t.name == "Evidence");
                    Log("EVIDENCE: " + b.name + (revealed != null && revealed.gameObject.activeInHierarchy ? " -> " + revealed.text : " (not revealed)"));
                }
                ClickNamed(caseBoard.transform, "ContinueButton");
                nextAction += 0.4;
                return;
            }

            var card = Object.FindAnyObjectByType<VisualCard>();
            if (card != null && card.gameObject.activeInHierarchy)
            {
                var title = card.FullTitle;
                Log("CARD: " + title);
                if (!Running) return;
                ClickNamed(card.transform, "ContinueButton");
                nextAction += 0.4;
                return;
            }

            var menu = MenuDialog.ActiveMenuDialog;
            if (menu != null && menu.isActiveAndEnabled && menu.DisplayedOptionsCount > 0)
            {
                // A first-time tip is about to open over this menu: give it a moment (at most ~3 s) before choosing.
                if (tutorial != null && tutorial.TipPending && SessionState.GetInt("OHI_TipWait", 0) < 12)
                {
                    SessionState.SetInt("OHI_TipWait", SessionState.GetInt("OHI_TipWait", 0) + 1);
                    return;
                }
                SessionState.SetInt("OHI_TipWait", 0);
                var buttons = menu.CachedButtons.Where(b => b.gameObject.activeSelf).ToList();
                var shown = string.Join(" | ", buttons.Select(b =>
                {
                    var t = b.GetComponentInChildren<TMPro.TMP_Text>();
                    var u = b.GetComponentInChildren<Text>();
                    return (t != null ? t.text : u != null ? u.text : "?").Split('\n')[0];
                }).ToArray());
                if (SessionState.GetString("OHI_LastMenu", "") != shown)
                {
                    SessionState.SetString("OHI_LastMenu", shown);
                    Log("OPTIONS: " + shown);
                    if (!Running) return;
                }
                var plan = SessionState.GetString(PlanKey, "").Split(',');
                int step = SessionState.GetInt(StepKey, 0);
                int pick = step < plan.Length ? int.Parse(plan[step].Trim()) : 0;
                var label = buttons[pick].GetComponentInChildren<TMPro.TMP_Text>();
                var legacy = buttons[pick].GetComponentInChildren<Text>();
                string labelText = label != null ? label.text : legacy != null ? legacy.text : "?";
                Log("CHOOSE [" + pick + "]: " + labelText);
                if (!buttons[pick].interactable)
                {
                    Log("ERROR: option " + pick + " is disabled (cannot be afforded); a player could not choose it.");
                    Stop();
                    return;
                }
                SessionState.SetInt(StepKey, step + 1);

                // Fungus starts the chosen block after WaitForEndOfFrame, which never fires in an
                // unfocused editor. Do what the click does, minus that wait: find the Menu command
                // with this label and execute its target block.
                // Labels repeat across menus ("Commit", "NOT YET KNOWN"), so read the option's own
                // target block from the click delegate Fungus attached to this button.
                var target = TargetOf(buttons[pick]);
                if (target == null)
                {
                    Log("ERROR: no Menu command found for '" + labelText + "'");
                    Stop();
                    return;
                }
                menu.Clear();
                menu.HideSayDialog();
                menu.gameObject.SetActive(false);
                target.GetFlowchart().ExecuteBlock(target);
                nextAction += 0.4;
                return;
            }

            // SayDialog.ActiveSayDialog can be null while a dialog is on screen, so check every visible one.
            var say = Object.FindObjectsByType<SayDialog>()
                .FirstOrDefault(d => d.isActiveAndEnabled && d.GetComponent<Writer>() != null && d.GetComponent<Writer>().IsWaitingForInput);
            if (say != null)
            {
                var writer = say.GetComponent<Writer>();
                var active = Object.FindObjectsByType<Flowchart>().SelectMany(f => f.GetExecutingBlocks())
                    .Select(b => b.ActiveCommand).OfType<Say>().FirstOrDefault();
                var text = active == null ? say.StoryText
                    : (active._Character != null ? active._Character.NameText + ": " : "") + active.GetFlowchart().SubstituteVariables(active.GetStandardText());
                if (writer != null && writer.IsWaitingForInput)
                {
                    if (SessionState.GetString("OHI_LastSay", "") != text)
                    {
                        Log("SAY: " + text.Replace("\n", " "));
                        SessionState.SetString("OHI_LastSay", text);
                        if (!Running) return;
                    }
                    writer.OnNextLineEvent();
                }
            }
        }

        /// <summary>The Block captured by the runtime listener MenuDialog.AddOption added to this button.</summary>
        static Block TargetOf(Button button)
        {
            const System.Reflection.BindingFlags F = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            var calls = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", F).GetValue(button.onClick);
            var runtime = calls.GetType().GetField("m_RuntimeCalls", F).GetValue(calls) as System.Collections.IEnumerable;
            foreach (var call in runtime)
            {
                var del = call.GetType().GetField("Delegate", F).GetValue(call) as System.Delegate;
                if (del == null || del.Target == null) continue;
                foreach (var field in del.Target.GetType().GetFields(F))
                {
                    var block = field.GetValue(del.Target) as Block;
                    if (block != null) return block;
                }
            }
            return null;
        }

        /// <summary>True if a pointer click at the button's centre would hit the button (or one of its children).</summary>
        static bool ClickReaches(Button button)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null || !button.IsInteractable()) return false;
            var corners = new Vector3[4];
            ((RectTransform)button.transform).GetWorldCorners(corners);
            var data = new UnityEngine.EventSystems.PointerEventData(es) { position = (corners[0] + corners[2]) / 2f };
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            es.RaycastAll(data, hits);
            return hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(button.transform);
        }

        static void ClickNamed(Transform root, string name)
        {
            var b = root.GetComponentsInChildren<Button>(true).FirstOrDefault(x => x.name == name);
            if (b != null) b.onClick.Invoke();
        }
    }
}

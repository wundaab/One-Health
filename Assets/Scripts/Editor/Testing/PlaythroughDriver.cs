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
        static double nextAction;

        static PlaythroughDriver()
        {
            EditorApplication.update += Tick;
        }

        public static void Start(string plan)
        {
            SessionState.SetString(PlanKey, plan);
            SessionState.SetInt(StepKey, 0);
            SessionState.SetString(LogKey, "");
        }

        public static void Stop() { SessionState.EraseString(PlanKey); }

        public static bool Running { get { return !string.IsNullOrEmpty(SessionState.GetString(PlanKey, "")); } }

        public static string Transcript { get { return SessionState.GetString(LogKey, ""); } }

        static void Log(string line)
        {
            SessionState.SetString(LogKey, Transcript + line + "\n");
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || !Running) return;
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

            var results = Object.FindAnyObjectByType<ResultsPanel>();
            if (results != null && results.gameObject.activeInHierarchy)
            {
                var s = GameSession.State;
                Log(string.Format("RESULTS: Money {0}, Time {1}, Trust {2}, Resistance {3} | knowledge [{4}] | unlocks [{5}] | decisions [{6}] | completed [{7}]",
                    s.money, s.timeWeeks, s.trust, s.resistance, string.Join(", ", s.knowledge.ToArray()),
                    string.Join(", ", s.unlocks.ToArray()), string.Join(", ", s.decisions.ToArray()), string.Join(", ", s.completedScenarios.ToArray())));
                Stop();
                return;
            }

            var caseBoard = GameObject.Find("CaseBoardPanel");
            if (caseBoard != null && caseBoard.activeInHierarchy)
            {
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
                var title = card.GetComponentsInChildren<TMPro.TMP_Text>(true).First().text;
                Log("CARD: " + title);
                ClickNamed(card.transform, "ContinueButton");
                nextAction += 0.4;
                return;
            }

            var menu = MenuDialog.ActiveMenuDialog;
            if (menu != null && menu.isActiveAndEnabled && menu.DisplayedOptionsCount > 0)
            {
                var buttons = menu.CachedButtons.Where(b => b.gameObject.activeSelf).ToList();
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

        static void ClickNamed(Transform root, string name)
        {
            var b = root.GetComponentsInChildren<Button>(true).FirstOrDefault(x => x.name == name);
            if (b != null) b.onClick.Invoke();
        }
    }
}

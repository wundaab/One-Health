using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Results screen (S01 blueprint §19): final state, what changed, learning, unlocks,
    /// and Replay / Next. Also supports the "Scenario Not Completed" state with Retry.
    /// </summary>
    public class ResultsPanel : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] TMP_Text header;
        [SerializeField] TMP_Text scenarioTitle;
        [SerializeField] TMP_Text summary;
        [SerializeField] TMP_Text moneyRow;
        [SerializeField] TMP_Text timeRow;
        [SerializeField] TMP_Text trustRow;
        [SerializeField] TMP_Text resistanceRow;
        [SerializeField] TMP_Text learnedText;
        [SerializeField] TMP_Text unlockedText;

        [SerializeField] Button replayButton;
        [SerializeField] TMP_Text replayLabel;
        [SerializeField] Button menuButton;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;

        string nextScene;

        void Awake()
        {
            replayButton.onClick.AddListener(Replay);
            menuButton.onClick.AddListener(GameSession.ReturnToMainMenu);
            nextButton.onClick.AddListener(Next);
        }

        public void ShowCompleted(string title, string summaryText, string next, string nextStageLabel)
        {
            nextScene = next;
            header.text = "Scenario Complete";
            replayLabel.text = "Replay Scenario";
            nextButton.gameObject.SetActive(!string.IsNullOrEmpty(next));
            nextLabel.text = string.IsNullOrEmpty(nextStageLabel) ? "Next" : nextStageLabel;
            if (GameSession.IsReplay)
                summaryText += "\n<i>This was a replay to try a different strategy. Your campaign continues from your original outcome.</i>";
            Fill(title, summaryText);
            panel.Open();
            nextButton.Select();
        }

        public void ShowNotCompleted(string title, string reason)
        {
            nextScene = null;
            header.text = "Scenario Not Completed";
            replayLabel.text = "Retry Scenario";
            nextButton.gameObject.SetActive(false);
            Fill(title, reason);
            panel.Open();
            replayButton.Select();
        }

        void Fill(string title, string summaryText)
        {
            var end = GameSession.State;
            var start = GameSession.ScenarioStartState ?? end;
            scenarioTitle.text = title;
            summary.text = summaryText;
            moneyRow.text = Row(start.money, end.money, "", false);
            timeRow.text = Row(start.timeWeeks, end.timeWeeks, " weeks", false);
            // Scenarios that do not use time (Scenario 2) hide the Time row.
            foreach (Transform t in timeRow.transform.parent)
                if (t.name.StartsWith("Time")) t.gameObject.SetActive(GameSession.ShowTimeStat);
            trustRow.text = Row(start.trust, end.trust, "", false);
            resistanceRow.text = Row(start.resistance, end.resistance, "", true);

            learnedText.text = List(end.knowledge, start.knowledge, "Nothing new recorded");
            unlockedText.text = List(end.unlocks, start.unlocks, "No new capability unlocked");
        }

        static string Row(int from, int to, string unit, bool higherIsWorse)
        {
            int d = to - from;
            string change = "";
            if (d != 0)
            {
                bool good = higherIsWorse ? d < 0 : d > 0;
                string hex = ColorUtility.ToHtmlStringRGB(good ? UITheme.LeafGreen : UITheme.Coral);
                change = string.Format("  <color=#{0}>({1})</color>", hex, StatDelta.Signed(d));
            }
            return from + unit + "  →  <b>" + to + unit + "</b>" + change;
        }

        /// <summary>New items as bullets; anything carried in from earlier scenarios on one compact line.</summary>
        static string List(List<string> now, List<string> before, string empty)
        {
            var lines = new List<string>();
            var earlier = new List<string>();
            var fresh = new List<string>();
            foreach (var id in now)
            {
                if (before.Contains(id)) earlier.Add(id);
                else fresh.Add(id);
            }
            // Up to three new items as bullets; more than that as one compact line so nothing is cut off.
            if (fresh.Count <= 3)
                foreach (var id in fresh) lines.Add("• <b>New:</b> " + id);
            else
                lines.Add("<b>New:</b> " + string.Join(", ", fresh.ToArray()));
            if (earlier.Count > 0)
                lines.Add("<size=80%><color=#" + ColorUtility.ToHtmlStringRGB(UITheme.Teal) + ">Earlier: " + string.Join(", ", earlier.ToArray()) + "</color></size>");
            return lines.Count == 0 ? empty : string.Join("\n", lines.ToArray());
        }

        void Replay()
        {
            GameSession.RestoreScenarioStart();
            GameSession.LoadScene(SceneManager.GetActiveScene().name);
        }

        void Next()
        {
            // After a replay, the campaign continues from the saved (original) outcome.
            if (GameSession.IsReplay)
                GameSession.LoadSavedCampaign();
            GameSession.LoadScene(nextScene);
        }
    }
}

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
        [SerializeField] TMP_Text capacityRow;
        [SerializeField] TMP_Text learnedText;
        [SerializeField] TMP_Text unlockedText;

        [SerializeField] Button replayButton;
        [SerializeField] TMP_Text replayLabel;
        [SerializeField] Button menuButton;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [Tooltip("Button sprites (normal, hover, pressed, selected, disabled) for the one primary action")]
        [SerializeField] Sprite[] primarySprites;
        [Tooltip("Button sprites for Main Menu when it is not the primary action")]
        [SerializeField] Sprite[] neutralSprites;

        string nextScene;

        bool wired;

        void Awake() { Wire(); }

        // Buttons are wired on first use as well as in Awake, so the overlay works however it is activated.
        void Wire()
        {
            if (wired) return;
            wired = true;
            replayButton.onClick.AddListener(Replay);
            menuButton.onClick.AddListener(GameSession.ReturnToMainMenu);
            nextButton.onClick.AddListener(Next);
        }

        public void ShowCompleted(string title, string summaryText, string next, string nextStageLabel)
        {
            Wire();
            nextScene = next;
            header.text = string.IsNullOrEmpty(next) || next == GameSession.MainMenuScene ? "Game Complete" : "Scenario Complete";
            replayLabel.text = "Replay Scenario";
            nextButton.gameObject.SetActive(!string.IsNullOrEmpty(next));
            nextLabel.text = string.IsNullOrEmpty(nextStageLabel) ? "Next" : nextStageLabel;
            if (GameSession.IsReplay)
                summaryText += "\n<i>This was a replay to try a different strategy. Your campaign continues from your original outcome.</i>";
            Fill(title, summaryText);
            // Game Complete has no Next: Main Menu becomes the primary (green) action and takes focus.
            bool hasNext = nextButton.gameObject.activeSelf;
            Style(menuButton, hasNext ? neutralSprites : primarySprites);
            Open();
            (hasNext ? nextButton : menuButton).Select();
        }

        public void ShowNotCompleted(string title, string reason)
        {
            Wire();
            nextScene = null;
            header.text = "Scenario Not Completed";
            replayLabel.text = "Retry Scenario";
            nextButton.gameObject.SetActive(false);
            Fill(title, reason);
            Style(menuButton, neutralSprites);
            Open();
            replayButton.Select();
        }

        /// <summary>The panel shows the same numbers as the HUD, so the HUD steps aside instead of overlapping it.</summary>
        void Open()
        {
            var hud = FindAnyObjectByType<StatHUD>();
            if (hud != null) hud.SetVisible(false);
            panel.Open();
        }

        static void Style(Button button, Sprite[] sprites)
        {
            if (button == null || sprites == null || sprites.Length < 5) return;
            var image = button.targetGraphic as Image;
            if (image != null) image.sprite = sprites[0];
            button.spriteState = new SpriteState
            {
                highlightedSprite = sprites[1], pressedSprite = sprites[2], selectedSprite = sprites[3], disabledSprite = sprites[4],
            };
        }

        void Fill(string title, string summaryText)
        {
            var end = GameSession.State;
            var start = GameSession.ScenarioStartState ?? end;
            scenarioTitle.text = title;
            summary.text = Eyebrow(summaryText);
            moneyRow.text = Row(start.money, end.money, "", StatKind.Money);
            timeRow.text = Row(start.timeWeeks, end.timeWeeks, " weeks", StatKind.Time);
            if (capacityRow != null)
                capacityRow.text = Row(start.capacity, end.capacity, "", StatKind.Capacity);
            // Rows for parameters the scenario does not use are hidden (e.g. Time in Scenario 2).
            foreach (Transform t in timeRow.transform.parent)
            {
                if (t.name.StartsWith("Time")) t.gameObject.SetActive(GameSession.ShowTimeStat);
                if (t.name.StartsWith("Capacity")) t.gameObject.SetActive(GameSession.ShowCapacityStat);
            }
            trustRow.text = Row(start.trust, end.trust, "", StatKind.Trust);
            resistanceRow.text = Row(start.resistance, end.resistance, "", StatKind.Resistance);

            learnedText.text = List(end.knowledge, start.knowledge, "Nothing new recorded");
            var text = List(end.unlocks, start.unlocks, "No new capability unlocked");
            var newBadges = end.badges.FindAll(b => !start.badges.Contains(b));
            if (newBadges.Count > 0)
                text += "\n<b>Badges:</b> " + string.Join(", ", newBadges.ToArray());
            // Game Complete: badges are recognition only, so show the campaign total.
            if (header.text == "Game Complete")
                text += string.Format("\n<b>Campaign badges: {0} of {1}</b>",
                    System.Array.FindAll(GameState.AllBadges, end.badges.Contains).Length, GameState.AllBadges.Length);
            unlockedText.text = text;
        }

        /// <summary>
        /// "from → to (change)" on tab stops so the arrows and numbers line up in every row. Spending Money,
        /// Time or Capacity is neutral (amber-brown); green and red are kept for Trust and Resistance outcomes.
        /// </summary>
        static string Row(int from, int to, string unit, StatKind kind)
        {
            int d = to - from;
            string change = "";
            if (d != 0)
                change = string.Format("<pos=78%><color={0}>({1})</color>", UITheme.ToHex(ChangeColor(kind, d)), StatDelta.Signed(d));
            return from + unit + "<pos=36%>→<pos=46%><b>" + to + unit + "</b>" + change;
        }

        public static Color ChangeColor(StatKind kind, int d)
        {
            switch (kind)
            {
                case StatKind.Trust: return d > 0 ? UITheme.Positive : UITheme.Negative;
                case StatKind.Resistance: return d < 0 ? UITheme.Positive : UITheme.Negative;
                default: return UITheme.Spend;
            }
        }

        /// <summary>
        /// A summary that opens with an all-caps rank ("ONE HEALTH CHAMPION — ...") shows that
        /// run as a small teal eyebrow line above the sentence instead of shouting inline. Same words.
        /// </summary>
        static string Eyebrow(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int k = text.IndexOf(" — ");
            if (k <= 0) return text;
            var head = text.Substring(0, k);
            bool caps = head == head.ToUpperInvariant() && head.ToLowerInvariant() != head;
            if (!caps) return text;
            return "<size=22><cspace=1.5><b><color=" + UITheme.ToHex(UITheme.Teal) + ">" + head + "</color></b></cspace></size>\n" + text.Substring(k + 3);
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
                lines.Add("<size=22><color=" + UITheme.ToHex(UITheme.InkMuted) + ">Earlier: " + string.Join(", ", earlier.ToArray()) + "</color></size>");
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

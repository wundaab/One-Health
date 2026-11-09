using System.Collections;
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
        bool gameComplete;

        const string Supporting = "You have completed One Health Innovator. Better health outcomes begin with better decisions and collective action.";

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
            gameComplete = string.IsNullOrEmpty(next) || next == GameSession.MainMenuScene;
            header.text = !gameComplete ? "Scenario Complete"
                : GameSession.State.badges.Contains("One Health Champion") ? "CONGRATULATIONS, ONE HEALTH CHAMPION!" : "CONGRATULATIONS!";
            header.enableAutoSizing = true;
            header.fontSizeMin = 30;
            header.fontSizeMax = 48;
            replayLabel.text = gameComplete ? "Replay Game" : "Replay Scenario";
            nextButton.gameObject.SetActive(!string.IsNullOrEmpty(next));
            nextLabel.text = string.IsNullOrEmpty(nextStageLabel) ? "Next" : nextStageLabel;
            if (GameSession.IsReplay)
                summaryText += "\n<i>This was a replay to try a different strategy. Your campaign continues from your original outcome.</i>";
            if (gameComplete) summaryText = FinalSummary(summaryText);
            Fill(title, summaryText);
            // Game Complete has no Next: Main Menu becomes the primary (green) action and takes focus.
            bool hasNext = nextButton.gameObject.activeSelf;
            Style(menuButton, hasNext ? neutralSprites : primarySprites);
            Open();
            UICues.Play(gameComplete ? Cue.Achievement : Cue.Transition);
            if (gameComplete) StartCoroutine(Celebrate());
            (hasNext ? nextButton : menuButton).Select();
        }

        public void ShowNotCompleted(string title, string reason)
        {
            Wire();
            nextScene = null;
            gameComplete = false;
            header.text = "Scenario Not Completed";
            replayLabel.text = "Retry Scenario";
            nextButton.gameObject.SetActive(false);
            Fill(title, reason);
            Style(menuButton, neutralSprites);
            Open();
            UICues.Play(Cue.Warning);
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

            if (gameComplete) { FillFinal(end); return; }
            learnedText.text = List(end.knowledge, start.knowledge, "Nothing new recorded");
            var text = List(end.unlocks, start.unlocks, "No new capability unlocked");
            var newBadges = end.badges.FindAll(b => !start.badges.Contains(b));
            if (newBadges.Count > 0)
                text += "\n<b>Badges:</b> " + string.Join(", ", newBadges.ToArray());
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

        /// <summary>GAME COMPLETE status and outcome, then the closing message (Enhancement Plan section 4).</summary>
        static string FinalSummary(string ending)
        {
            int k = ending.IndexOf(" — ");
            string outcome = k > 0 ? ending.Substring(0, k) : "";
            string status = outcome.Length > 0 && outcome != "GAME COMPLETE" ? "GAME COMPLETE · " + outcome : "GAME COMPLETE";
            int replay = ending.IndexOf("\n<i>");
            string extra = replay >= 0 ? ending.Substring(replay) : "";
            return status + " — " + Supporting + extra;
        }

        /// <summary>
        /// Final screen, right column: the achievements earned, each with what it recognises, then a short
        /// debrief of how the campaign went on evidence, prevention and treatment, trust and collaboration.
        /// </summary>
        void FillFinal(GameState end)
        {
            var column = learnedText.transform.parent;
            SetHeading(column, "LearnedHeading", string.Format("Achievements ({0} of {1})",
                System.Array.FindAll(GameState.AllBadges, end.badges.Contains).Length, GameState.AllBadges.Length));
            SetHeading(column, "UnlockedHeading", "Debrief");
            Place(column.Find("UnlockedHeading"), -86f, 44f);
            Place(learnedText.transform, 46f, 196f);
            Place(unlockedText.transform, -158f, 100f);

            var lines = new List<string>();
            foreach (var b in GameState.AllBadges)
                if (end.badges.Contains(b))
                    lines.Add("• <b>" + b + "</b> <color=" + UITheme.ToHex(UITheme.InkMuted) + ">— " + Achievements.Reason(b) + "</color>");
            learnedText.text = lines.Count > 0 ? string.Join("\n", lines.ToArray()) : "No badges this time. Replay to try a different strategy.";

            string resistance = end.resistance <= 30 ? "kept low" : end.resistance <= 60 ? "held at a moderate level" : "left high";
            string trust = end.trust >= 70 ? "strong" : end.trust >= 40 ? "steady" : "fragile";
            unlockedText.text = string.Format(
                "<b>Evidence:</b> {0} concepts recorded. <b>Collaboration:</b> {1} capabilities built.\n" +
                "<b>Prevention and treatment:</b> resistance pressure {2}. <b>Community trust:</b> {3}.",
                end.knowledge.Count, end.unlocks.Count, resistance, trust);
        }

        static void SetHeading(Transform column, string name, string text)
        {
            var t = column.Find(name);
            if (t != null) t.GetComponent<TMP_Text>().text = text;
        }

        static void Place(Transform t, float y, float height)
        {
            var rt = t as RectTransform;
            if (rt == null) return;
            // Wider than the default lists (one achievement per line), keeping the left edge under the heading.
            float left = rt.anchoredPosition.x - rt.sizeDelta.x / 2f, width = Mathf.Max(rt.sizeDelta.x, 500f);
            rt.anchoredPosition = new Vector2(left + width / 2f, y);
            rt.sizeDelta = new Vector2(width, height);
        }

        /// <summary>A short celebration: the heading pops in.</summary>
        IEnumerator Celebrate()
        {
            var t = header.transform;
            for (float e = 0f; e < 0.6f; e += Time.unscaledDeltaTime)
            {
                float k = e / 0.6f;
                t.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(k * Mathf.PI) * (1f - k * 0.5f));
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        void Replay()
        {
            if (gameComplete)
            {
                GameSession.StartNewCampaign();
                GameSession.LoadScene(GameSession.Scenarios[0].scene);
                return;
            }
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

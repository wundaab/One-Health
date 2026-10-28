using UnityEngine;

namespace OneHealth
{
    /// <summary>What a tutorial step points at. None centres the explanation on screen.</summary>
    public enum TutorialTarget { None, Hud, Money, Time, Trust, Resistance, Capacity, Pause, Options, LockedOption }

    public struct TutorialStep
    {
        public string title;
        public string body;
        public TutorialTarget target;

        public TutorialStep(TutorialTarget target, string title, string body)
        {
            this.target = target;
            this.title = title;
            this.body = body;
        }
    }

    /// <summary>
    /// Onboarding tips: their wording, and which ones the player has already seen. Seen flags are per
    /// device (PlayerPrefs), not per campaign, so starting a new game does not repeat them; Settings can
    /// reset them. Statements here describe the game's actual rules; keep them in step with the mechanics.
    /// </summary>
    public static class Tutorial
    {
        /// <summary>Scenario 1 opening: the dashboard, pause and how to do well.</summary>
        public const string Hud = "hud";
        /// <summary>The first decision with costed options.</summary>
        public const string Choices = "choices";
        /// <summary>The first time an option is locked.</summary>
        public const string Locked = "locked";
        /// <summary>The first time the Capacity chip appears (Scenario 2).</summary>
        public const string Capacity = "capacity";

        static readonly string[] All = { Hud, Choices, Locked, Capacity };
        const string KeyPrefix = "OHI_Tutorial_";

        /// <summary>True while a tip is on screen (pause and dialogue input stand down).</summary>
        public static bool IsOpen { get; internal set; }

        public static bool Seen(string id) { return PlayerPrefs.GetInt(KeyPrefix + id, 0) == 1; }

        public static void MarkSeen(string id)
        {
            PlayerPrefs.SetInt(KeyPrefix + id, 1);
            PlayerPrefs.Save();
        }

        /// <summary>"Skip tips": the player does not want any of them.</summary>
        public static void SkipAll() { foreach (var id in All) MarkSeen(id); }

        /// <summary>Settings: show every tip again the next time it applies.</summary>
        public static void ResetAll()
        {
            foreach (var id in All) PlayerPrefs.DeleteKey(KeyPrefix + id);
            PlayerPrefs.Save();
        }

        public static string Heading(string id) { return id == Hud ? "How to play" : "Tip"; }

        public static TutorialStep[] Steps(string id)
        {
            switch (id)
            {
                case Hud: return new[]
                {
                    new TutorialStep(TutorialTarget.None, "Welcome to Kivu",
                        "You will coordinate Kivu’s response to real health problems. Listen to people, choose an action, then see what changed. There is no perfect answer: aim for a responsible decision you could explain."),
                    new TutorialStep(TutorialTarget.Money, "Money",
                        "Money pays for actions. Every choice shows its cost before you commit. What you do not spend carries into the next scenario, so avoid spending it all early."),
                    new TutorialStep(TutorialTarget.Time, "Time",
                        "Time is the deadline for this scenario, counted in weeks. Most actions take a week. When it reaches zero, actions that need time can no longer be taken."),
                    new TutorialStep(TutorialTarget.Trust, "Trust",
                        "Trust shows how willing people are to work with you. Listening, involving the community and being honest about what you know usually raise it. Dismissing concerns or rushing usually lowers it."),
                    new TutorialStep(TutorialTarget.Resistance, "Resistance Pressure",
                        "This tracks conditions that help antimicrobial resistance grow, so lower is better. Careful assessment and prevention bring it down; using antimicrobials when they are not needed pushes it up. It is a game indicator, not a clinical measurement."),
                    new TutorialStep(TutorialTarget.Hud, "Reading the dashboard",
                        "After each decision, arrows under the chips show what changed. A red label such as Low or Severe means that area needs attention. Tap the i on any chip for a reminder."),
                    new TutorialStep(TutorialTarget.Pause, "Pause",
                        "Pause lets you change settings, restart this scenario from its beginning, or return to the main menu. The game saves at the start and end of every scenario."),
                    new TutorialStep(TutorialTarget.None, "How to do well",
                        "Gather information before you act. Involve the people affected. Use antimicrobials only when they are needed. Keep some resources for the steps ahead. At the end of each scenario you will see what changed, and you can replay it to try another approach."),
                };
                case Choices: return new[]
                {
                    new TutorialStep(TutorialTarget.Options, "Making a choice",
                        "Each option shows what it costs in Money, Time or Capacity. Its effect on Trust and Resistance Pressure appears after you decide, so think about the likely consequences first. Click or tap an option, or use the arrow keys and Enter."),
                };
                case Locked: return new[]
                {
                    new TutorialStep(TutorialTarget.LockedOption, "Locked options",
                        "A padlock means you cannot choose this option right now, usually because there is not enough Money, Time or Capacity left. The option says what is missing. If no option for a required step can be paid for, you can retry the scenario."),
                };
                case Capacity: return new[]
                {
                    new TutorialStep(TutorialTarget.Capacity, "New: Capacity",
                        "Capacity is how much work your team can still take on. Most actions use some, and bigger actions lock when it runs low. Spend it where it matters most."),
                };
                default: return new TutorialStep[0];
            }
        }
    }
}

using System.Collections.Generic;

namespace OneHealth
{
    /// <summary>What each badge recognises, for the final screen. Short enough for one line each. Keep in step with the badge cards in the scenario builders.</summary>
    public static class Achievements
    {
        static readonly Dictionary<string, string> reasons = new Dictionary<string, string>
        {
            { "Evidence Tracker", "kept claims to the evidence" },
            { "Joint Response", "linked all three sectors" },
            { "One Health Coordinator", "led across sectors" },
            { "Community Partner", "a plan people could use" },
            { "Responsible Stewardship", "careful antimicrobial use" },
            { "Precaution With Evidence", "acted early, then checked" },
            { "Kivu Coordinator", "honest next steps for Kivu" },
            { "One Health Champion", "finished every workstream" },
        };

        public static string Reason(string badge)
        {
            string r;
            return reasons.TryGetValue(badge, out r) ? r : "";
        }
    }
}

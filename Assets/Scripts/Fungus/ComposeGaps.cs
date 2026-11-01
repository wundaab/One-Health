using System.Collections.Generic;
using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Builds player-facing "gap" lines from recorded story flags: the unresolved items handed over
    /// from Scenario 3's field report, or the workstreams still incomplete in Scenario 4. Writes
    /// Gap1, Gap2 and GapList string variables (blueprint: "Show one/two unresolved gap cards",
    /// "Show gaps remaining"), and the field report's decisions in player-facing words
    /// (Report_Evidence, Report_Animal, Report_Route, Report_Package) for the report cards.
    /// </summary>
    [CommandInfo("One Health", "Compose Gaps", "Write Gap1, Gap2, GapList and the Report_* lines from recorded story flags.")]
    [AddComponentMenu("")]
    public class ComposeGaps : Command
    {
        public enum Source { FieldReportHandover, FinalResponseWorkstreams }

        [SerializeField] protected Source source = Source.FieldReportHandover;

        public override void OnEnter()
        {
            var s = GameSession.State;
            var gaps = new List<string>();
            if (source == Source.FieldReportHandover)
            {
                if (s.GetFlag("s04_leads_checked") == "1") gaps.Add("Only one lead was investigated.");
                var animal = s.GetFlag("s04_animal_response");
                if (animal == "dismiss") gaps.Add("The farmer’s request for animal-health support was dismissed.");
                else if (animal == "targeted") gaps.Add("Animal-health risks were only partly examined.");
                var package = s.GetFlag("s04_response_package");
                if (package == "none") gaps.Add("No response package was delivered.");
                else if (package == "water") gaps.Add("Animal-health questions remain.");
                else if (package == "animal") gaps.Add("Environmental concerns remain.");
                if (s.GetFlag("s04_closing") == "gaps") gaps.Add("The field report closed with important gaps.");
                if (s.GetFlag("s04_route_response") == "continue") gaps.Add("The route delay reduced the time available.");
                gaps.Add("It is still not known whether the reports share a cause.");
            }
            else
            {
                if (s.GetFlag("s05_community_complete") != "true") gaps.Add("Community health and prevention is not complete.");
                else if (s.GetFlag("s05_community_plan") == "weak") gaps.Add("The community plan may reduce cooperation.");
                if (s.GetFlag("s05_animal_complete") != "true") gaps.Add("Animal-health assessment is not complete.");
                else if (s.GetFlag("s05_animal_plan") == "prevention_only") gaps.Add("The animal-health concern still needs appropriate assessment.");
                if (s.GetFlag("s05_water_complete") != "true") gaps.Add("Water and environmental safety is not complete.");
                else if (s.GetFlag("s05_water_plan") == "guidance_only") gaps.Add("Water-quality follow-up remains a gap.");
                if (gaps.Count == 0) gaps.Add("All three workstreams are complete. Follow-up still needs clear responsibilities.");
            }

            var fc = GetFlowchart();
            FlowchartSync.SetString(fc, "Gap1", gaps.Count > 0 ? gaps[0] : "");
            FlowchartSync.SetString(fc, "Gap2", gaps.Count > 1 ? gaps[1] : "");
            FlowchartSync.SetString(fc, "GapList", "• " + string.Join("\n• ", gaps.ToArray()));

            // Report cards print these instead of the internal flag values (M4).
            FlowchartSync.SetString(fc, "Report_Evidence", Describe(s.GetFlag("s04_evidence_quality")));
            FlowchartSync.SetString(fc, "Report_Animal", Describe(s.GetFlag("s04_animal_response")));
            FlowchartSync.SetString(fc, "Report_Route", Describe(s.GetFlag("s04_route_response")));
            FlowchartSync.SetString(fc, "Report_Package", Describe(s.GetFlag("s04_response_package")));
            Continue();
        }

        /// <summary>Player-facing wording for a Scenario 3 field-report flag value, e.g. "redirect".</summary>
        public static string Describe(string flagValue)
        {
            switch (flagValue)
            {
                case "assess": return "Animal assessment and prevention plan delivered";
                case "targeted": return "Short-term targeted animal support provided";
                case "dismiss": return "Farmer request dismissed without action";
                case "redirect": return "Field team redirected via accessible route";
                case "restore": return "Access restored at higher resource investment";
                case "continue": return "Continued along original disrupted route";
                case "water": return "Water safety and community prevention package";
                case "animal": return "Animal health and responsible-use support package";
                case "balanced": return "Balanced cross-sector coordination package";
                case "none": return "No package delivered";
                case "strong": return "Strong evidence base established";
                case "partial": return "Partial evidence base with remaining gaps";
                default: return string.IsNullOrEmpty(flagValue) ? "Not recorded" : flagValue;
            }
        }

        public override string GetSummary() { return source.ToString(); }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

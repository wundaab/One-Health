using System.Collections.Generic;
using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Builds player-facing "gap" lines from recorded story flags: the unresolved items handed over
    /// from Scenario 3's field report, or the workstreams still incomplete in Scenario 4. Writes
    /// Gap1, Gap2 and GapList string variables (blueprint: "Show one/two unresolved gap cards",
    /// "Show gaps remaining").
    /// </summary>
    [CommandInfo("One Health", "Compose Gaps", "Write Gap1, Gap2 and GapList from recorded story flags.")]
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
            Continue();
        }

        public override string GetSummary() { return source.ToString(); }

        public override Color GetButtonColor() { return new Color32(163, 214, 200, 255); }
    }
}

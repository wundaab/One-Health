using Fungus;
using UnityEditor;
using UnityEngine;
using K = OneHealth.EditorTools.ScenarioKit;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Scenario 3 — The Warning Signs. Built from "One Health Innovator S04 Production Blueprint"
    /// (the team's S04; the project's Scenario 3 because the blueprint's S02 and S03 were combined).
    /// Story flags keep the blueprint's s04_* names so the next blueprint's handover rules map 1:1.
    /// </summary>
    public static class SC03Builder
    {
        public const string ScenePath = "Assets/Scenes/SC03.unity";
        const string Title = "Scenario 3 — The Warning Signs";
        const string You = "Coordinator (you)";
        const string Radio = "Radio operator";
        const string Amina = "Amina";
        const string Farmer = "Farmer";
        const string Water = "Water collector";
        const string AnimalWorker = "Animal-health worker";
        const string EnvOfficer = "Environmental officer";
        const string CommunityRep = "Community representative";

        static StatDelta D(int money, int time, int trust, int res, int cap) { return new StatDelta(money, time, trust, res, cap); }

        [MenuItem("One Health/Build Scenario 3")]
        public static string Build()
        {
            var k = K.CreateScene(ScenePath, "Assets/Art/01_Scenario_1/02_Environments/Kivu_Community_Outdoor_Meeting_Area.png");

            foreach (var key in new[] { "Money", "Time", "Trust", "Resistance", "Capacity", "Entry_Resistance" })
                k.Var<IntegerVariable>(key);

            // ---------------------------------------------------------------- blocks
            var start = k.StartBlock("START");
            var intro = k.Block("INTRO");
            var a1 = k.Block("A1_Three_Reports");
            var reports = k.Block("A1_Report_Cards");
            var rWater = k.Block("A1_Water_Report");
            var rAnimal = k.Block("A1_Animal_Report");
            var rCommunity = k.Block("A1_Community_Report");
            var logged = k.Block("A1_Reports_Logged");
            var logApply = k.Block("A1_Log_Reports");
            var leadWater = k.Block("A1_Lead_Water");
            var leadAnimal = k.Block("A1_Lead_Animal");
            var leadCommunity = k.Block("A1_Lead_Community");
            var a2 = k.Block("A2_Listen_Before_Acting");
            var a2Listen = k.Block("A2_Listen");
            var a2Rush = k.Block("A2_Rush");
            var a2Question = k.Block("A2_Follow_Up_Question");
            var qFarmer = k.Block("A2_Q_Farmer");
            var qWater = k.Block("A2_Q_Water");
            var qAnimal = k.Block("A2_Q_Animal");
            var a3 = k.Block("A3_Sort_Evidence");
            var cards = new Block[4];
            for (int i = 0; i < 4; i++) cards[i] = k.Block("A3_Statement_" + (i + 1));
            var a3Leads = k.Block("A3_Investigate_Leads");
            var a3Two = k.Block("A3_Two_Leads");
            var a3One = k.Block("A3_One_Lead");
            var a4 = k.Block("A4_Farmer_Needs_Response");
            var a4Choice = k.Block("A4_Response_Cards");
            var a4Assess = k.Block("A4_Assess");
            var a4Targeted = k.Block("A4_Targeted");
            var a4Dismiss = k.Block("A4_Dismiss");
            var a4ConfirmAssess = k.Block("A4_Confirm_Assess");
            var a4ConfirmTargeted = k.Block("A4_Confirm_Targeted");
            var a4ConfirmDismiss = k.Block("A4_Confirm_Dismiss");
            var a5 = k.Block("A5_Heavy_Rain");
            var a5Redirect = k.Block("A5_Redirect");
            var a5Restore = k.Block("A5_Restore");
            var a5Continue = k.Block("A5_Continue");
            var a6 = k.Block("A6_Response_Package");
            var a6None = k.Block("A6_No_Package");
            var a6Water = k.Block("A6_Water_Package");
            var a6Animal = k.Block("A6_Animal_Package");
            var a6Balanced = k.Block("A6_Balanced_Package");
            var a7 = k.Block("A7_Verify_Document");
            var a7Verify = k.Block("A7_Verify");
            var a7Add = k.Block("A7_Add_Intervention");
            var a7Gaps = k.Block("A7_Close_With_Gaps");
            var submit = k.Block("A7_Submit");
            var outcome = k.Block("OUTCOME");
            var failure = k.Block("FAILURE");

            // ---------------------------------------------------------------- start
            var begin = E.Add<BeginScenario>(start);
            E.Set(begin, "scenarioId", "S03");
            E.Set(begin, "timeBudgetWeeks", -1);
            E.Set(begin, "timeGrantWeeks", 5);
            E.Set(begin, "showTime", true);
            E.Set(begin, "showCapacity", true);
            k.FadeToBackground(start);
            E.Add<ShowHUD>(start);
            k.Music(start, "Assets/Audio/M02_Decision.wav.mp3");
            k.Call(start, intro);

            k.Card(intro, Title, "Three reports arrive from a nearby farming community.\n\n<size=80%>One Health Innovator is an educational game, not medical advice.</size>");
            k.Say(intro, "Coordinator, three reports need attention: several animals are unwell, residents have noticed changes in a local water source after heavy rain, and community members are worried about possible health risks. We do not yet know whether these reports are connected.", Radio);
            k.Say(intro, "Investigate the reports, separate observations from assumptions, and build a response your team can deliver.");
            k.Plain(intro, "Start investigation", a1);

            // ---------------------------------------------------------------- A1 — Three reports arrive
            k.Say(a1, "Coordinator, the team has received three reports. A farmer says several animals are unwell. Residents say the nearby water looks different after heavy rain. Community members are worried. We have not confirmed a connection.", Radio);
            k.Say(a1, "Open all three reports before choosing where to investigate first.", You);
            k.Call(a1, reports);

            k.IfBool(reports, "Viewed_Water", true);
            k.IfBool(reports, "Viewed_Animal", true);
            k.IfBool(reports, "Viewed_Community", true);
            k.Call(reports, logged);
            k.End(reports); k.End(reports); k.End(reports);
            k.Say(reports, "Choose a report to open.");
            k.Plain(reports, "WATER REPORT", rWater, k.Var<BooleanVariable>("Viewed_Water"));
            k.Plain(reports, "ANIMAL HEALTH REPORT", rAnimal, k.Var<BooleanVariable>("Viewed_Animal"));
            k.Plain(reports, "COMMUNITY REPORT", rCommunity, k.Var<BooleanVariable>("Viewed_Community"));

            ReportCard(k, rWater, "Viewed_Water", "WATER REPORT — Residents say the nearby water looks different after heavy rain.", reports);
            ReportCard(k, rAnimal, "Viewed_Animal", "ANIMAL HEALTH REPORT — A farmer says several animals are unwell.", reports);
            ReportCard(k, rCommunity, "Viewed_Community", "COMMUNITY REPORT — Community members are worried about possible health risks.", reports);

            k.FailIfNone(logged, failure);
            k.Option(logged, "Log the three reports", logApply, D(0, -1, 0, 0, 1));
            k.Apply(logApply, "A1", "Three reports logged", D(0, -1, 0, 0, 1));
            k.Flag(logApply, "s04_reports_viewed", "true");
            k.Say(logApply, "3 REPORTS LOGGED.");
            k.Say(logApply, "Initial picture recorded. The reports may be connected, but the evidence does not establish a shared cause.");
            k.Say(logApply, "Which lead will you inspect first?");
            k.Plain(logApply, "Water", leadWater);
            k.Plain(logApply, "Animal health", leadAnimal);
            k.Plain(logApply, "Community concerns", leadCommunity);
            foreach (var pair in new[] { new { b = leadWater, v = "water" }, new { b = leadAnimal, v = "animal health" }, new { b = leadCommunity, v = "community concerns" } })
            {
                k.Flag(pair.b, "s04_first_lead", pair.v);
                k.Call(pair.b, a2);
            }

            // ---------------------------------------------------------------- A2 — Listen before acting
            k.Say(a2, "People are worried, but each person is seeing only part of the situation.", Amina);
            k.Say(a2, "Some of my animals are unwell. I need practical help that protects them and my livelihood.", Farmer);
            k.Say(a2, "The water looked different after the rain. I cannot tell you why.", Water);
            k.Say(a2, "We should assess the animals before deciding what care is needed.", AnimalWorker);
            k.Say(a2, "Choose how you will begin the conversations.", You);
            k.FailIfNone(a2, failure);
            k.Option(a2, "Listen and record", a2Listen, D(-5, -1, 8, -1, 1));
            k.Option(a2, "Rush the interviews", a2Rush, D(0, 0, -5, 1, 0));

            k.Apply(a2Listen, "A2_Listen", "Listen and record", D(-5, -1, 8, -1, 1));
            k.Flag(a2Listen, "s04_interview_approach", "listen");
            k.Say(a2Listen, "People are more willing to share what they know. The team has a clearer starting point.");
            k.Say(a2Listen, "INTERVIEW NOTES ADDED.");
            k.Call(a2Listen, a2Question);
            k.Apply(a2Rush, "A2_Rush", "Rush the interviews", D(0, 0, -5, 1, 0));
            k.Flag(a2Rush, "s04_interview_approach", "rush");
            k.Say(a2Rush, "You move on quickly, but cooperation has weakened and some context remains unclear.");
            k.Say(a2Rush, "INTERVIEW NOTES ADDED.");
            k.Call(a2Rush, a2Question);

            // Follow-up question: reveals context, never causation (blueprint A2).
            k.Say(a2Question, "Choose one follow-up question.", You);
            k.Plain(a2Question, "Ask the farmer when the animals became unwell", qFarmer);
            k.Plain(a2Question, "Ask the water collector where people collect water", qWater);
            k.Plain(a2Question, "Ask the animal-health worker what is needed first", qAnimal);
            Question(k, qFarmer, Farmer, "Over the past week. Some are eating less and moving slowly.", a3);
            Question(k, qWater, Water, "Most households here use the same source near the fields.", a3);
            Question(k, qAnimal, AnimalWorker, "An assessment of the animals. Then we can decide what care is appropriate.", a3);

            // ---------------------------------------------------------------- A3 — Sort evidence from assumptions
            k.Say(a3, "A change in water appearance deserves investigation, but appearance alone cannot tell us whether water is unsafe.", EnvOfficer);
            k.Say(a3, "The animal report needs assessment. It does not prove what is happening in the water.", AnimalWorker);
            k.Say(a3, "Place each statement where the evidence supports it.", You);
            k.Call(a3, cards[0]);
            string[] statements =
            {
                "Residents noticed a change after rain.",
                "Some animals are unwell.",
                "The water caused the animal illness.",
                "The reports may share a risk pathway.",
            };
            bool[] observed = { true, true, false, false };
            for (int i = 0; i < 4; i++)
            {
                var b = cards[i];
                var next = i < 3 ? cards[i + 1] : a3Leads;
                var right = k.Block("A3_Statement_" + (i + 1) + "_Right");
                var wrong = k.Block("A3_Statement_" + (i + 1) + "_Wrong");
                k.Say(b, "STATEMENT " + (i + 1) + " OF 4 — “" + statements[i] + "”");
                k.Plain(b, "OBSERVED", observed[i] ? right : wrong);
                k.Plain(b, "NOT YET KNOWN", observed[i] ? wrong : right);
                k.Call(right, next);
                if (observed[i])
                    k.Say(wrong, "This was reported directly. Move it to OBSERVED.");
                else
                {
                    k.Say(wrong, "That conclusion goes beyond the evidence. Move it to NOT YET KNOWN.");
                    k.SetBool(wrong, "Overclaimed", true);
                }
                k.Call(wrong, b);
            }

            k.Say(a3Leads, "Good investigation separates observations from explanations. A possible connection is not proof.");
            k.IfBool(a3Leads, "Overclaimed", false);
            k.Badge(a3Leads, "Evidence Tracker");
            k.Flag(a3Leads, "s04_evidence_badge", "true");
            k.Say(a3Leads, "BADGE — EVIDENCE TRACKER. You kept both unsupported statements out of the evidence.");
            k.Else(a3Leads);
            k.Flag(a3Leads, "s04_evidence_badge", "false");
            k.End(a3Leads);
            k.Knowledge(a3Leads, "Observation is not explanation");
            k.Say(a3Leads, "How many leads will the team investigate?", You);
            k.FailIfNone(a3Leads, failure);
            k.Option(a3Leads, "Investigate two leads", a3Two, D(-10, -1, 2, -2, -2));
            k.Option(a3Leads, "Investigate one lead", a3One, D(-5, 0, 0, 0, 0));
            k.Apply(a3Two, "A3_Two", "Investigate two leads", D(-10, -1, 2, -2, -2));
            k.Flag(a3Two, "s04_evidence_quality", "strong");
            k.Flag(a3Two, "s04_leads_checked", "2");
            k.Call(a3Two, a4);
            k.Apply(a3One, "A3_One", "Investigate one lead", D(-5, 0, 0, 0, 0));
            k.Flag(a3One, "s04_evidence_quality", "partial");
            k.Flag(a3One, "s04_leads_checked", "1");
            k.Call(a3One, a4);

            // ---------------------------------------------------------------- A4 — The farmer needs a response
            k.Say(a4, "I cannot wait indefinitely. I need a practical plan for the animals and my family’s income.", Farmer);
            k.Say(a4, "We can arrange an assessment and discuss prevention. We should not guess at treatment.", AnimalWorker);
            k.Say(a4, "Choose a response your team can deliver.", You);
            k.Call(a4, a4Choice);
            k.FailIfNone(a4Choice, failure);
            k.Option(a4Choice, "Assess animals and support prevention", a4ConfirmAssess, D(-15, -1, 6, -3, -2));
            k.Option(a4Choice, "Targeted short-term support", a4ConfirmTargeted, D(-10, -1, 2, -1, -1));
            k.Option(a4Choice, "Dismiss the request", a4ConfirmDismiss, D(0, 0, -8, 2, 0));
            Confirm(k, a4ConfirmAssess, a4Assess, a4Choice);
            Confirm(k, a4ConfirmTargeted, a4Targeted, a4Choice);
            Confirm(k, a4ConfirmDismiss, a4Dismiss, a4Choice);
            Decision(k, a4Assess, "A4_Assess", "Assess animals and support prevention", D(-15, -1, 6, -3, -2), "s04_animal_response", "assess",
                "The team can investigate animal health and reduce avoidable risks while keeping appropriate treatment available when needed.", "DECISION RECORDED. RESOURCES UPDATED.", a5);
            Decision(k, a4Targeted, "A4_Targeted", "Targeted short-term support", D(-10, -1, 2, -1, -1), "s04_animal_response", "targeted",
                "You address an immediate need, but some risks remain unexamined.", "DECISION RECORDED. RESOURCES UPDATED.", a5);
            Decision(k, a4Dismiss, "A4_Dismiss", "Dismiss the request", D(0, 0, -8, 2, 0), "s04_animal_response", "dismiss",
                "The concern remains, and cooperation has weakened.", "DECISION RECORDED. RESOURCES UPDATED.", a5);

            // ---------------------------------------------------------------- A5 — Heavy rain changes the plan
            k.Music(a5, "Assets/Audio/M03_Tension.wav.mp3");
            k.Say(a5, "Coordinator, heavy rain has made the planned route difficult to use. The team must choose how to continue.", Radio);
            k.Say(a5, "The disruption changes our options. It does not tell us what caused the reports.", You);
            k.Say(a5, "FIELD MAP — The planned route is blocked. One site is still accessible.");
            k.FailIfNone(a5, failure);
            k.Option(a5, "Redirect the team", a5Redirect, D(-3, -1, 1, 0, -1));
            k.Option(a5, "Restore access", a5Restore, D(-12, 0, 2, 0, -2));
            k.Option(a5, "Continue the old plan", a5Continue, D(0, -2, -2, 0, 0));
            Decision(k, a5Redirect, "A5_Redirect", "Redirect the team", D(-3, -1, 1, 0, -1), "s04_route_response", "redirect",
                "The team can continue, but has less capacity for other work.", "FIELD PLAN UPDATED.", a6);
            Decision(k, a5Restore, "A5_Restore", "Restore access", D(-12, 0, 2, 0, -2), "s04_route_response", "restore",
                "Access is restored at a higher resource cost.", "FIELD PLAN UPDATED.", a6);
            Decision(k, a5Continue, "A5_Continue", "Continue the old plan", D(0, -2, -2, 0, 0), "s04_route_response", "continue",
                "The delay reduces time available for the response.", "FIELD PLAN UPDATED.", a6);

            // ---------------------------------------------------------------- A6 — Assemble the response package
            k.Music(a6, "Assets/Audio/M02_Decision.wav.mp3");
            k.Say(a6, "People need clear advice and a chance to help shape the response.", CommunityRep);
            k.Say(a6, "We can support practical water-safety measures while the concern is investigated.", EnvOfficer);
            k.Say(a6, "Animal assessment and responsible antimicrobial use must remain part of the plan.", AnimalWorker);
            k.Say(a6, "Choose the package your team can realistically deliver.", You);
            // No affordable package -> no package delivered (Unresolved), not a failure (blueprint A6 / section 4).
            k.FailIfNone(a6, a6None);
            k.Option(a6, "Water safety and community prevention", a6Water, D(-12, -1, 5, -3, -1));
            k.Option(a6, "Animal health and responsible-use support", a6Animal, D(-12, -1, 3, -3, -1));
            k.Option(a6, "Balanced cross-sector package", a6Balanced, D(-20, -1, 7, -5, -3));
            k.Say(a6None, "No response package can be delivered with the resources remaining.");
            k.Flag(a6None, "s04_response_package", "none");
            k.Flag(a6None, "s04_joint_badge", "false");
            k.Call(a6None, a7);
            Decision(k, a6Water, "A6_Water", "Water safety and community prevention", D(-12, -1, 5, -3, -1), "s04_response_package", "water",
                "Community prevention is strengthened; animal-health questions remain.", null, a7);
            Decision(k, a6Animal, "A6_Animal", "Animal health and responsible-use support", D(-12, -1, 3, -3, -1), "s04_response_package", "animal",
                "Animal-health support improves; environmental concerns remain.", null, a7);
            Decision(k, a6Balanced, "A6_Balanced", "Balanced cross-sector package", D(-20, -1, 7, -5, -3), "s04_response_package", "balanced",
                "The response covers more of the system, but leaves fewer resources for other needs.", null, a7);
            InsertBefore(k, a6Water, "s04_joint_badge", "false");
            InsertBefore(k, a6Animal, "s04_joint_badge", "false");
            // Badge only when the balanced package is selected and affordable.
            int callAt = a6Balanced.CommandList.Count - 1;
            var badge = E.Insert<AwardBadge>(a6Balanced, callAt);
            E.Set(badge, "badgeId", "Joint Response");
            var jf = E.Insert<SetCampaignFlag>(a6Balanced, callAt + 1);
            E.Set(jf, "key", "s04_joint_badge");
            E.Set(jf, "value", "true");
            E.Say(a6Balanced, callAt + 2, "BADGE — JOINT RESPONSE.", null, null);
            E.Set(a6Balanced.CommandList[callAt + 2], "setSayDialog", k.sayDialog);

            // ---------------------------------------------------------------- A7 — Verify, document and hand over
            k.Say(a7, "Before we close this response, we must record what changed, what remains uncertain and what needs follow-up.", You);
            k.Say(a7, "A clear handover will help the next team avoid repeating our assumptions.", Amina);
            k.Card(a7, "Draft field report",
                "Leads checked: {$s04_leads_checked}\nEvidence quality: {$s04_evidence_quality}\nAnimal-health response: {$s04_animal_response}\nRoute decision: {$s04_route_response}\nResponse package: {$s04_response_package}\n" +
                "Remaining: Money {$Money}  ·  Time {$Time} weeks  ·  Capacity {$Capacity}\nStill unanswered: whether the three reports share a cause.");
            // Unresolved also covers closing with gaps when no feasible intervention could be paid for.
            k.CheckAffordable(a7, k.Var<BooleanVariable>("CanVerify"), D(-3, -1, 0, 0, 0));
            k.CheckAffordable(a7, k.Var<BooleanVariable>("CanAdd"), D(-8, -1, 0, 0, -1));
            k.Say(a7, "Choose a closing action.", You);
            k.Option(a7, "Verify and document", a7Verify, D(-3, -1, 3, -1, 0));
            k.Option(a7, "Add one feasible intervention", a7Add, D(-8, -1, 2, -2, -1));
            k.Option(a7, "Close with gaps", a7Gaps, D(0, 0, -5, 1, 0));
            Decision(k, a7Verify, "A7_Verify", "Verify and document", D(-3, -1, 3, -1, 0), "s04_closing", "verify",
                "Your report separates findings from unanswered questions.", null, submit);
            Decision(k, a7Add, "A7_Add", "Add one feasible intervention", D(-8, -1, 2, -2, -1), "s04_closing", "intervention",
                "You strengthened the response but have fewer resources for what comes next.", null, submit);
            Decision(k, a7Gaps, "A7_Gaps", "Close with gaps", D(0, 0, -5, 1, 0), "s04_closing", "gaps",
                "The immediate activity is closed, but important gaps remain.", null, submit);
            k.Plain(submit, "SUBMIT FIELD REPORT", outcome);

            // ---------------------------------------------------------------- Outcome (blueprint section 4)
            k.Flag(outcome, "s04_report_submitted", "true");
            k.Say(outcome, "FIELD REPORT SAVED.");
            k.SetString(outcome, "Ending", "partial");
            // Strong: >=2 leads, strong evidence, package selected, Trust >= 50, Capacity >= 10, Resistance Pressure not above entry.
            k.IfString(outcome, "s04_leads_checked", "2");
            k.IfString(outcome, "s04_evidence_quality", "strong");
            k.IfString(outcome, "s04_response_package", "none", false, K.Ne);
            k.IfInt(outcome, "Trust", K.Ge, 50);
            k.IfInt(outcome, "Capacity", K.Ge, 10);
            k.IfIntVar(outcome, "Resistance", K.Le, "Entry_Resistance");
            k.SetString(outcome, "Ending", "strong");
            for (int i = 0; i < 6; i++) k.End(outcome);
            // Unresolved: no package delivered, or closed with gaps when no feasible intervention could be paid for.
            k.IfString(outcome, "s04_response_package", "none");
            k.SetString(outcome, "Ending", "unresolved");
            k.End(outcome);
            k.IfString(outcome, "s04_closing", "gaps");
            k.IfBool(outcome, "CanVerify", false);
            k.IfBool(outcome, "CanAdd", false);
            k.SetString(outcome, "Ending", "unresolved");
            k.End(outcome); k.End(outcome); k.End(outcome);
            k.Flag(outcome, "s04_ending", "{$Ending}");

            k.IfString(outcome, "Ending", "strong");
            k.Badge(outcome, "One Health Coordinator");
            k.SetString(outcome, "EndingText", "STRONG RESPONSE. Your team acted on evidence, coordinated across sectors and kept key risks in view. Some uncertainty remains, as it often does in real systems.");
            k.IfString(outcome, "Ending", "partial", true);
            k.SetString(outcome, "EndingText", "PARTIAL SUCCESS. You addressed part of the concern, but important gaps remain. Review the consequences before continuing.");
            k.Else(outcome);
            k.SetString(outcome, "EndingText", "UNRESOLVED. The response did not close the main operational gaps. The next team will inherit these risks and unanswered questions.");
            k.End(outcome);
            k.Music(outcome, "Assets/Audio/M04_Reflection.wav.mp3");
            k.Say(outcome, "{$EndingText}");
            k.IfString(outcome, "Ending", "strong");
            k.Say(outcome, "BADGE — ONE HEALTH COORDINATOR.");
            k.End(outcome);
            k.Knowledge(outcome, "Exposure is a clue, not proof");
            var complete = E.Add<CompleteScenario>(outcome);
            E.Set(complete, "scenarioId", "S03");
            E.Set(complete, "scenarioTitle", Title);
            E.Set(complete, "nextScene", "SC04");
            E.Set(complete, "nextLabel", "Continue to Scenario 4");
            E.Set(complete, "summary", "{$EndingText}");

            // ---------------------------------------------------------------- Failure (blueprint section 4)
            k.Say(failure, "RESPONSE BLOCKED. Your team does not have the resources needed to complete the required action. Review your choices and try again.");
            var fail = E.Add<FailScenario>(failure);
            E.Set(fail, "scenarioTitle", Title);
            E.Set(fail, "reason", "RESPONSE BLOCKED. Your team does not have the resources needed to complete the required action. Review your choices and try again.");

            k.Save();
            K.AddToBuild(ScenePath);
            return E.DumpAll(k.fc);
        }

        // ------------------------------------------------------------------ helpers

        static void ReportCard(K k, Block b, string viewedKey, string text, Block back)
        {
            k.SetBool(b, viewedKey, true);
            k.Say(b, text);
            k.Call(b, back);
        }

        static void Question(K k, Block b, string who, string answer, Block next)
        {
            k.Say(b, answer, who);
            k.Say(b, "This adds context. It does not prove what caused the reports.");
            k.Call(b, next);
        }

        /// <summary>"Commit to this plan?" before effects are applied (blueprint A4).</summary>
        static void Confirm(K k, Block b, Block commit, Block back)
        {
            k.Say(b, "Commit to this plan?");
            k.Plain(b, "Commit", commit);
            k.Plain(b, "Choose again", back);
        }

        static void Decision(K k, Block b, string id, string action, StatDelta d, string flagKey, string flagValue,
            string feedback, string notification, Block next)
        {
            k.Apply(b, id, action, d);
            k.Flag(b, flagKey, flagValue);
            k.Say(b, feedback);
            if (!string.IsNullOrEmpty(notification)) k.Say(b, notification);
            k.Call(b, next);
        }

        static void InsertBefore(K k, Block b, string key, string value)
        {
            var f = E.Insert<SetCampaignFlag>(b, b.CommandList.Count - 1);
            E.Set(f, "key", key);
            E.Set(f, "value", value);
        }
    }
}

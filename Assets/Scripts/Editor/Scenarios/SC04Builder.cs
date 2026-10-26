using Fungus;
using UnityEditor;
using UnityEngine;
using K = OneHealth.EditorTools.ScenarioKit;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Scenario 4 — Kivu's Turning Point (final scenario). Built from "One Health Innovator S05
    /// Production Blueprint" (the team's S05; the project's Scenario 4). Reads the s04_* flags
    /// written by Scenario 3 and writes s05_* flags, as the blueprint specifies.
    /// </summary>
    public static class SC04Builder
    {
        public const string ScenePath = "Assets/Scenes/SC04.unity";
        const string Title = "Scenario 4 — Kivu’s Turning Point";
        const string You = "Coordinator (you)";
        const string Radio = "Radio operator";
        const string Amina = "Amina";
        const string Farmer = "Farmer";
        const string Water = "Water collector";
        const string AnimalWorker = "Animal-health worker";
        const string EnvOfficer = "Environmental officer";
        const string CommunityRep = "Community representative";
        const string Narrator = "Narrator";

        static StatDelta D(int money, int time, int trust, int res, int cap) { return new StatDelta(money, time, trust, res, cap); }

        [MenuItem("One Health/Build Scenario 4")]
        public static string Build()
        {
            var k = K.CreateScene(ScenePath, "Assets/Art/01_Scenario_1/02_Environments/Kivu_Community_Outdoor_Meeting_Area.png");
            foreach (var key in new[] { "Money", "Time", "Trust", "Resistance", "Capacity", "Entry_Resistance" })
                k.Var<IntegerVariable>(key);
            foreach (var key in new[] { "s04_ending", "Gap1", "Gap2", "GapList" })
                k.Var<StringVariable>(key);

            // ---------------------------------------------------------------- blocks
            var start = k.StartBlock("START");
            var intro = k.Block("INTRO");
            var a1 = k.Block("A1_Read_Handover");
            var a1Review = k.Block("A1_Review_Handover");
            var hub = k.Block("WORKSTREAMS");
            var pickCommunity = k.Block("Pick_Community");
            var pickAnimal = k.Block("Pick_Animal");
            var pickWater = k.Block("Pick_Water");
            var a2 = k.Block("A2_Community_Health");
            var pair = new Block[3];
            for (int i = 0; i < 3; i++) pair[i] = k.Block("A2_Pair_" + (i + 1));
            var a2Review = k.Block("A2_Review_Plan");
            var a2Revise = k.Block("A2_Revise");
            var a2Commit = k.Block("A2_Commit");
            var a2Safe = k.Block("A2_Safe_Plan");
            var a2Weak = k.Block("A2_Weak_Plan");
            var a3 = k.Block("A3_Animal_Health");
            var a3Assess = k.Block("A3_Assessment");
            var a3Prevent = k.Block("A3_Prevention_Only");
            var a4 = k.Block("A4_Water_Environment");
            var sort = new Block[3];
            for (int i = 0; i < 3; i++) sort[i] = k.Block("A4_Card_" + (i + 1));
            var a4Plan = k.Block("A4_Choose_Plan");
            var a4Full = k.Block("A4_Guidance_FollowUp");
            var a4Guidance = k.Block("A4_Guidance_Only");
            var a5 = k.Block("A5_Resource_Disruption");
            var a5Combine = k.Block("A5_Combine");
            var a5Pay = k.Block("A5_Pay");
            var a5Keep = k.Block("A5_Keep");
            var a6 = k.Block("A6_Negotiate");
            var tokens = k.Block("A6_Tokens");
            var tFarmer = k.Block("A6_Token_Farmer");
            var tCommunity = k.Block("A6_Token_Community");
            var tEnv = k.Block("A6_Token_Environment");
            var a6Final = k.Block("A6_Final_Priority");
            var a6Protect = k.Block("A6_Protect");
            var a6Prioritize = k.Block("A6_Prioritize");
            var a6Focus = k.Block("A6_Focus");
            var a7 = k.Block("A7_Verify_Brief");
            var verify = new Block[4];
            for (int i = 0; i < 4; i++) verify[i] = k.Block("A7_Status_" + (i + 1));
            var a7Verified = k.Block("A7_Report_Verified");
            var a7Brief = k.Block("A7_Briefing");
            var a7Transparent = k.Block("A7_Transparent");
            var a7Overconfident = k.Block("A7_Overconfident");
            var a8 = k.Block("A8_Close_Response");
            var a8Shared = k.Block("A8_Shared");
            var a8Basic = k.Block("A8_Basic");
            var ending = k.Block("ENDING");
            var failure = k.Block("FAILURE");

            // ---------------------------------------------------------------- start & handover (blueprint section 2)
            var begin = E.Add<BeginScenario>(start);
            E.Set(begin, "scenarioId", "S04");
            E.Set(begin, "timeBudgetWeeks", -1);
            E.Set(begin, "timeGrantWeeks", 5);
            E.Set(begin, "showTime", true);
            E.Set(begin, "showCapacity", true);
            k.FadeToBackground(start);
            E.Add<ShowHUD>(start);
            k.Music(start, "Assets/Audio/M01_Opening.wav.mp3");
            k.Call(start, intro);

            k.Card(intro, Title, "The final response for Kivu.\n\n<size=80%>One Health Innovator is an educational game, not medical advice.</size>");
            k.Say(intro, "The field report is in. The team recorded what was observed, what remains uncertain, and which actions were completed. Now Kivu needs a coordinated response that protects people, supports animal health, and reduces environmental risks without assuming a single cause.", Narrator);
            var gaps = E.Add<ComposeGaps>(intro);
            E.Set(gaps, "source", (int)ComposeGaps.Source.FieldReportHandover);
            k.IfString(intro, "s04_ending", "strong");
            k.Say(intro, "Our investigation gives us a stronger starting point. We still need to deliver the plan and verify the results.", You);
            k.IfString(intro, "s04_ending", "partial", true);
            k.Say(intro, "We made progress, but some gaps remain. We must prioritize carefully and avoid repeating assumptions.", You);
            k.Card(intro, "Unresolved gap", "{$Gap1}");
            k.Else(intro);
            k.Say(intro, "The report leaves important gaps. We will start with what is known, make uncertainty visible, and focus on actions we can deliver safely.", You);
            k.Card(intro, "Unresolved gaps", "{$Gap1}\n\n{$Gap2}");
            k.End(intro);
            k.Say(intro, "Assign your limited resources, complete all three workstreams, and deliver a final public briefing.");
            k.Plain(intro, "START FINAL RESPONSE", a1);

            // ---------------------------------------------------------------- A1 — Read handover and plan
            k.Say(a1, "We have three workstreams and limited resources. Choose the order in which the team begins, but all three must be completed before the final briefing.", You);
            k.Say(a1, "Please make the plan clear to the community. People need to know what is happening and what remains uncertain.", Amina);
            k.FailIfNone(a1, failure);
            k.Option(a1, "Review the handover cards", a1Review, D(0, -1, 0, 0, 1));
            k.Apply(a1Review, "A1", "Review handover", D(0, -1, 0, 0, 1));
            k.Flag(a1Review, "s05_handover_read", "true");
            k.Card(a1Review, "Field report handover",
                "Leads checked: {$s04_leads_checked}  ·  Evidence quality: {$s04_evidence_quality}\nAnimal-health response: {$s04_animal_response}\nRoute decision: {$s04_route_response}\nResponse package: {$s04_response_package}\n\n{$GapList}");
            k.Say(a1Review, "HANDOVER LOADED.");
            k.Call(a1Review, hub);
            foreach (var key in new[] { "s04_leads_checked", "s04_evidence_quality", "s04_animal_response", "s04_route_response", "s04_response_package" })
                k.Var<StringVariable>(key);

            // Workstream hub: the player chooses the order; all three are mandatory.
            k.IfBool(hub, "Done_Community", true);
            k.IfBool(hub, "Done_Animal", true);
            k.IfBool(hub, "Done_Water", true);
            k.Call(hub, a5);
            k.End(hub); k.End(hub); k.End(hub);
            k.Say(hub, "Choose the next workstream. All three must be completed.", You);
            k.Plain(hub, "Community health and prevention", pickCommunity, k.Var<BooleanVariable>("Done_Community"));
            k.Plain(hub, "Animal-health assessment and responsible use", pickAnimal, k.Var<BooleanVariable>("Done_Animal"));
            k.Plain(hub, "Water and environmental safety", pickWater, k.Var<BooleanVariable>("Done_Water"));
            Pick(k, pickCommunity, "community", a2);
            Pick(k, pickAnimal, "animal", a3);
            Pick(k, pickWater, "water", a4);

            // ---------------------------------------------------------------- A2 — Community health and prevention
            k.Say(a2, "Residents want to know what they can do now. We must give practical advice without claiming the reports prove one source.", Amina);
            k.Say(a2, "People cooperate when we explain what is known, what is uncertain, and how they can help.", CommunityRep);
            k.SetInt(a2, "UnsafeCards", K.Assign, 0);
            k.Call(a2, pair[0]);
            string[,] pairs =
            {
                { "Share safe water-handling steps", "Spread an unverified warning" },
                { "Promote hand hygiene and prevention", "Blame a group for the illness" },
                { "Give clear reporting and care-seeking guidance", "Promise the cause is confirmed" },
            };
            for (int i = 0; i < 3; i++)
            {
                var next = i < 2 ? pair[i + 1] : a2Review;
                var safe = k.Block("A2_Pair_" + (i + 1) + "_Safe");
                var unsafeB = k.Block("A2_Pair_" + (i + 1) + "_Unsafe");
                k.Say(pair[i], "PLAN STEP " + (i + 1) + " OF 3 — Choose one card.");
                k.Plain(pair[i], pairs[i, 0], safe);
                k.Plain(pair[i], pairs[i, 1], unsafeB);
                k.Call(safe, next);
                k.SetInt(unsafeB, "UnsafeCards", K.Add, 1);
                k.Call(unsafeB, next);
            }
            k.IfInt(a2Review, "UnsafeCards", K.Eq, 0);
            k.Say(a2Review, "Your plan is practical, respectful and honest about what is known.");
            k.Call(a2Review, a2Commit);
            k.Else(a2Review);
            k.Say(a2Review, "This plan may reduce cooperation because it blames people or presents an unconfirmed claim as fact.");
            k.IfBool(a2Review, "RevisionUsed", false);
            k.Plain(a2Review, "Revise the plan (one revision)", a2Revise);
            k.Plain(a2Review, "Commit this plan", a2Commit);
            k.Else(a2Review);
            k.Call(a2Review, a2Commit);
            k.End(a2Review);
            k.End(a2Review);
            k.SetBool(a2Revise, "RevisionUsed", true);
            k.SetInt(a2Revise, "UnsafeCards", K.Assign, 0);
            k.Call(a2Revise, pair[0]);
            // Commit: the plan's quality decides its effects; it must be affordable.
            k.IfInt(a2Commit, "UnsafeCards", K.Eq, 0);
            k.CheckAffordable(a2Commit, k.Var<BooleanVariable>("CanPayPlan"), D(-10, -1, 0, 0, -1));
            k.Else(a2Commit);
            k.CheckAffordable(a2Commit, k.Var<BooleanVariable>("CanPayPlan"), D(-5, -1, 0, 0, 0));
            k.End(a2Commit);
            k.IfBool(a2Commit, "CanPayPlan", false);
            k.Call(a2Commit, failure);
            k.End(a2Commit);
            k.IfInt(a2Commit, "UnsafeCards", K.Eq, 0);
            k.Call(a2Commit, a2Safe);
            k.Else(a2Commit);
            k.Call(a2Commit, a2Weak);
            k.End(a2Commit);
            k.Apply(a2Safe, "A2_Safe", "Complete safe plan", D(-10, -1, 6, -2, -1));
            k.Flag(a2Safe, "s05_community_plan", "strong");
            k.Flag(a2Safe, "s05_community_complete", "true");
            k.SetBool(a2Safe, "Done_Community", true);
            k.Say(a2Safe, "COMMUNITY PLAN READY. Practical steps are clear and respectful.");
            k.Badge(a2Safe, "Community Partner");
            k.Say(a2Safe, "BADGE — COMMUNITY PARTNER.");
            k.Say(a2Safe, "COMMUNITY FEEDBACK RECEIVED.");
            k.Call(a2Safe, hub);
            k.Apply(a2Weak, "A2_Weak", "Submit unsupported/blaming plan", D(-5, -1, -7, 1, 0));
            k.Flag(a2Weak, "s05_community_plan", "weak");
            k.Flag(a2Weak, "s05_community_complete", "true");
            k.SetBool(a2Weak, "Done_Community", true);
            k.Say(a2Weak, "This message may reduce cooperation because it blames people or presents an unconfirmed claim as fact.");
            k.Say(a2Weak, "COMMUNITY FEEDBACK RECEIVED.");
            k.Call(a2Weak, hub);

            // ---------------------------------------------------------------- A3 — Animal-health assessment and responsible use
            k.Say(a3, "The field report records animal illness, but it does not identify a cause. We need appropriate assessment and practical prevention.", AnimalWorker);
            k.Say(a3, "I need advice that protects my animals and livelihood. I do not want guesswork.", Farmer);
            k.Say(a3, "Choose an animal-health plan. This workstream cannot be skipped.", You);
            k.FailIfNone(a3, failure);
            k.Option(a3, "Assessment + prevention", a3Assess, D(-12, -1, 4, -3, -2));
            k.Option(a3, "Prevention support only", a3Prevent, D(-7, -1, 2, -1, -1));
            k.Apply(a3Assess, "A3_Assess", "Assessment + prevention", D(-12, -1, 4, -3, -2));
            k.Flag(a3Assess, "s05_animal_plan", "assessment");
            k.Flag(a3Assess, "s05_animal_complete", "true");
            k.SetBool(a3Assess, "Done_Animal", true);
            k.Say(a3Assess, "ANIMAL-HEALTH PLAN READY. The team supports assessment, animal welfare and responsible antimicrobial use. Appropriate treatment remains available when indicated.");
            k.Badge(a3Assess, "Responsible Stewardship");
            k.Say(a3Assess, "BADGE — RESPONSIBLE STEWARDSHIP.");
            k.Call(a3Assess, hub);
            k.Apply(a3Prevent, "A3_Prevention", "Prevention support only", D(-7, -1, 2, -1, -1));
            k.Flag(a3Prevent, "s05_animal_plan", "prevention_only");
            k.Flag(a3Prevent, "s05_animal_complete", "true");
            k.SetBool(a3Prevent, "Done_Animal", true);
            k.Say(a3Prevent, "Prevention helps, but the concern still needs appropriate assessment.");
            k.Call(a3Prevent, hub);

            // ---------------------------------------------------------------- A4 — Water and environmental safety
            k.Say(a4, "The report describes a change in water appearance after heavy rain. It deserves follow-up, but does not prove contamination or explain the animal illness.", EnvOfficer);
            k.Say(a4, "Tell us what practical steps to take while the concern is checked.", Water);
            k.Say(a4, "Place each card: DO NOW, VERIFY NEXT, or reject it as unsupported.", You);
            k.Call(a4, sort[0]);
            string[] sortCards =
            {
                "Share practical safe-water guidance",
                "Arrange appropriate water-quality follow-up",
                "Declare the water the confirmed cause",
            };
            int[] correct = { 0, 1, 2 }; // 0 = DO NOW, 1 = VERIFY NEXT, 2 = REJECT
            string[] fixes =
            {
                "Practical guidance can start now, while the concern is checked. Move it to DO NOW.",
                "Follow-up checks what is not yet known. Move it to VERIFY NEXT.",
                "The water has not been confirmed as the cause. Reject this card as unsupported.",
            };
            for (int i = 0; i < 3; i++)
            {
                var next = i < 2 ? sort[i + 1] : a4Plan;
                var right = k.Block("A4_Card_" + (i + 1) + "_Right");
                var wrong = k.Block("A4_Card_" + (i + 1) + "_Wrong");
                k.Say(sort[i], "CARD " + (i + 1) + " OF 3 — “" + sortCards[i] + "”");
                k.Plain(sort[i], "DO NOW", correct[i] == 0 ? right : wrong);
                k.Plain(sort[i], "VERIFY NEXT", correct[i] == 1 ? right : wrong);
                k.Plain(sort[i], "Reject (unsupported)", correct[i] == 2 ? right : wrong);
                k.Call(right, next);
                k.Say(wrong, fixes[i]);
                k.Call(wrong, sort[i]);
            }
            k.Say(a4Plan, "Choose the water and environment plan.", You);
            k.FailIfNone(a4Plan, failure);
            k.Option(a4Plan, "Guidance + follow-up", a4Full, D(-12, -1, 4, -2, -2));
            k.Option(a4Plan, "Guidance only", a4Guidance, D(-6, -1, 2, -1, -1));
            k.Apply(a4Full, "A4_Full", "Guidance + follow-up", D(-12, -1, 4, -2, -2));
            k.Flag(a4Full, "s05_water_plan", "full");
            k.Flag(a4Full, "s05_water_complete", "true");
            k.SetBool(a4Full, "Done_Water", true);
            k.Say(a4Full, "WATER AND ENVIRONMENT PLAN READY. Immediate precautions are paired with follow-up; the cause has not been assumed.");
            k.Badge(a4Full, "Precaution With Evidence");
            k.Say(a4Full, "BADGE — PRECAUTION WITH EVIDENCE.");
            k.Call(a4Full, hub);
            k.Apply(a4Guidance, "A4_Guidance", "Guidance only", D(-6, -1, 2, -1, -1));
            k.Flag(a4Guidance, "s05_water_plan", "guidance_only");
            k.Flag(a4Guidance, "s05_water_complete", "true");
            k.SetBool(a4Guidance, "Done_Water", true);
            k.Say(a4Guidance, "Practical advice has been shared, but follow-up remains a gap.");
            k.Call(a4Guidance, hub);

            // ---------------------------------------------------------------- A5 — Resource disruption (after all three workstreams)
            k.Music(a5, "Assets/Audio/M03_Tension.wav.mp3");
            k.Say(a5, "Coordinator, the team has fewer transport and staff hours available than expected. The plan cannot be delivered exactly as written.", Radio);
            k.Say(a5, "We must adapt without abandoning an essential workstream.", You);
            k.FailIfNone(a5, failure);
            k.Option(a5, "Combine visits", a5Combine, D(-4, -1, 1, 0, 1));
            k.Option(a5, "Pay for extra transport", a5Pay, D(-12, 0, 2, 0, -1));
            k.Option(a5, "Keep the original plan", a5Keep, D(0, -2, -2, 0, 0));
            Decision(k, a5Combine, "A5_Combine", "Combine visits", D(-4, -1, 1, 0, 1), "s05_resource_response", "combine", "The team shares transport and protects more capacity.", "RESOURCE PLAN UPDATED.", a6);
            Decision(k, a5Pay, "A5_Pay", "Pay for extra transport", D(-12, 0, 2, 0, -1), "s05_resource_response", "pay", "The schedule is protected, but money is reduced.", "RESOURCE PLAN UPDATED.", a6);
            Decision(k, a5Keep, "A5_Keep", "Keep the original plan", D(0, -2, -2, 0, 0), "s05_resource_response", "delay", "The delay leaves less time for verification.", "RESOURCE PLAN UPDATED.", a6);

            // ---------------------------------------------------------------- A6 — Negotiate priorities
            k.Music(a6, "Assets/Audio/M02_Decision.wav.mp3");
            k.Say(a6, "Please do not forget the animals and the people who depend on them.", Farmer);
            k.Say(a6, "Residents need prevention advice and honest updates.", CommunityRep);
            k.Say(a6, "The water concern needs follow-up, not assumptions.", EnvOfficer);
            k.SetInt(a6, "Tokens", K.Assign, 0);
            k.Call(a6, tokens);
            k.IfInt(tokens, "Tokens", K.Ge, 2);
            k.Call(tokens, a6Final);
            k.End(tokens);
            k.Say(tokens, "Assign a priority token to a stakeholder request ({$Tokens} of 2 assigned).", You);
            k.Plain(tokens, "Farmer: protect the animals and livelihoods", tFarmer, k.Var<BooleanVariable>("Token_Farmer"));
            k.Plain(tokens, "Community: prevention advice and honest updates", tCommunity, k.Var<BooleanVariable>("Token_Community"));
            k.Plain(tokens, "Environmental officer: water follow-up", tEnv, k.Var<BooleanVariable>("Token_Environment"));
            foreach (var t in new[] { new { b = tFarmer, key = "Token_Farmer", name = "farmer" }, new { b = tCommunity, key = "Token_Community", name = "community" }, new { b = tEnv, key = "Token_Environment", name = "environment" } })
            {
                k.SetBool(t.b, t.key, true);
                k.SetInt(t.b, "Tokens", K.Add, 1);
                k.Flag(t.b, "s05_priority_tokens", "{$s05_priority_tokens} " + t.name);
                k.Call(t.b, tokens);
            }
            var gaps2 = E.Add<ComposeGaps>(a6Final);
            E.Set(gaps2, "source", (int)ComposeGaps.Source.FinalResponseWorkstreams);
            k.Card(a6Final, "Gaps remaining", "{$GapList}");
            k.Say(a6Final, "Choose the final priority package, accounting for completed work and remaining resources.", You);
            k.FailIfNone(a6Final, failure);
            k.Option(a6Final, "Protect follow-up across all sectors", a6Protect, D(-8, -1, 5, -2, -1));
            k.Option(a6Final, "Prioritize the most incomplete workstream", a6Prioritize, D(-5, -1, 2, -1, 0));
            k.Option(a6Final, "Focus on one visible activity", a6Focus, D(-3, 0, -5, 1, 0));
            Decision(k, a6Protect, "A6_Protect", "Protect follow-up across all sectors", D(-8, -1, 5, -2, -1), "s05_final_priority", "all_sector", "Stakeholders agree on next steps and responsibilities.", "PRIORITIES CONFIRMED.", a7);
            Decision(k, a6Prioritize, "A6_Prioritize", "Prioritize most incomplete workstream", D(-5, -1, 2, -1, 0), "s05_final_priority", "most_incomplete", "The largest gap receives attention, but other follow-up remains limited.", "PRIORITIES CONFIRMED.", a7);
            Decision(k, a6Focus, "A6_Focus", "Focus on one visible activity", D(-3, 0, -5, 1, 0), "s05_final_priority", "one_activity", "One visible task is completed, but coordination and trust weaken.", "PRIORITIES CONFIRMED.", a7);

            // ---------------------------------------------------------------- A7 — Verify results and brief the public
            k.Say(a7, "Check what was completed and what remains uncertain.", Amina);
            k.Say(a7, "Report observations separately from unconfirmed explanations.", EnvOfficer);
            k.Say(a7, "Make clear that appropriate treatment remains available when clinically indicated.", AnimalWorker);
            k.Say(a7, "Correct unsupported claims and deliver the briefing.", You);
            k.Call(a7, verify[0]);
            string[] statusText =
            {
                "The community plan was delivered.",
                "The animal-health workstream was completed.",
                "Water and environmental follow-up was completed.",
                "One confirmed source caused all the reports.",
            };
            int[] statusAnswer = { 0, 0, 1, 2 }; // 0 COMPLETED, 1 FOLLOW-UP REQUIRED, 2 NOT ESTABLISHED
            string[] statusFix =
            {
                "The community plan was delivered in this response. Mark it COMPLETED.",
                "The animal-health workstream was completed in this response. Mark it COMPLETED.",
                "Follow-up has been arranged, but it has not been completed yet. Mark it FOLLOW-UP REQUIRED.",
                "No single source has been confirmed. Mark it NOT ESTABLISHED.",
            };
            for (int i = 0; i < 4; i++)
            {
                var next = i < 3 ? verify[i + 1] : a7Verified;
                var right = k.Block("A7_Status_" + (i + 1) + "_Right");
                var wrong = k.Block("A7_Status_" + (i + 1) + "_Wrong");
                k.Say(verify[i], "STATEMENT " + (i + 1) + " OF 4 — “" + statusText[i] + "”");
                k.Plain(verify[i], "COMPLETED", statusAnswer[i] == 0 ? right : wrong);
                k.Plain(verify[i], "FOLLOW-UP REQUIRED", statusAnswer[i] == 1 ? right : wrong);
                k.Plain(verify[i], "NOT ESTABLISHED", statusAnswer[i] == 2 ? right : wrong);
                k.Call(right, next);
                k.Say(wrong, statusFix[i]);
                k.Call(wrong, verify[i]);
            }
            k.Flag(a7Verified, "s05_report_verified", "true");
            k.Say(a7Verified, "FINAL REPORT VERIFIED. Findings, actions and remaining limits are clearly separated.");
            k.Call(a7Verified, a7Brief);
            k.Say(a7Brief, "Choose the tone of the public briefing.", You);
            k.FailIfNone(a7Brief, failure);
            k.Option(a7Brief, "Transparent briefing", a7Transparent, D(-2, -1, 5, -1, 0));
            k.Option(a7Brief, "Overconfident briefing", a7Overconfident, D(0, 0, -8, 0, 0), k.Var<BooleanVariable>("OverconfidentTried"));
            k.Apply(a7Overconfident, "A7_Overconfident", "Overconfident briefing", D(0, 0, -8, 0, 0));
            k.SetBool(a7Overconfident, "OverconfidentTried", true);
            k.Say(a7Overconfident, "This briefing presents unconfirmed claims as fact, so it cannot be delivered. Revise it.");
            k.Call(a7Overconfident, a7Brief);
            k.Apply(a7Transparent, "A7_Transparent", "Transparent briefing", D(-2, -1, 5, -1, 0));
            k.Flag(a7Transparent, "s05_briefing", "transparent");
            k.Say(a7Transparent, "The community receives practical next steps and an honest account of the evidence.");
            k.Badge(a7Transparent, "Kivu Coordinator");
            k.Say(a7Transparent, "BADGE — KIVU COORDINATOR.");
            k.Call(a7Transparent, a8);

            // ---------------------------------------------------------------- A8 — Close the response
            k.Music(a8, "Assets/Audio/M04_Reflection.wav.mp3");
            k.Say(a8, "We completed the response plan. We know what actions were delivered, what the evidence supports, and how the teams will continue coordinating.", You);
            k.Say(a8, "Choose a final coordination plan. Each plan assigns a responsible group, a follow-up action and a way to share updates.", You);
            k.FailIfNone(a8, failure);
            k.Option(a8, "Shared coordination plan", a8Shared, D(-3, -1, 4, -1, -1));
            k.Option(a8, "Basic handover plan", a8Basic, D(-1, -1, 1, 0, 0));
            Decision(k, a8Shared, "A8_Shared", "Shared coordination plan", D(-3, -1, 4, -1, -1), "s05_final_plan", "shared", "SUSTAINABLE RESPONSE ESTABLISHED.", null, ending);
            Decision(k, a8Basic, "A8_Basic", "Basic handover plan", D(-1, -1, 1, 0, 0), "s05_final_plan", "basic", "RESPONSE COMPLETED. The plan is feasible, though coordination is more limited.", null, ending);

            // ---------------------------------------------------------------- Ending, debrief, GAME COMPLETE (sections 4-5)
            k.Say(ending, "The community knows the practical steps and where to receive updates.", Amina);
            k.Say(ending, "Animal health and appropriate treatment remain part of the response.", AnimalWorker);
            k.Say(ending, "Environmental follow-up has a named responsibility and a clear record.", EnvOfficer);
            k.Say(ending, "One Health works when people act on evidence, adapt to constraints and coordinate across connected systems.", Narrator);
            // One Health Champion: all workstreams, verified report, transparent briefing, shared plan,
            // Trust >= 50, Capacity >= 5, Resistance Pressure at the end <= at entry.
            k.SetString(ending, "Ending", "completed");
            k.IfString(ending, "s05_final_plan", "shared");
            k.IfInt(ending, "Trust", K.Ge, 50);
            k.IfInt(ending, "Capacity", K.Ge, 5);
            k.IfIntVar(ending, "Resistance", K.Le, "Entry_Resistance");
            k.SetString(ending, "Ending", "champion");
            k.End(ending); k.End(ending); k.End(ending); k.End(ending);
            k.Flag(ending, "s05_ending", "{$Ending}");
            k.IfString(ending, "Ending", "champion");
            k.Badge(ending, "One Health Champion");
            k.Say(ending, "ONE HEALTH CHAMPION.");
            k.SetString(ending, "EndingText", "ONE HEALTH CHAMPION. GAME COMPLETE — Better outcomes require evidence, appropriate care, prevention, trust and collaboration across human, animal and environmental health.");
            k.Else(ending);
            k.Say(ending, "RESPONSE COMPLETED.");
            k.SetString(ending, "EndingText", "RESPONSE COMPLETED. GAME COMPLETE — Better outcomes require evidence, appropriate care, prevention, trust and collaboration across human, animal and environmental health.");
            k.End(ending);
            k.Card(ending, "Debrief — Evidence", "You learned to separate observations from explanations. A possible connection is not proof of a common cause.");
            k.Card(ending, "Debrief — Stewardship", "Antimicrobials are essential when clinically indicated. Stewardship means appropriate access and use, not withholding needed treatment.");
            k.Card(ending, "Debrief — One Health", "Community prevention, animal-health assessment and environmental follow-up can support one another. No single sector can manage every risk alone.");
            k.Card(ending, "Debrief — Trade-offs", "Money, Trust, Resistance Pressure, Time and Capacity represent different pressures. Improving one part of a response may create costs elsewhere.");
            k.Card(ending, "Debrief — Adaptation", "When conditions changed, you revised the plan while protecting essential actions.");
            k.Card(ending, "One Health", "One Health is a way of working: use evidence, respect people, protect health, and coordinate across human, animal and environmental systems.");
            k.Knowledge(ending, "One Health is a way of working");
            k.Say(ending, "GAME COMPLETE — Better outcomes require evidence, appropriate care, prevention, trust and collaboration across human, animal and environmental health.");
            var complete = E.Add<CompleteScenario>(ending);
            E.Set(complete, "scenarioId", "S04");
            E.Set(complete, "scenarioTitle", Title);
            E.Set(complete, "nextScene", "");
            E.Set(complete, "nextLabel", "");
            E.Set(complete, "summary", "{$EndingText}");

            // ---------------------------------------------------------------- Failure
            k.Say(failure, "RESPONSE BLOCKED. Your team cannot complete a required action with the resources remaining.");
            var fail = E.Add<FailScenario>(failure);
            E.Set(fail, "scenarioTitle", Title);
            E.Set(fail, "reason", "RESPONSE BLOCKED. Your team cannot complete a required action with the resources remaining. Review your decisions and try again.");

            k.Var<StringVariable>("s05_priority_tokens");
            k.Save();
            K.AddToBuild(ScenePath);
            return E.DumpAll(k.fc);
        }

        static void Pick(K k, Block b, string name, Block workstream)
        {
            k.IfString(b, "s05_workstream_order", "");
            k.Say(b, "Response order saved. All three workstreams remain required.");
            k.End(b);
            k.Flag(b, "s05_workstream_order", "{$s05_workstream_order} " + name);
            k.Call(b, workstream);
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
    }
}

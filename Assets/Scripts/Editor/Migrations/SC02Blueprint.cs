using System.Collections.Generic;
using System.Linq;
using Fungus;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// Rebuilds the SC02 flowchart to William's "Scenario 2 — Complete Production Blueprint":
    /// Evidence / Follow-up / Cross-sector states, capacity costs and failure, case board costs,
    /// test result beats, connection board, coordination, callback, capacity crisis, final
    /// allocation (two actions), outcome logic, decision review, reflection and final story beat.
    /// Reuses the existing dialogue blocks (E01–E05), which already match the blueprint text.
    /// </summary>
    public static class SC02Blueprint
    {
        const string Title = "Scenario 2 — The Fever That Won’t Wait";

        // SetOperator / CompareOperator indices (Fungus enums)
        const int Assign = 0, Add = 2, Sub = 3;
        const int Eq = 0, Lt = 2, Le = 4, Ge = 5;

        static Flowchart fc;
        static Say tplSay;
        static MenuDialog menuDialog;
        static Character amina, malik;
        static IntegerVariable cap, evidence, followUp, cross, picks;
        static StringVariable outcome;

        public static string Run()
        {
            fc = Object.FindAnyObjectByType<Flowchart>();
            amina = Char("Amina");
            malik = Char("Dr. Malik");
            tplSay = (Say)E.B(fc, "E01_Return_to_Kivu").CommandList.First(c => c is Say);
            menuDialog = Object.FindAnyObjectByType<MenuDialog>(FindObjectsInactive.Include);

            // ------------------------------------------------------------ variables (blueprint §3)
            foreach (var k in new[] { "FinalChoice", "Unlocked_CommunityEngagement" }) E.RemoveVar(fc, k);
            cap = (IntegerVariable)fc.GetVariable("ClinicResponseCapacity");
            evidence = E.Var<IntegerVariable>(fc, "Evidence");
            followUp = E.Var<IntegerVariable>(fc, "FollowUp");
            cross = E.Var<IntegerVariable>(fc, "CrossSector");
            picks = E.Var<IntegerVariable>(fc, "FinalPicks");
            outcome = E.Var<StringVariable>(fc, "Outcome");
            E.Var<StringVariable>(fc, "Path");
            foreach (var k in new[] { "EvidenceLevel", "FollowUpLevel", "CrossSectorLevel", "OutcomeText" }) E.Var<StringVariable>(fc, k);
            var unlockedCC = E.Var<BooleanVariable>(fc, "Unlocked_CommunityCoordination");

            // ------------------------------------------------------------ remove superseded blocks
            foreach (var name in new[] { "D04_D_Community", "E06_Outcome" })
            {
                var b = fc.FindBlock(name);
                if (b != null) E.DeleteBlock(b);
            }

            var start = E.B(fc, "S02_Start");
            var e01 = E.B(fc, "E01_Return_to_Kivu");
            var e02 = E.B(fc, "E02_The_Case");
            var e03 = E.B(fc, "E03_Evidence_Check");
            var e04 = E.B(fc, "E04_Caregiver");
            var e05 = E.B(fc, "E05_Signal");

            // New blocks
            var title = Blk("E00_Scenario_Title", -600, 0);
            var failure = Blk("FAILURE", 1400, 1600);
            var cardIll = Blk("CARD_RecentIllness", 900, 200);
            var cardMed = Blk("CARD_RecentMedicine", 900, 280);
            var cardCtx = Blk("CARD_HomeContext", 900, 360);
            var e06 = Blk("E06_Callback", 0, 1500);
            var e07 = Blk("E07_Capacity_Crisis", 0, 1600);
            var d07 = Blk("D07_Final_Allocation", 0, 1700);
            var d07a = Blk("D07_A_FollowUp", -300, 1800);
            var d07b = Blk("D07_B_Community", -100, 1800);
            var d07c = Blk("D07_C_Investigate", 100, 1800);
            var d07d = Blk("D07_D_Preserve", 300, 1800);
            var d07next = Blk("D07_Next", 0, 1900);
            var outcomeB = Blk("OUTCOME", 0, 2000);
            var review = Blk("REVIEW", 0, 2100);
            var reflect = Blk("REFLECTION", 0, 2200);
            var reflect2 = Blk("REFLECTION_2", 0, 2300);
            var ending = Blk("ENDING", 0, 2400);

            // ------------------------------------------------------------ S02_START
            var begin = start.CommandList.OfType<BeginScenario>().First();
            E.Set(begin, "showTime", false);                          // blueprint HUD: Money, Trust, Resistance Pressure, Capacity
            var startCall = start.CommandList.OfType<Call>().Last();
            E.Set(startCall, "targetBlock", title);

            // ------------------------------------------------------------ E00: title + time card + carried capability
            var card = E.Add<ShowVisualCard>(title);
            E.Set(card, "title", Title);
            E.Set(card, "caption", "3 DAYS LATER\n\nYou return to the Kivu Community Clinic. Your decisions in Scenario 1 come with you.");
            var ifCC = E.Add<If>(title); E.SetBoolCondition(ifCC, unlockedCC, true);
            Say(title, "COMMUNITY COORDINATION — UNLOCKED. You earned this capability in Scenario 1 by involving the community.");
            E.Add<End>(title);
            E.Call(title, -1, e01);

            // ------------------------------------------------------------ E01: blueprint lines only (remove earlier personalised opener)
            var e01If = e01.CommandList.FirstOrDefault(c => c is If);
            if (e01If != null)
            {
                int from = e01.CommandList.IndexOf(e01If);
                int to = e01.CommandList.FindIndex(from, c => c is End);
                foreach (var c in e01.CommandList.GetRange(from, to - from + 1).ToList()) E.Remove(c);
            }

            // ------------------------------------------------------------ E02: objective
            Say(e02, "OBJECTIVE — HELP THE TEAM RESPOND RESPONSIBLY", e02.CommandList.Count - 1);

            // ------------------------------------------------------------ D01
            var d01 = E.B(fc, "D01_Prioritize_the_Response");
            var d01a = Reset("D01_A_Treatment"); var d01b = Reset("D01_B_Assessment"); var d01c = Reset("D01_C_Background");
            var caseBoard = E.B(fc, "CASE_BOARD");
            Menus(d01,
                "Move directly toward treatment  ·  5 capacity", d01a,
                "Support a focused assessment  ·  8 capacity", d01b,
                "Gather wider background first  ·  7 capacity", d01c);
            Choice(d01a, 5, "Moved directly toward treatment", "You reduced waiting pressure, but uncertainty remains.", caseBoard, ev: -1);
            Choice(d01b, 8, "Supported a focused assessment", "The team has a clearer picture before deciding what happens next.", caseBoard, ev: 1);
            Choice(d01c, 7, "Gathered wider background first", "The wider history may help, but the immediate situation still needs attention.", caseBoard, ev: 1);

            // ------------------------------------------------------------ CASE BOARD: 5 capacity per card, optional
            Card(cardIll, "Card_RecentIllness");
            Card(cardMed, "Card_RecentMedicine");
            Card(cardCtx, "Card_HomeContext");
            WireCaseBoard(cardIll, cardMed, cardCtx);

            // ------------------------------------------------------------ E03
            foreach (var rk in e03.CommandList.OfType<RecordKnowledge>()) E.Set(rk, "knowledgeId", "Uncertainty");

            // ------------------------------------------------------------ D02 + TEST_RESULT
            var d02 = E.B(fc, "D02_Reduce_Uncertainty");
            var d02a = Reset("D02_A_Evidence"); var d02b = Reset("D02_B_Test"); var d02c = Reset("D02_C_Current");
            var test = Reset("TEST_RESULT");
            var d03 = E.B(fc, "D03_Response");
            Menus(d02,
                "Gather additional clinical evidence  ·  10 capacity", d02a,
                "Use available diagnostic pathway  ·  20 capacity", d02b,
                "Proceed with current information  ·  no capacity", d02c);
            Choice(d02a, 10, "Gathered additional clinical evidence", "More information gives the team a stronger basis for the next step.", d03, ev: 1);
            Choice(d02b, 20, "Used the diagnostic pathway", "You spent more capacity, but gained a more specific piece of evidence.", test);
            Choice(d02c, 0, "Proceeded with current information", "You preserved capacity, but uncertainty remains.", d03);

            // Test result beats (blueprint): requested -> processing -> result available -> evidence -> what changed.
            // Exact test, result wording and clinical implications await Dr Kakra's review; wording stays non-clinical.
            Say(test, "TEST REQUESTED");
            Say(test, "Processing the test…");
            Say(test, "RESULT AVAILABLE — The result gives the team a more specific piece of evidence about the illness.");
            Int(test, evidence, Add, 2);
            Say(test, "WHAT CHANGED? The team can now choose a response based on evidence rather than assumptions.");
            E.Call(test, -1, d03);

            // ------------------------------------------------------------ D03
            var d03a = Reset("D03_A_Support"); var d03b = Reset("D03_B_AMR"); var d03c = Reset("D03_C_Ref");
            Menus(d03,
                "Supportive care + monitoring where clinically appropriate  ·  10 capacity", d03a,
                "Clinician-guided antimicrobial treatment when indicated  ·  15 capacity", d03b,
                "Escalate / refer for additional clinical support  ·  25 capacity", d03c);
            Choice(d03a, 10, "Chose supportive care and monitoring", "You preserved resources while the team continued appropriate monitoring.", e04, fu: 1, knowledge: "Responsible antimicrobial use");
            // Resistance Pressure changes only under clinically reviewed logic (blueprint): none applied.
            Choice(d03b, 15, "Chose clinician-guided antimicrobial treatment", "The team used antimicrobial treatment when the clinical assessment supported it.", e04, knowledge: "Responsible antimicrobial use");
            Choice(d03c, 25, "Escalated for additional clinical support", "Additional support reduced uncertainty, but used more time and capacity.", e04, ev: 1, knowledge: "Responsible antimicrobial use");

            // ------------------------------------------------------------ D04
            var d04 = E.B(fc, "D04_Followup");
            foreach (var m in d04.CommandList.OfType<Fungus.Menu>().Skip(3).ToList()) E.Remove(m);
            var d04a = Reset("D04_A_Plan"); var d04b = Reset("D04_B_Quick"); var d04c = Reset("D04_C_Support");
            Menus(d04,
                "Explain the plan and arrange follow-up  ·  10 capacity", d04a,
                "Give a quick explanation and end the visit  ·  2 capacity", d04b,
                "Arrange additional support because follow-up may be difficult  ·  15 capacity", d04c);
            Choice(d04a, 10, "Explained the plan and arranged follow-up", "The plan is clear, and the team has a way to check what happens next.", e05, fu: 2, trust: 6, knowledge: "Follow-up");
            Choice(d04b, 2, "Gave a quick explanation and ended the visit", "You saved capacity, but less information will be available later.", e05, fu: -1, trust: -5, knowledge: "Follow-up");
            Choice(d04c, 15, "Arranged additional follow-up support", "Extra capacity makes follow-up more reliable.", e05, fu: 2, trust: 3, knowledge: "Follow-up");

            // ------------------------------------------------------------ Connection board + D05
            var board = Reset("CONNECTION_BOARD");
            var d05p = Reset("D05_People"); var d05a = Reset("D05_Animals"); var d05e = Reset("D05_Env");
            var d06 = Reset("D06_Coordination");
            Say(board, "KIVU CONNECTION BOARD — Where do you investigate? Each choice uses 10 capacity.");
            Menus(board,
                "People — human health", d05p,
                "Animals — animal health", d05a,
                "Environment — environmental pathways", d05e);
            Choice(d05p, 10, "Investigated human health",
                "PEOPLE — Human-health teams are monitoring similar signals in the community.", d06, knowledge: "One Health connections",
                extra: "The clinic now has another piece of the human-health picture.");
            Choice(d05a, 10, "Investigated animal health",
                "ANIMALS — The animal-health contact shares information about antimicrobial use and resistance monitoring in animal production.", d06, knowledge: "One Health connections",
                extra: "The animal-health team adds information the clinic could not see alone.");
            Choice(d05e, 10, "Investigated environmental pathways",
                "ENVIRONMENT — Information about shared water and waste pathways adds another part of the system.", d06, knowledge: "One Health connections",
                extra: "The environmental information changes how the team sees the wider situation.");

            // ------------------------------------------------------------ D06
            var d06a = Reset("D06_A_Coordinate"); var d06b = Reset("D06_B_Clinic"); var d06c = Reset("D06_C_Patient");
            Say(d06, "COORDINATE OR STAY NARROW?");
            Menus(d06,
                "Coordinate across sectors  ·  20 capacity", d06a,
                "Keep the response inside the clinic  ·  10 capacity", d06b,
                "Focus only on the original patient  ·  5 capacity", d06c);
            Choice(d06a, 20, "Coordinated across sectors", "The response is broader, but it uses more capacity.", e06, cr: 2, trust: 5, res: -3);
            Choice(d06b, 10, "Kept the response inside the clinic", "The clinic can act quickly, but the wider signal remains less understood.", e06, cr: 1);
            Choice(d06c, 5, "Focused only on the original patient", "The immediate case receives attention, but the wider signal remains unresolved.", e06);

            // ------------------------------------------------------------ E06: consequence callback
            var i1 = E.Add<If>(e06); IntCond(i1, evidence, Ge, 4);
            Say(e06, "The information you gathered earlier helps the team interpret the new signal.");
            var i2 = E.Add<ElseIf>(e06); IntCond(i2, evidence, Ge, 2);
            Say(e06, "The team has useful information, but some uncertainty remains.");
            E.Add<Else>(e06);
            Say(e06, "Less information was gathered earlier, so fewer pieces can be connected now.");
            E.Add<End>(e06);
            var i3 = E.Add<If>(e06); IntCond(i3, followUp, Ge, 3);
            Say(e06, "The follow-up plan gives the team another source of information.");
            var i4 = E.Add<ElseIf>(e06); IntCond(i4, followUp, Le, 0);
            Say(e06, "Limited follow-up leaves the team with less visibility.");
            E.Add<End>(e06);
            E.Call(e06, -1, e07);

            // ------------------------------------------------------------ E07: capacity crisis
            Music(e07, "Assets/Audio/M03_Tension.wav.mp3");
            Say(e07, "RESPONSE CAPACITY: LOW — {$ClinicResponseCapacity} capacity left.");
            Say(e07, "Three demands need attention: continue supporting the original case, respond to the community signal, and maintain communication with the community.");
            Say(e07, "You have enough capacity for TWO final actions.");
            E.Call(e07, -1, d07);

            // ------------------------------------------------------------ D07: choose exactly two
            var canA = E.Var<BooleanVariable>(fc, "CanAfford15");
            var canC = E.Var<BooleanVariable>(fc, "CanAfford20");
            var doneA = E.Var<BooleanVariable>(fc, "D07A_Done");
            var doneB = E.Var<BooleanVariable>(fc, "D07B_Done");
            var doneC = E.Var<BooleanVariable>(fc, "D07C_Done");
            var doneD = E.Var<BooleanVariable>(fc, "D07D_Done");
            var a1 = E.Add<If>(d07); IntCond(a1, cap, Ge, 15); Bool(d07, canA, true); E.Add<Else>(d07); Bool(d07, canA, false); E.Add<End>(d07);
            var a2 = E.Add<If>(d07); IntCond(a2, cap, Ge, 20); Bool(d07, canC, true); E.Add<Else>(d07); Bool(d07, canC, false); E.Add<End>(d07);
            var p0 = E.Add<If>(d07); IntCond(p0, picks, Eq, 0);
            Say(d07, "FINAL ALLOCATION — Choose your FIRST of two final actions.");
            E.Add<Else>(d07);
            Say(d07, "FINAL ALLOCATION — Choose your SECOND final action.");
            E.Add<End>(d07);
            FinalMenu(d07, "Strengthen patient follow-up  ·  15 capacity", d07a, canA, doneA);
            FinalMenu(d07, "Coordinate community response  ·  15 capacity", d07b, canA, doneB);
            FinalMenu(d07, "Investigate the wider AMR signal  ·  20 capacity", d07c, canC, doneC);
            FinalMenu(d07, "Preserve resources for future cases  ·  no capacity", d07d, null, doneD);

            Final(d07a, doneA, 15, "Strengthened patient follow-up", "Patient follow-up is stronger, and the family feels supported.", d07next, fu: 2, trust: 3);
            Final(d07b, doneB, 15, "Coordinated the community response", "The community response is better coordinated.", d07next, trust: 4, cr: 1);
            Final(d07c, doneC, 20, "Investigated the wider AMR signal", "The team learns more about the wider resistance signal.", d07next, ev: 1, cr: 1);
            Final(d07d, doneD, 0, "Preserved resources for future cases", "Capacity is kept for future cases, but nothing improves right now.", d07next);

            var pn = E.Add<If>(d07next); IntCond(pn, picks, Ge, 2);
            E.Call(d07next, -1, outcomeB);
            E.Add<End>(d07next);
            E.Call(d07next, -1, d07);

            // ------------------------------------------------------------ OUTCOME (blueprint §6)
            Music(outcomeB, "Assets/Audio/M04_Reflection.wav.mp3");
            Level(outcomeB, evidence, "EvidenceLevel", new[] { 4, 2 }, new[] { "High", "Medium", "Low" });
            Level(outcomeB, followUp, "FollowUpLevel", new[] { 3, 1 }, new[] { "Strong", "Adequate", "Weak" });
            Level(outcomeB, cross, "CrossSectorLevel", new[] { 2, 1 }, new[] { "Strong", "Partial", "None" });
            Str(outcomeB, outcome, "RECOVERY");
            var s1 = E.Add<If>(outcomeB); IntCond(s1, evidence, Ge, 2);
            var s2 = E.Add<If>(outcomeB); IntCond(s2, followUp, Ge, 1);
            var s3 = E.Add<If>(outcomeB); IntCond(s3, cross, Ge, 1);
            Str(outcomeB, outcome, "SUCCESS");
            E.Add<End>(outcomeB); E.Add<End>(outcomeB); E.Add<End>(outcomeB);
            var so = E.Add<If>(outcomeB); E.SetStringCondition(so, outcome, "SUCCESS");
            Str(outcomeB, (StringVariable)fc.GetVariable("OutcomeText"), "You built a response that could adapt when new information appeared.");
            E.Add<Else>(outcomeB);
            Str(outcomeB, (StringVariable)fc.GetVariable("OutcomeText"), "Some earlier decisions created pressure later. You adapted before the response broke down.");
            E.Add<End>(outcomeB);
            Say(outcomeB, "OUTCOME — {$Outcome}");
            Say(outcomeB, "{$OutcomeText}");
            E.Call(outcomeB, -1, review);

            // ------------------------------------------------------------ FAILURE: capacity exhausted -> retry
            var panel = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "CaseBoardPanel" && g.scene.IsValid());
            var hide = E.Add<SetActive>(failure);
            E.Set(hide, "_targetGameObject.gameObjectVal", panel);
            E.Set(hide, "activeState.booleanVal", false);
            Say(failure, "RESPONSE STOPPED — The team no longer has enough capacity to continue the response as planned.");
            var fail = E.Add<FailScenario>(failure);
            E.Set(fail, "scenarioTitle", Title);
            E.Set(fail, "reason", "The team no longer has enough capacity to continue the response as planned. Earlier choices used more capacity than the response could afford. Try gathering targeted information first, and keep capacity for follow-up.");

            // ------------------------------------------------------------ REVIEW (blueprint §7)
            var pathCard = E.Add<ShowVisualCard>(review);
            E.Set(pathCard, "title", "Decision Review — Your Path");
            E.Set(pathCard, "caption", "{$Path}");
            Say(review, "WHAT CHANGED — Evidence: {$EvidenceLevel}  ·  Follow-up: {$FollowUpLevel}  ·  Cross-sector coordination: {$CrossSectorLevel}  ·  Capacity left: {$ClinicResponseCapacity}");
            var w = E.Add<If>(review); E.SetStringCondition(w, outcome, "SUCCESS");
            Say(review, "WHY IT MATTERED — Gathering evidence, planning follow-up and looking beyond the clinic gave the team options when the situation changed.");
            E.Add<Else>(review);
            Say(review, "WHY IT MATTERED — Gaps left by earlier choices created pressure later, but your later decisions kept the response going.");
            E.Add<End>(review);
            var t1 = E.Add<If>(review); IntCond(t1, evidence, Lt, 2);
            Say(review, "TRY DIFFERENTLY — Gather targeted information before acting.");
            var t2 = E.Add<ElseIf>(review); IntCond(t2, followUp, Lt, 1);
            Say(review, "TRY DIFFERENTLY — Build a follow-up plan the family can realistically manage.");
            var t3 = E.Add<ElseIf>(review); IntCond(t3, cross, Lt, 1);
            Say(review, "TRY DIFFERENTLY — Investigate and coordinate beyond the clinic.");
            E.Add<Else>(review);
            Say(review, "TRY DIFFERENTLY — Replay and test another strategy to see how the consequences change.");
            E.Add<End>(review);
            E.Call(review, -1, reflect);

            // ------------------------------------------------------------ REFLECTION (blueprint §8)
            Say(reflect, "REFLECTION — Which decision created the biggest later consequence? Think back over your path.");
            Say(reflect, "What information would you have wanted earlier?");
            Menus(reflect,
                "Clinical information", reflect2,
                "Medicine history", reflect2,
                "Community context", reflect2,
                "Animal and environment information", reflect2,
                "More diagnostic capacity", reflect2);
            Say(reflect2, "What would you change? Choose Replay Scenario at the end to try a different path.");
            E.Call(reflect2, -1, ending);

            // ------------------------------------------------------------ ENDING (blueprint §9)
            Say(ending, "Today showed us something important.", amina);
            Say(ending, "We started with one patient.", amina);
            Say(ending, "But the situation was bigger than one patient.", amina);
            Say(ending, "And we still did not have every answer.", malik);
            Say(ending, "That is why we kept looking, learning and adapting.", amina);
            var rk2 = E.Add<RecordKnowledge>(ending); E.Set(rk2, "knowledgeId", "Adaptation");
            var un = E.Add<UnlockCapability>(ending); E.Set(un, "capabilityId", "Cross-Sector Response");
            Say(ending, "NEW CAPABILITY UNLOCKED — CROSS-SECTOR RESPONSE. Coordinate information and actions across human, animal and environmental health settings.");
            var complete = E.Add<CompleteScenario>(ending);
            E.Set(complete, "scenarioId", "S02");
            E.Set(complete, "scenarioTitle", Title);
            E.Set(complete, "nextScene", GameSession.MainMenuScene);
            E.Set(complete, "nextLabel", "Continue Journey");
            E.Set(complete, "summary", "OUTCOME: {$Outcome}. {$OutcomeText}");

            E.Finish(fc);
            return E.DumpAll(fc);
        }

        // ================================================================== helpers

        static Block Blk(string name, float x, float y)
        {
            var b = fc.FindBlock(name);
            if (b != null) { foreach (var c in b.CommandList.ToList()) E.Remove(c); return b; }
            return E.NewBlock(fc, name, new Vector2(x, y));
        }

        static Block Reset(string name)
        {
            var b = E.B(fc, name);
            foreach (var c in b.CommandList.ToList()) E.Remove(c);
            return b;
        }

        static Say Say(Block b, string text, Character who = null) { return E.Say(b, -1, text, who, tplSay); }
        static Say Say(Block b, string text, int index) { return E.Say(b, index, text, null, tplSay); }

        static void Menus(Block b, params object[] pairs)
        {
            foreach (var m in b.CommandList.OfType<Fungus.Menu>().ToList()) E.Remove(m);
            for (int i = 0; i < pairs.Length; i += 2)
                Option(b, (string)pairs[i], (Block)pairs[i + 1]);
        }

        static void FinalMenu(Block b, string text, Block target, Variable canAfford, Variable done)
        {
            var m = Option(b, text, target);
            if (canAfford != null) E.Set(m, "interactable.booleanRef", canAfford);
            E.Set(m, "hideThisOption.booleanRef", done);
        }

        static Fungus.Menu Option(Block b, string text, Block target)
        {
            var m = E.Menu(b, -1, text, target, null);
            E.Set(m, "setMenuDialog", menuDialog);
            return m;
        }

        static void Int(Block b, Variable v, int op, int value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", v);
            E.Set(s, "setOperator", op);
            E.Set(s, "anyVar.data.integerData.integerVal", value);
        }

        static void Bool(Block b, Variable v, bool value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", v);
            E.Set(s, "anyVar.data.booleanData.booleanVal", value);
        }

        static void Str(Block b, Variable v, string value)
        {
            var s = E.Add<SetVariable>(b);
            E.Set(s, "anyVar.variable", v);
            E.Set(s, "anyVar.data.stringData.stringVal", value);
        }

        static void IntCond(VariableCondition c, Variable v, int op, int value)
        {
            E.EnsureCondition(c);
            E.Set(c, "conditions.Array.data[0].anyVar.variable", v);
            E.Set(c, "conditions.Array.data[0].compareOperator", op);
            E.Set(c, "conditions.Array.data[0].anyVar.data.integerData.integerVal", value);
        }

        /// <summary>Spend capacity; at 0 the response cannot continue (blueprint FAILURE).</summary>
        static void Spend(Block b, int cost)
        {
            if (cost <= 0) return;
            Int(b, cap, Sub, cost);
            var i = E.Add<If>(b); IntCond(i, cap, Le, 0);
            Int(b, cap, Assign, 0);
            E.Call(b, -1, E.B(fc, "FAILURE"));
            E.Add<End>(b);
        }

        static void Choice(Block b, int cost, string step, string feedback, Block next,
            int ev = 0, int fu = 0, int cr = 0, int trust = 0, int res = 0, string knowledge = null, string extra = null)
        {
            Spend(b, cost);
            if (ev != 0) Int(b, evidence, Add, ev);
            if (fu != 0) Int(b, followUp, Add, fu);
            if (cr != 0) Int(b, cross, Add, cr);
            if (trust != 0 || res != 0)
            {
                var d = E.Add<ApplyDecision>(b);
                E.Set(d, "decisionId", b.BlockName);
                E.Set(d, "actionName", step);
                E.Set(d, "trust", trust);
                E.Set(d, "resistance", res);
            }
            var p = E.Add<RecordPathStep>(b); E.Set(p, "step", step);
            Say(b, feedback);
            if (extra != null) Say(b, extra);
            if (knowledge != null) { var k = E.Add<RecordKnowledge>(b); E.Set(k, "knowledgeId", knowledge); }
            E.Call(b, -1, next);
        }

        static void Final(Block b, Variable done, int cost, string step, string feedback, Block next,
            int ev = 0, int fu = 0, int cr = 0, int trust = 0)
        {
            Bool(b, done, true);
            Int(b, picks, Add, 1);
            Choice(b, cost, step, feedback, next, ev: ev, fu: fu, cr: cr, trust: trust);
        }

        static void Level(Block b, Variable v, string key, int[] limits, string[] names)
        {
            var target = (StringVariable)fc.GetVariable(key);
            var i = E.Add<If>(b); IntCond(i, v, Ge, limits[0]);
            Str(b, target, names[0]);
            var ei = E.Add<ElseIf>(b); IntCond(ei, v, Ge, limits[1]);
            Str(b, target, names[1]);
            E.Add<Else>(b);
            Str(b, target, names[2]);
            E.Add<End>(b);
        }

        static void Music(Block b, string path)
        {
            var pm = E.Add<PlayMusic>(b);
            E.Set(pm, "musicClip", AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            E.Set(pm, "loop", true);
            E.Set(pm, "fadeDuration", 1f);
        }

        /// <summary>Card block: charges 5 capacity and adds evidence once; reopening is free.</summary>
        static void Card(Block b, string flag)
        {
            var opened = E.Var<BooleanVariable>(fc, flag);
            var i = E.Add<If>(b); E.SetBoolCondition(i, opened, true);
            E.Add<Stop>(b);
            E.Add<End>(b);
            Bool(b, opened, true);
            Spend(b, 5);
            Int(b, evidence, Add, 1);
            var p = E.Add<RecordPathStep>(b);
            E.Set(p, "step", "Reviewed " + flag.Replace("Card_", "").Replace("RecentIllness", "recent illness").Replace("RecentMedicine", "recent medicine use").Replace("HomeContext", "home and community context"));
        }

        /// <summary>Card clicks run the card blocks; card text follows the blueprint's reveal column.</summary>
        static void WireCaseBoard(Block ill, Block med, Block ctx)
        {
            var panel = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "CaseBoardPanel" && g.scene.IsValid()).transform;
            var exec = typeof(Flowchart).GetMethod("ExecuteBlock", new[] { typeof(string) });
            var action = (UnityAction<string>)System.Delegate.CreateDelegate(typeof(UnityAction<string>), fc, exec);
            var cards = new[]
            {
                new { name = "RecentIllness", block = ill, title = "Recent illness", text = "How long the symptoms have lasted, and how they have changed." },
                new { name = "RecentMedicine", block = med, title = "Recent medicine use", text = "The child was given leftover medicine at home before coming in." },
                new { name = "HomeContext", block = ctx, title = "Home & community context", text = "A household member works with poultry.\nNEW LEAD: animal-production connection." },
            };
            foreach (var c in cards)
            {
                var card = panel.Find(c.name + "Card");
                var button = card.GetComponent<Button>();
                for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
                    if (button.onClick.GetPersistentMethodName(i) == "ExecuteBlock") UnityEventTools.RemovePersistentListener(button.onClick, i);
                UnityEventTools.AddStringPersistentListener(button.onClick, action, c.block.BlockName);
                card.Find("Title").GetComponent<TMP_Text>().text = c.title;
                card.Find("Hint").GetComponent<TMP_Text>().text = "Review  ·  5 capacity";
                card.Find(c.name + "Revealed/Evidence").GetComponent<TMP_Text>().text = c.text;
                EditorUtility.SetDirty(button);
            }
            panel.Find("Instruction").GetComponent<TMP_Text>().text =
                "Each card you open uses 5 capacity. You do not have to open every card.";
            EditorUtility.SetDirty(panel.gameObject);
        }

        static Character Char(string name)
        {
            return Object.FindObjectsByType<Character>(FindObjectsInactive.Include).First(c => c.NameText == name);
        }
    }
}

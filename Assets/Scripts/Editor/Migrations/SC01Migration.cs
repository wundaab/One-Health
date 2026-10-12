using System.Linq;
using Fungus;
using UnityEditor;
using UnityEngine;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// One-time migration of SC01 to the S01 TEACH blueprint and Core Mechanics Specification.
    /// See the commit message / report for the list of changes. Safe to delete after it has run.
    /// </summary>
    public static class SC01Migration
    {
        const string Marker = "S01_MIGRATED";

        public static string Run()
        {
            var fc = E.Find("Scenario01_Flowchart");
            if (fc.GetVariable(Marker) != null) return "Already migrated";

            var amina = Char("Amina");
            var samuel = Char("Samuel");
            var tplSay = (Say)E.B(fc, "Event01_Arrival_and_Orientation").CommandList[7];
            var tplMenu = (Fungus.Menu)E.B(fc, "Event02_First_Decision").CommandList[0];

            var selected = fc.GetVariable("SelectedOption");
            var lastChange = AddVar<StringVariable>(fc, "LastChange");
            var engaged = AddVar<BooleanVariable>(fc, "EngagedCommunity");
            AddVar<BooleanVariable>(fc, Marker);

            var resistanceSprite = Sprite("Assets/Art/01_Scenario_1/05_Icons/05_Icons_HD/Resistance.png");
            var oneHealthSprite = Sprite("Assets/Art/01_Scenario_1/03_One_Health_Visual/People_Animals_Environment_Card.png");
            var mOpening = Clip("Assets/Audio/M01_Opening.wav.mp3");
            var mDecision = Clip("Assets/Audio/M02_Decision.wav.mp3");
            var mReflection = Clip("Assets/Audio/M04_Reflection.wav.mp3");

            // ---------------------------------------------------------------- Start
            // The duplicate title menu is removed: the MainMenu scene is the title screen.
            var start = E.B(fc, "Start");
            var e1 = E.B(fc, "Event01_Arrival_and_Orientation");
            foreach (var c in start.CommandList.OfType<Fungus.Menu>().ToList()) E.Remove(c);
            var begin = E.Add<BeginScenario>(start);
            E.Set(begin, "scenarioId", "S01");
            E.Set(begin, "timeBudgetWeeks", 4);
            Music(start, -1, mOpening);
            E.Call(start, -1, e1);

            // ---------------------------------------------------------------- Event 01
            var ev1 = e1.CommandList.ToList();
            for (int i = 0; i <= 4; i++) E.Remove(ev1[i]);               // stat SetVariables: now Begin Scenario
            var fade = ev1[5];
            E.Say(e1, e1.CommandList.IndexOf(fade) + 1,
                "NEW MESSAGE — You have joined the community response team. A local clinic is seeing more requests for antibiotics without proper assessment.",
                null, tplSay);

            // HUD appears after the role introduction (blueprint §5, 1:10).
            int afterRole = e1.CommandList.IndexOf(ev1[8]) + 1;
            E.Insert<ShowHUD>(e1, afterRole);
            E.Say(e1, afterRole + 1,
                "This is your dashboard. Money and Time are what you can spend. Trust shows how willing people are to work with you. Resistance shows the pressure from antimicrobial resistance — higher is worse.",
                null, tplSay);

            E.Remove(ev1[10]);                                            // Player "Ok": the player has no voiced dialogue

            int visualAt = e1.CommandList.IndexOf(ev1[15]);
            E.Remove(ev1[15]);                                            // [SHOW VISUAL HERE]
            var resCard = E.Insert<ShowVisualCard>(e1, visualAt);
            E.Set(resCard, "title", "What is resistance?");
            E.Set(resCard, "image", resistanceSprite);
            E.Set(resCard, "caption", "Resistance describes the microorganism’s ability to survive or continue growing despite a medicine that would normally control it. It is the microorganism that becomes resistant, not the person.");
            var recAmr = E.Insert<RecordKnowledge>(e1, visualAt + 1);
            E.Set(recAmr, "knowledgeId", "AMR");

            var oneHealthLine = (Say)ev1[19];                             // moved to Event 04 (One Health is introduced at the second event)
            string oneHealthText = oneHealthLine.GetStandardText();
            E.Remove(oneHealthLine);

            var report = (Say)ev1[20];
            E.Set(report, "storyText", "COMMUNITY REPORT — Families are asking the clinic for antibiotics because they believe the medicine will make common illnesses go away faster.");
            E.Remove(ev1[25]);                                            // SetVariable Knowledge_AMR: now Record Knowledge

            // ---------------------------------------------------------------- Event 02 (first decision)
            var e2 = E.B(fc, "Event02_First_Decision");
            var c1a = E.B(fc, "Choice_1A");
            var c1b = E.B(fc, "Choice_1B");
            var c1c = E.B(fc, "Choice_1C");
            Music(e2, 0, mDecision);
            E.Say(e2, 1, "DECISION — Many people in the community are using antibiotics without proper guidance. You have limited resources. What should you do first?", null, tplSay);
            var menus = e2.CommandList.OfType<Fungus.Menu>().ToList();
            // Order and values follow S01 blueprint §10 (A/B/C) and the Action Effect Matrix.
            SetMenu(menus[0], "Fund a community information session  ·  Cost: 10 Money, 1 week", c1a);
            SetMenu(menus[1], "Strengthen clinic assessment and prescribing  ·  Cost: 15 Money, 1 week", c1b);
            SetMenu(menus[2], "Prioritise rapid access to treatment  ·  Cost: 20 Money, 1 week", c1c);

            var e3 = E.B(fc, "Event03_Consequence");
            var toCommunity = c1b.CommandList.OfType<FadeToView>().First();
            E.Move(toCommunity, e3, 0);                                   // every choice moves to the community reaction, not only 1B

            RewriteChoice(c1a, "1A", "Community information session", -10, -1, 8, -5, engaged, e3);
            RewriteChoice(c1b, "1B", "Clinic assessment and prescribing", -15, -1, 5, -8, null, e3);
            RewriteChoice(c1c, "1C", "Rapid access to treatment", -20, -1, 3, 3, null, e3);

            // ---------------------------------------------------------------- Event 03 (consequence)
            var ev3 = e3.CommandList.ToList();
            E.Set(ev3[1], "storyText", "RESULT — {$LastChange}");
            // Feedback text follows the action now that the IDs match the blueprint letters.
            E.SetStringCondition((VariableCondition)ev3[2], selected, "1C");    // immediate treatment text
            E.SetStringCondition((VariableCondition)ev3[6], selected, "1A");    // education text
            int afterEnd = e3.CommandList.IndexOf(ev3[8]) + 1;
            E.Say(e3, afterEnd, "Using an antimicrobial when it is not needed can contribute to conditions that favour resistance.", null, tplSay);
            var recRes = E.Insert<RecordKnowledge>(e3, afterEnd + 1);
            E.Set(recRes, "knowledgeId", "Resistance");
            var aminaOneHealth = (Say)ev3[11];                            // moved after the One Health visual in Event 04

            // ---------------------------------------------------------------- Event 04 (one week later, second decision)
            var e4 = E.B(fc, "Event04_Second_Decision");
            var c2a = E.B(fc, "Choice_2A");
            var c2b = E.B(fc, "Choice_2B");
            var e5 = E.B(fc, "Event05_Reflection");
            var c2c = E.NewBlock(fc, "Choice_2C", c2b._NodeRect.position + new Vector2(0, 90));

            var ev4 = e4.CommandList.ToList();
            E.Set(ev4[0], "storyText", "ONE WEEK LATER");
            int k = 1;
            var ifA = E.Insert<If>(e4, k++); E.SetStringCondition(ifA, selected, "1A");
            E.Say(e4, k++, "More families are asking questions before requesting antibiotics, and cooperation is improving. The information session used part of your budget.", null, tplSay);
            var ifB = E.Insert<ElseIf>(e4, k++); E.SetStringCondition(ifB, selected, "1B");
            E.Say(e4, k++, "The clinic is assessing patients more carefully, and fewer antibiotics are given without need. Some families are unhappy about the extra waiting, and they were not involved in the change.", null, tplSay);
            var ifC = E.Insert<ElseIf>(e4, k++); E.SetStringCondition(ifC, selected, "1C");
            E.Say(e4, k++, "The clinic is busier than ever. Many people still expect antibiotics for every illness, and the pressure on the clinic keeps growing.", null, tplSay);
            E.Insert<End>(e4, k++);

            var ohCard = E.Insert<ShowVisualCard>(e4, k++);
            E.Set(ohCard, "title", "One Health");
            E.Set(ohCard, "image", oneHealthSprite);
            E.Set(ohCard, "caption", oneHealthText);
            var recOh = E.Insert<RecordKnowledge>(e4, k++);
            E.Set(recOh, "knowledgeId", "One Health");
            E.Move(aminaOneHealth, e4, k++);
            E.Say(e4, k++, "Community leaders want a say in what happens next. They are waiting to hear how you will respond.", samuel, tplSay);
            E.Say(e4, k++, "TIP — Look at cost, time, trust and likely consequences before acting.", null, tplSay);

            var menus2 = e4.CommandList.OfType<Fungus.Menu>().ToList();
            // Second decision per S01 blueprint §11.
            SetMenu(menus2[0], "Consult community leaders  ·  Cost: 5 Money, 1 week", c2a);
            SetMenu(menus2[1], "Seek expert and community input  ·  Cost: 3 Money, 1 week", c2b);
            E.Menu(e4, -1, "Delay and take no action  ·  No direct cost", c2c, tplMenu);

            RewriteChoice(c2a, "2A", "Consult community leaders", -5, -1, 10, 0, engaged, e5);
            RewriteChoice(c2b, "2B", "Seek expert and community input", -3, -1, 2, 0, null, e5);
            RewriteChoice(c2c, "2C", "Delay / take no action", 0, 0, -8, 8, null, e5);

            // ---------------------------------------------------------------- Event 05 (reflection)
            var ev5 = e5.CommandList.ToList();
            Music(e5, 0, mReflection);
            E.Set(ev5[0], "storyText", "RESULT — {$LastChange}");
            E.Set(ev5[2], "storyText", "Community leaders felt heard and agreed to support the next steps. Trust grew because people were involved before anything changed.");
            E.Set(ev5[4], "storyText", "Talking with the clinic team and residents revealed something important: some families share leftover antibiotics or buy them without a prescription. You can use that information in your next response.");
            int endAt = e5.CommandList.IndexOf(ev5[5]);
            var if2c = E.Insert<ElseIf>(e5, endAt); E.SetStringCondition(if2c, selected, "2C");
            E.Say(e5, endAt + 1, "Nothing changed, so the pressure kept building. Families kept asking for antibiotics without assessment, and people felt their concerns were ignored. Inaction is also a decision.", null, tplSay);

            // One cause-and-effect relationship the player demonstrated or missed (blueprint Event 05).
            int at = e5.CommandList.IndexOf(ev5[6]);
            var ifEngaged = E.Insert<If>(e5, at++);
            E.SetBoolCondition(ifEngaged, engaged, true);
            E.Say(e5, at++, "You involved the community, and their support grew. Cooperation is a resource you can build.", null, tplSay);
            E.Insert<Else>(e5, at++);
            E.Say(e5, at++, "You did not involve the community this time. Without their support, even good interventions are harder to sustain.", null, tplSay);
            E.Insert<End>(e5, at++);
            E.Set(ev5[6], "storyText", "What did your decisions achieve? What did they cost, and who did they affect? What would you do differently next time?");

            // ---------------------------------------------------------------- Event 06 (knowledge check)
            // Answering a question is not a narrative cause for a Trust change, and Knowledge is a
            // concept record, not a score (Mechanics Rules / Compliance R02).
            var c3b = E.B(fc, "Choice_3B");
            foreach (var sv in c3b.CommandList.OfType<SetVariable>().ToList()) E.Remove(sv);

            // ---------------------------------------------------------------- Event 07 (completion)
            var e7 = E.B(fc, "Event07_Completion");
            var ev7 = e7.CommandList.ToList();
            E.Remove(ev7[0]);   // SCENARIO COMPLETE: shown by the results screen
            E.Remove(ev7[2]);   // +20 Money: undocumented reward
            E.Remove(ev7[3]);   // "+20 Money / Unlocked: Community Coordination"
            E.Remove(ev7[5]);   // [SAVED]
            E.Remove(ev7[6]);   // LoadScene SC02: NEXT on the results screen

            at = 1;
            var ifUnlock = E.Insert<If>(e7, at++);
            E.SetBoolCondition(ifUnlock, engaged, true);
            var unlock = E.Insert<UnlockCapability>(e7, at++);
            E.Set(unlock, "capabilityId", "Community Engagement");
            E.Say(e7, at++, "UNLOCKED — Community Engagement. Because you involved the community, you can use community engagement actions in the next stage.", null, tplSay);
            E.Insert<Else>(e7, at++);
            E.Say(e7, at++, "Community Engagement was not unlocked this time. Involving the community in your decisions would have earned it.", null, tplSay);
            E.Insert<End>(e7, at++);
            Music(e7, -1, mOpening);
            var complete = E.Add<CompleteScenario>(e7);
            E.Set(complete, "scenarioId", "S01");
            E.Set(complete, "scenarioTitle", "S01 — The First Response");
            E.Set(complete, "nextScene", "SC02");
            E.Set(complete, "nextLabel", "BUILD");
            E.Set(complete, "summary", "You learned what AMR and resistance mean, made two decisions with limited resources, and saw how each one changed Money, Time, Trust and Resistance. The next stage gives less guidance and stronger trade-offs.");

            // ---------------------------------------------------------------- variables & scene objects
            var knowledgeInt = fc.GetVariable("Knowledge");
            if (knowledgeInt != null) { fc.Variables.Remove(knowledgeInt); Undo.DestroyObjectImmediate(knowledgeInt); }

            foreach (var name in new[] { "MainMenuDialog", "SaveMenu" })
            {
                var go = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == name);
                if (go != null) Undo.DestroyObjectImmediate(go);
            }
            if (Object.FindAnyObjectByType<StatHUD>(FindObjectsInactive.Include) == null)
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/GameUI.prefab"));

            E.Finish(fc);
            return E.DumpAll(fc);
        }

        /// <summary>
        /// Re-creates the custom commands that were dropped when they lived in a single
        /// multi-class file (Unity can only serialize a MonoBehaviour whose file name matches).
        /// </summary>
        public static string RepairDroppedCommands()
        {
            var fc = E.Find("Scenario01_Flowchart");
            var engaged = fc.GetVariable("EngagedCommunity");

            Replace<BeginScenario>(fc, "Start", 2, c => { E.Set(c, "scenarioId", "S01"); E.Set(c, "timeBudgetWeeks", 4); });
            Replace<ShowHUD>(fc, "Event01_Arrival_and_Orientation", 5, c => { });
            Replace<RecordKnowledge>(fc, "Event01_Arrival_and_Orientation", 13, c => E.Set(c, "knowledgeId", "AMR"));
            Replace<RecordKnowledge>(fc, "Event03_Consequence", 10, c => E.Set(c, "knowledgeId", "Resistance"));
            Replace<RecordKnowledge>(fc, "Event04_Second_Decision", 9, c => E.Set(c, "knowledgeId", "One Health"));
            Replace<UnlockCapability>(fc, "Event07_Completion", 2, c => E.Set(c, "capabilityId", "Community Engagement"));

            Decision(fc, "Choice_1A", "1A", "Community information session", -10, -1, 8, -5);
            Decision(fc, "Choice_1B", "1B", "Clinic assessment and prescribing", -15, -1, 5, -8);
            Decision(fc, "Choice_1C", "1C", "Rapid access to treatment", -20, -1, 3, 3);
            Decision(fc, "Choice_2A", "2A", "Consult community leaders", -5, -1, 10, 0);
            Decision(fc, "Choice_2B", "2B", "Seek expert and community input", -3, -1, 2, 0);
            Decision(fc, "Choice_2C", "2C", "Delay / take no action", 0, 0, -8, 8);

            E.Finish(fc);
            return E.DumpAll(fc);
        }

        static void Replace<T>(Flowchart fc, string block, int index, System.Action<T> configure) where T : Command
        {
            var b = E.B(fc, block);
            if (b.CommandList[index] != null) throw new System.Exception(block + "[" + index + "] is not empty");
            b.CommandList.RemoveAt(index);
            configure(E.Insert<T>(b, index));
        }

        static void Decision(Flowchart fc, string block, string id, string action, int money, int weeks, int trust, int resistance)
        {
            Replace<ApplyDecision>(fc, block, 0, d =>
            {
                E.Set(d, "decisionId", id);
                E.Set(d, "actionName", action);
                E.Set(d, "money", money);
                E.Set(d, "timeWeeks", weeks);
                E.Set(d, "trust", trust);
                E.Set(d, "resistance", resistance);
            });
        }

        static void RewriteChoice(Block block, string id, string action, int money, int weeks, int trust, int resistance,
            Variable engagedVar, Block next)
        {
            foreach (var c in block.CommandList.ToList()) E.Remove(c);
            var d = E.Add<ApplyDecision>(block);
            E.Set(d, "decisionId", id);
            E.Set(d, "actionName", action);
            E.Set(d, "money", money);
            E.Set(d, "timeWeeks", weeks);
            E.Set(d, "trust", trust);
            E.Set(d, "resistance", resistance);
            if (engagedVar != null)
            {
                var sv = E.Add<SetVariable>(block);
                E.Set(sv, "anyVar.variable", engagedVar);
                E.Set(sv, "anyVar.data.booleanData.booleanVal", true);
            }
            E.Call(block, -1, next);
        }

        static void SetMenu(Fungus.Menu m, string text, Block target)
        {
            E.Set(m, "text", text);
            E.Set(m, "targetBlock", target);
        }

        static void Music(Block block, int index, AudioClip clip)
        {
            var pm = E.Insert<PlayMusic>(block, index);
            E.Set(pm, "musicClip", clip);
            E.Set(pm, "loop", true);
            E.Set(pm, "fadeDuration", 1f);
        }

        static T AddVar<T>(Flowchart fc, string key) where T : Variable
        {
            var v = Undo.AddComponent<T>(fc.gameObject);
            v.Key = key;
            v.Scope = VariableScope.Private;
            v.hideFlags = HideFlags.HideInInspector;
            fc.Variables.Add(v);
            return v;
        }

        static Character Char(string name)
        {
            return Object.FindObjectsByType<Character>(FindObjectsInactive.Include).First(c => c.NameText == name);
        }

        static Sprite Sprite(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
        }

        static AudioClip Clip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}

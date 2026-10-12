using System.Linq;
using Fungus;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using E = OneHealth.EditorTools.FungusEdit;

namespace OneHealth.EditorTools
{
    /// <summary>
    /// One-time migration of SC02 ("The Fever That Won't Wait"): state continuity from S01,
    /// shared HUD/pause/results UI and dialogs, routing fixes, case board content, the
    /// Community Engagement capability, and the unfinished final decision and ending.
    /// </summary>
    public static class SC02Migration
    {
        const string Marker = "S02_MIGRATED";
        const string Capacity = "ClinicResponseCapacity";

        public static string Run()
        {
            var fc = Object.FindAnyObjectByType<Flowchart>();
            if (fc.GetVariable(Marker) != null) return "Already migrated";

            // ---------------------------------------------------------------- scene UI
            // Same custom dialogs as S01 (SC02 had none, so Fungus fell back to its stock dialog).
            var sayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialogs/CustomSayDialogBox.prefab");
            var menuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Dialogs/CustomSelectionMenuDialog.prefab");
            var say = (GameObject)PrefabUtility.InstantiatePrefab(sayPrefab);
            var menu = (GameObject)PrefabUtility.InstantiatePrefab(menuPrefab);
            var gameUI = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/GameUI.prefab"));
            var capacityChip = gameUI.GetComponentsInChildren<StatChip>(true).First(c => c.Kind == StatKind.FlowchartInteger);
            capacityChip.gameObject.SetActive(true);

            var sayDialog = say.GetComponent<SayDialog>();
            var menuDialog = menu.GetComponent<MenuDialog>();

            var amina = Char("Amina");
            var malik = Char("Dr. Malik");
            var caregiver = Char("Caregiver");
            var trustVar = fc.GetVariable("Trust");
            AddVar<BooleanVariable>(fc, Marker);
            var unlockedCE = AddVar<BooleanVariable>(fc, "Unlocked_CommunityEngagement");
            var choice = AddVar<StringVariable>(fc, "FinalChoice");

            var mDecision = Clip("Assets/Audio/M02_Decision.wav.mp3");
            var mTension = Clip("Assets/Audio/M03_Tension.wav.mp3");
            var mReflection = Clip("Assets/Audio/M04_Reflection.wav.mp3");

            // ---------------------------------------------------------------- start: inherit S01 state
            var start = E.B(fc, "S02_Start");
            var begin = E.Insert<BeginScenario>(start, 1);
            E.Set(begin, "scenarioId", "S02");
            E.Set(begin, "timeBudgetWeeks", -1);
            E.Insert<ShowHUD>(start, 2);
            Music(start, 3, mDecision);

            // ---------------------------------------------------------------- E01: opening reflects S01 (blueprint §22)
            var e1 = E.B(fc, "E01_Return_to_Kivu");
            var tplSay = (Say)e1.CommandList[1];
            Bind(tplSay, sayDialog);
            int k = 2;
            var ifHigh = E.Insert<If>(e1, k++);
            IntCondition(ifHigh, trustVar, 5 /* GreaterThanOrEquals */, 60);
            E.Say(e1, k++, "People remember that you involved them last time. More families are willing to work with the clinic.", amina, tplSay);
            var ifLow = E.Insert<ElseIf>(e1, k++);
            IntCondition(ifLow, trustVar, 2 /* LessThan */, 50);
            E.Say(e1, k++, "Some families are still unsure about the changes at the clinic. We will need to rebuild their confidence.", amina, tplSay);
            E.Insert<End>(e1, k++);

            // ---------------------------------------------------------------- D01: show costs; treatment without assessment
            var d01 = E.B(fc, "D01_Prioritize_the_Response");
            Label(d01, 1, "Move directly toward treatment  ·  Uses 5 capacity", menuDialog);
            Label(d01, 2, "Support a focused assessment  ·  Uses 8 capacity", menuDialog);
            Label(d01, 3, "Gather wider background first  ·  Uses 7 capacity", menuDialog);
            // Treating before assessment is the same action type as S01's "rapid access to treatment"
            // (Action Effect Matrix: Immediate response, Resistance 0 to +3).
            Decision(E.B(fc, "D01_A_Treatment"), 0, "D01A", "Treatment before assessment", 0, 3);

            // ---------------------------------------------------------------- Case board content
            BuildCaseBoard();

            // ---------------------------------------------------------------- E03 / D02
            var e3 = E.B(fc, "E03_Evidence_Check");
            var recEvidence = E.Insert<RecordKnowledge>(e3, e3.CommandList.Count - 1);
            E.Set(recEvidence, "knowledgeId", "Evidence before assumptions");
            var d02 = E.B(fc, "D02_Reduce_Uncertainty");
            E.Remove(d02.CommandList.OfType<Call>().Single());   // jumped to D02_A without waiting for the player's choice
            Label(d02, 0, "Gather additional clinical evidence  ·  Uses 10 capacity", menuDialog);
            Label(d02, 1, "Use available diagnostic pathway  ·  Uses 20 capacity", menuDialog);
            Label(d02, 2, "Proceed with current information  ·  No capacity used", menuDialog);

            // TEST_RESULT was empty, so choosing the diagnostic pathway ended the scenario.
            var test = E.B(fc, "TEST_RESULT");
            E.Say(test, -1, "RESULT AVAILABLE — The test gives the team more specific evidence. Dr. Malik can now choose the next step with more confidence.", null, tplSay);
            E.Say(test, -1, "This is why we test when we can. It shows us what we are treating, instead of guessing.", malik, tplSay);
            E.Call(test, -1, E.B(fc, "D03_Response"));

            var d03 = E.B(fc, "D03_Response");
            Label(d03, 0, "Supportive care + monitoring where clinically appropriate  ·  Uses 10 capacity", menuDialog);
            Label(d03, 1, "Clinician-guided antimicrobial treatment when indicated  ·  Uses 15 capacity", menuDialog);
            Label(d03, 2, "Escalate / refer for additional clinical support  ·  Uses 25 capacity", menuDialog);

            // ---------------------------------------------------------------- E04: speakers were missing
            var e4 = E.B(fc, "E04_Caregiver");
            E.Set(e4.CommandList[1], "character", caregiver);
            E.Set(e4.CommandList[4], "character", malik);

            // ---------------------------------------------------------------- D04: Trust now reaches the campaign state
            var d04 = E.B(fc, "D04_Followup");
            Label(d04, 0, "Explain the plan + arrange follow-up  ·  Uses 10 capacity", menuDialog);
            Label(d04, 1, "Quick explanation / end visit  ·  Uses 2 capacity", menuDialog);
            Label(d04, 2, "Arrange additional support  ·  Uses 15 capacity", menuDialog);
            ReplaceTrust(E.B(fc, "D04_A_Plan"), "D04A", "Explain plan and arrange follow-up");
            ReplaceTrust(E.B(fc, "D04_B_Quick"), "D04B", "Quick explanation");
            ReplaceTrust(E.B(fc, "D04_C_Support"), "D04C", "Arrange additional support");

            // Community Engagement (earned in S01) gives a Trust-related action here (S01 blueprint §14).
            var d04d = E.NewBlock(fc, "D04_D_Community", E.B(fc, "D04_C_Support")._NodeRect.position + new Vector2(0, 90));
            var ce = E.Menu(d04, -1, "Ask the community network to support follow-up  ·  Uses 5 capacity  ·  Requires Community Engagement", d04d,
                (Fungus.Menu)d04.CommandList[0]);
            E.Set(ce, "interactable.booleanRef", unlockedCE);
            CapacityCost(d04d, 5);
            Decision(d04d, -1, "D04D", "Community network supports follow-up", 8, 0);
            E.Say(d04d, -1, "Amina’s community network agrees to check on the family. Follow-up is more likely to happen, and the family feels supported.", null, tplSay);
            E.Call(d04d, -1, E.B(fc, "E05_Signal"));

            // ---------------------------------------------------------------- E05: tension + connection board
            var e5 = E.B(fc, "E05_Signal");
            Music(e5, 0, mTension);
            var recCausation = E.Insert<RecordKnowledge>(e5, e5.CommandList.Count - 1);
            E.Set(recCausation, "knowledgeId", "Connection is not causation");
            var board = E.B(fc, "CONNECTION_BOARD");
            E.Say(board, 0, "ONE HEALTH BOARD — Which connection will you look into first?  Each contact uses 10 capacity.", null, tplSay);
            Label(board, 1, "People — check nearby households for similar illness", menuDialog);
            Label(board, 2, "Animals — talk to the animal-health officer about the signal", menuDialog);
            Label(board, 3, "Environment — check the shared water source", menuDialog);
            SectorLine(E.B(fc, "D05_People"), "The community health team checks nearby households. Two other children had a fever last week, and both are recovering.", tplSay);
            SectorLine(E.B(fc, "D05_Animals"), "The animal-health officer explains that antibiotics are used often on the nearby farm. The resistant signal will be investigated further.", tplSay);
            SectorLine(E.B(fc, "D05_Env"), "The environmental health team will test the shared water source. Results will take time, but the check has started.", tplSay);

            // ---------------------------------------------------------------- D06: final allocation (was "Option Text" with empty blocks)
            var d06 = E.B(fc, "D06_Coordination");
            E.Say(d06, 0, "FINAL DECISION — How will you use the clinic’s remaining capacity?", null, tplSay);
            Label(d06, 1, "Coordinate a joint response with human, animal and environmental health partners  ·  Uses 15 capacity", menuDialog);
            Label(d06, 2, "Strengthen clinic follow-up and record-keeping  ·  Uses 10 capacity", menuDialog);
            Label(d06, 3, "Focus only on this patient’s follow-up  ·  Uses 5 capacity", menuDialog);

            var outcome = E.NewBlock(fc, "E06_Outcome", E.B(fc, "D06_A_Coordinate")._NodeRect.position + new Vector2(260, 0));
            Final(E.B(fc, "D06_A_Coordinate"), "A", 15, "D06A", "Joint One Health response", 5, -5, outcome);
            Final(E.B(fc, "D06_B_Clinic"), "B", 10, "D06B", "Clinic follow-up and records", 2, 0, outcome);
            Final(E.B(fc, "D06_C_Patient"), "C", 5, "D06C", "Single-patient follow-up", 0, 3, outcome);

            // ---------------------------------------------------------------- E06: outcome, reflection, results
            Music(outcome, -1, mReflection);
            var ifA = E.Insert<If>(outcome, -1); E.SetStringCondition(ifA, choice, "A");
            E.Say(outcome, -1, "Health, animal and environmental partners agreed to share what they find. The resistant signal will be followed up across the whole system, not only at the clinic.", null, tplSay);
            var ifB = E.Insert<ElseIf>(outcome, -1); E.SetStringCondition(ifB, choice, "B");
            E.Say(outcome, -1, "The clinic now records cases and follow-up more carefully. That will help the team notice patterns, but the signal outside the clinic is not being followed up yet.", null, tplSay);
            var ifC = E.Insert<ElseIf>(outcome, -1); E.SetStringCondition(ifC, choice, "C");
            E.Say(outcome, -1, "The child’s follow-up is in place. But nobody is following up the resistant signal, so the wider risk remains unknown.", null, tplSay);
            E.Insert<End>(outcome, -1);
            E.Say(outcome, -1, "One case can be part of a bigger picture. A connection is a reason to investigate, not proof of the cause.", amina, tplSay);
            E.Say(outcome, -1, "What did your choices reveal? What did they cost the clinic? What would you check first next time?", null, tplSay);
            var complete = E.Add<CompleteScenario>(outcome);
            E.Set(complete, "scenarioId", "S02");
            E.Set(complete, "scenarioTitle", "S02 — The Fever That Won’t Wait");
            E.Set(complete, "nextScene", "");
            E.Set(complete, "summary", "You managed a real case with limited clinic capacity, reduced uncertainty before acting, and followed a One Health signal without assuming its cause.");

            // Trust is synced from the campaign state by Begin Scenario.
            var trustInt = (IntegerVariable)trustVar;
            trustInt.Value = 50;
            EditorUtility.SetDirty(trustInt);

            E.Finish(fc);
            return E.DumpAll(fc);
        }

        // ------------------------------------------------------------------ helpers

        static void Label(Block block, int index, string text, MenuDialog dialog)
        {
            var m = (Fungus.Menu)block.CommandList[index];
            E.Set(m, "text", text);
            E.Set(m, "setMenuDialog", dialog);
        }

        static void Bind(Say template, SayDialog dialog)
        {
            E.Set(template, "setSayDialog", dialog);
        }

        static void Decision(Block block, int index, string id, string action, int trust, int resistance)
        {
            var d = E.Insert<ApplyDecision>(block, index);
            E.Set(d, "decisionId", id);
            E.Set(d, "actionName", action);
            E.Set(d, "trust", trust);
            E.Set(d, "resistance", resistance);
        }

        static void CapacityCost(Block block, int cost)
        {
            var fc = block.GetFlowchart();
            var sv = E.Add<SetVariable>(block);
            E.Set(sv, "anyVar.variable", fc.GetVariable(Capacity));
            E.Set(sv, "setOperator", 3); // Subtract
            E.Set(sv, "anyVar.data.integerData.integerVal", cost);
        }

        /// <summary>Replaces "SetVariable Trust += n" with Apply Decision so the change reaches the campaign state.</summary>
        static void ReplaceTrust(Block block, string id, string action)
        {
            var trustCmd = block.CommandList.OfType<SetVariable>().First(s => ((Variable)E.Get(s, "anyVar.variable")).Key == "Trust");
            int value = (int)E.Get(trustCmd, "anyVar.data.integerData.integerVal");
            int op = (int)E.Get(trustCmd, "setOperator");
            int delta = op == 3 ? -value : value;
            int at = block.CommandList.IndexOf(trustCmd);
            E.Remove(trustCmd);
            Decision(block, at, id, action, delta, 0);
        }

        static void SectorLine(Block block, string text, Say template)
        {
            E.Say(block, block.CommandList.Count - 1, text, null, template);
        }

        static void Final(Block block, string letter, int capacity, string id, string action, int trust, int resistance, Block next)
        {
            var fc = block.GetFlowchart();
            CapacityCost(block, capacity);
            Decision(block, -1, id, action, trust, resistance);
            var sv = E.Add<SetVariable>(block);
            E.Set(sv, "anyVar.variable", fc.GetVariable("FinalChoice"));
            E.Set(sv, "anyVar.data.stringData.stringVal", letter);
            E.Call(block, -1, next);
        }

        static void IntCondition(VariableCondition cond, Variable v, int compareOp, int value)
        {
            E.EnsureCondition(cond);
            E.Set(cond, "conditions.Array.data[0].anyVar.variable", v);
            E.Set(cond, "conditions.Array.data[0].compareOperator", compareOp);
            E.Set(cond, "conditions.Array.data[0].anyVar.data.integerData.integerVal", value);
        }

        static void Music(Block block, int index, AudioClip clip)
        {
            var pm = E.Insert<PlayMusic>(block, index);
            E.Set(pm, "musicClip", clip);
            E.Set(pm, "loop", true);
            E.Set(pm, "fadeDuration", 1f);
        }

        /// <summary>The case board art ships as empty shells; add the text the player needs to read.</summary>
        static void BuildCaseBoard()
        {
            var panel = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "CaseBoardPanel" && g.scene.IsValid());
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/Nunito-Bold SDF.asset");
            var regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/Nunito-Regular SDF.asset");

            Text(panel.transform, "Title", "CASE BOARD", bold, 34, UITheme.TealDark, new Vector2(0, 220), new Vector2(800, 50));
            Text(panel.transform, "Instruction", "Select each card to review the evidence, then continue.", regular, 22, UITheme.Teal, new Vector2(0, 175), new Vector2(800, 36));

            Card(panel.transform, "RecentIllnessCard", "Recent illness",
                "Fever for three days. Eating less, but still drinking.", bold, regular);
            Card(panel.transform, "RecentMedicineCard", "Recent medicine",
                "The family gave leftover antibiotics from a neighbour before coming to the clinic.", bold, regular);
            Card(panel.transform, "HomeContextCard", "Home context",
                "The family keeps chickens and goats, and collects water from a shared source.", bold, regular);

            var cont = panel.transform.Find("ContinueButton");
            var img = cont.GetComponent<Image>();
            img.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/01_Scenario_1/04_UI/Buttons/Green/normal.png").OfType<Sprite>().First();
            img.type = Image.Type.Sliced;
            ((RectTransform)cont).sizeDelta = new Vector2(260, 72);
            ((RectTransform)cont).anchoredPosition = new Vector2(0, -205);
            foreach (var t in cont.GetComponentsInChildren<TMP_Text>(true))
            {
                t.font = bold;
                t.fontSize = 28;
                t.color = Color.white;
                t.text = "Continue";
            }
            EditorUtility.SetDirty(panel);
        }

        /// <summary>
        /// Lays text onto the evidence card shells: title above the card, type icon in the circle
        /// slot, "Select to review" on the closed card, evidence text over the bar slots when revealed.
        /// </summary>
        public static void LayoutCaseBoard()
        {
            var panel = Resources.FindObjectsOfTypeAll<GameObject>().First(g => g.name == "CaseBoardPanel" && g.scene.IsValid()).transform;
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/Nunito-Bold SDF.asset");
            var regular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Fonts/Nunito-Regular SDF.asset");
            foreach (var name in new[] { "RecentIllness", "RecentMedicine", "HomeContext" })
            {
                var card = (RectTransform)panel.Find(name + "Card");
                card.sizeDelta = new Vector2(288, 236);
                card.anchoredPosition = new Vector2(Mathf.Sign(card.anchoredPosition.x) * (Mathf.Abs(card.anchoredPosition.x) > 1 ? 300 : 0), -24);

                var title = (RectTransform)card.Find("Title");
                title.anchoredPosition = new Vector2(0, 144);
                title.GetComponent<TMP_Text>().fontSize = 26;

                var icon = (RectTransform)card.Find(name + "Icon");
                icon.anchoredPosition = new Vector2(0, 2);
                icon.sizeDelta = new Vector2(64, 64);

                var hint = (RectTransform)card.Find("Hint");
                hint.anchoredPosition = new Vector2(0, -88);
                hint.GetComponent<TMP_Text>().fontSize = 19;

                var rev = (RectTransform)card.Find(name + "Revealed");
                rev.anchorMin = Vector2.zero;
                rev.anchorMax = Vector2.one;
                rev.offsetMin = rev.offsetMax = Vector2.zero;
                rev.SetAsLastSibling();          // covers the closed-card icon and hint
                title.SetAsLastSibling();        // title stays visible above the card

                var revIconTf = rev.Find("Icon");
                if (revIconTf == null)
                {
                    var revIcon = Object.Instantiate(icon.gameObject, rev);
                    revIcon.name = "Icon";
                    revIconTf = revIcon.transform;
                }
                ((RectTransform)revIconTf).anchoredPosition = new Vector2(0, 29);
                ((RectTransform)revIconTf).sizeDelta = new Vector2(56, 56);

                var ev = rev.Find("Evidence").GetComponent<TextMeshProUGUI>();
                ev.rectTransform.anchoredPosition = new Vector2(0, -74);
                ev.rectTransform.sizeDelta = new Vector2(250, 76);
                ev.font = regular;
                ev.enableAutoSizing = true;
                ev.fontSizeMin = 14;
                ev.fontSizeMax = 17;
                ev.textWrappingMode = TextWrappingModes.Normal;
                ev.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(card.gameObject);
            }
            var instr = (RectTransform)panel.Find("Instruction");
            instr.anchoredPosition = new Vector2(0, 196);
            var titleMain = (RectTransform)panel.Find("Title");
            titleMain.anchoredPosition = new Vector2(0, 240);
            ((RectTransform)panel.Find("ContinueButton")).anchoredPosition = new Vector2(0, -226);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        }

        static void Card(Transform panel, string cardName, string title, string revealed, TMP_FontAsset bold, TMP_FontAsset regular)
        {
            var card = panel.Find(cardName);
            Text(card, "Title", title, bold, 26, UITheme.TealDark, new Vector2(0, 62), new Vector2(240, 40));
            Text(card, "Hint", "Select to review", regular, 20, UITheme.Teal, new Vector2(0, 0), new Vector2(240, 30));
            var rev = card.Find(cardName.Replace("Card", "Revealed"));
            Text(rev, "Evidence", revealed, regular, 20, UITheme.TealDark, new Vector2(0, -18), new Vector2(230, 120));
        }

        static TextMeshProUGUI Text(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color, Vector2 pos, Vector2 box)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchoredPosition = pos;
            rt.sizeDelta = box;
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
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

        static AudioClip Clip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}

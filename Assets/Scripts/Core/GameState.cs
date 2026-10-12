using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// The persistent player state defined by the Progression / State System.
    /// Money, Time, Trust and Resistance plus knowledge records, unlocked capabilities
    /// and decision history. Carried from one scenario into the next.
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Ranges from the Core Mechanics Specification, sheet "02 Parameter Values".
        public const int MoneyMin = 0, MoneyMax = 150;
        public const int TimeMin = 0, TimeMax = 4;
        public const int TrustMin = 0, TrustMax = 100;
        public const int ResistanceMin = 0, ResistanceMax = 100;

        // Threshold rules from sheet "04 Progression & Outcomes".
        public const int TrustCriticalBelow = 20;
        public const int ResistanceCriticalAbove = 80;

        public int money = 100;
        public int timeWeeks = 4;
        public int trust = 50;
        public int resistance = 50;

        public List<string> knowledge = new List<string>();
        public List<string> unlocks = new List<string>();
        public List<string> decisions = new List<string>();
        public List<string> completedScenarios = new List<string>();

        /// <summary>Scene the player resumes from with Continue (the current scenario's start).</summary>
        public string checkpointScene = "";

        public static GameState NewCampaign()
        {
            return new GameState();
        }

        public GameState Clone()
        {
            return JsonUtility.FromJson<GameState>(JsonUtility.ToJson(this));
        }

        public bool HasKnowledge(string id) { return knowledge.Contains(id); }
        public bool HasUnlock(string id) { return unlocks.Contains(id); }
        public bool HasCompleted(string scenarioId) { return completedScenarios.Contains(scenarioId); }

        public void AddUnique(List<string> list, string id)
        {
            if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                list.Add(id);
        }

        public void Clamp()
        {
            money = Mathf.Clamp(money, MoneyMin, MoneyMax);
            timeWeeks = Mathf.Clamp(timeWeeks, TimeMin, TimeMax);
            trust = Mathf.Clamp(trust, TrustMin, TrustMax);
            resistance = Mathf.Clamp(resistance, ResistanceMin, ResistanceMax);
        }
    }

    /// <summary>A set of parameter changes caused by one confirmed action.</summary>
    [Serializable]
    public struct StatDelta
    {
        public int money;
        public int timeWeeks;
        public int trust;
        public int resistance;

        public StatDelta(int money, int timeWeeks, int trust, int resistance)
        {
            this.money = money;
            this.timeWeeks = timeWeeks;
            this.trust = trust;
            this.resistance = resistance;
        }

        public bool IsEmpty { get { return money == 0 && timeWeeks == 0 && trust == 0 && resistance == 0; } }

        /// <summary>Player-facing summary, e.g. "Money −10 · Time −1 week · Trust +8 · Resistance −5".</summary>
        public string ToDisplayString()
        {
            var parts = new List<string>();
            if (money != 0) parts.Add("Money " + Signed(money));
            if (timeWeeks != 0) parts.Add("Time " + Signed(timeWeeks) + (Mathf.Abs(timeWeeks) == 1 ? " week" : " weeks"));
            if (trust != 0) parts.Add("Trust " + Signed(trust));
            if (resistance != 0) parts.Add("Resistance " + Signed(resistance));
            return parts.Count == 0 ? "No change" : string.Join("  ·  ", parts.ToArray());
        }

        public static string Signed(int v)
        {
            return v > 0 ? "+" + v : (v < 0 ? "−" + (-v) : "0");
        }
    }
}

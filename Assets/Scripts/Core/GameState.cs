using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// The persistent player state carried from one scenario into the next: the five gameplay
    /// parameters (Money, Time, Trust, Resistance Pressure, Capacity), knowledge records,
    /// unlocked capabilities, badges, story flags and decision history.
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Scenario 3/4 blueprints: clamp Money, Trust, Resistance Pressure and Capacity to 0-100;
        // Time is an action budget that cannot fall below 0.
        public const int MoneyMin = 0, MoneyMax = 100;
        public const int TimeMin = 0, TimeMax = 12;
        public const int TrustMin = 0, TrustMax = 100;
        public const int ResistanceMin = 0, ResistanceMax = 100;
        public const int CapacityMin = 0, CapacityMax = 100;

        // Threshold rules from the Core Mechanics Specification, sheet "04 Progression & Outcomes".
        public const int TrustCriticalBelow = 20;
        public const int ResistanceCriticalAbove = 80;

        public int money = 100;
        public int timeWeeks = 4;
        public int trust = 50;
        public int resistance = 50;
        public int capacity = 100;

        public List<string> knowledge = new List<string>();
        public List<string> unlocks = new List<string>();
        public List<string> badges = new List<string>();
        public List<string> decisions = new List<string>();
        public List<string> completedScenarios = new List<string>();
        public List<Flag> flags = new List<Flag>();

        /// <summary>Scene the player resumes from with Continue (the current scenario's start).</summary>
        public string checkpointScene = "";

        [Serializable]
        public class Flag
        {
            public string key;
            public string value;
        }

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

        public string GetFlag(string key)
        {
            var f = flags.Find(x => x.key == key);
            return f == null ? "" : f.value;
        }

        public void SetFlag(string key, string value)
        {
            var f = flags.Find(x => x.key == key);
            if (f == null) flags.Add(new Flag { key = key, value = value });
            else f.value = value;
        }

        public void AddUnique(List<string> list, string id)
        {
            if (!string.IsNullOrEmpty(id) && !list.Contains(id))
                list.Add(id);
        }

        /// <summary>Whether the state can pay a set of (negative) costs without going below zero.</summary>
        public bool CanAfford(StatDelta d)
        {
            return money + Mathf.Min(0, d.money) >= MoneyMin
                && timeWeeks + Mathf.Min(0, d.timeWeeks) >= TimeMin
                && capacity + Mathf.Min(0, d.capacity) >= CapacityMin;
        }

        /// <summary>Names of the parameters that cannot cover a cost, e.g. "Money, Time".</summary>
        public string Shortfall(StatDelta d)
        {
            var missing = new List<string>();
            if (money + d.money < MoneyMin && d.money < 0) missing.Add("Money");
            if (timeWeeks + d.timeWeeks < TimeMin && d.timeWeeks < 0) missing.Add("Time");
            if (capacity + d.capacity < CapacityMin && d.capacity < 0) missing.Add("Capacity");
            return string.Join(", ", missing.ToArray());
        }

        public void Clamp()
        {
            money = Mathf.Clamp(money, MoneyMin, MoneyMax);
            timeWeeks = Mathf.Clamp(timeWeeks, TimeMin, TimeMax);
            trust = Mathf.Clamp(trust, TrustMin, TrustMax);
            resistance = Mathf.Clamp(resistance, ResistanceMin, ResistanceMax);
            capacity = Mathf.Clamp(capacity, CapacityMin, CapacityMax);
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
        public int capacity;

        public StatDelta(int money, int timeWeeks, int trust, int resistance, int capacity = 0)
        {
            this.money = money;
            this.timeWeeks = timeWeeks;
            this.trust = trust;
            this.resistance = resistance;
            this.capacity = capacity;
        }

        public bool IsEmpty { get { return money == 0 && timeWeeks == 0 && trust == 0 && resistance == 0 && capacity == 0; } }

        /// <summary>Player-facing summary, e.g. "Money −10 · Time −1 week · Trust +8 · Resistance Pressure −5".</summary>
        public string ToDisplayString()
        {
            var parts = new List<string>();
            if (money != 0) parts.Add("Money " + Signed(money));
            if (timeWeeks != 0) parts.Add("Time " + Signed(timeWeeks) + (Mathf.Abs(timeWeeks) == 1 ? " week" : " weeks"));
            if (trust != 0) parts.Add("Trust " + Signed(trust));
            if (resistance != 0) parts.Add("Resistance Pressure " + Signed(resistance));
            if (capacity != 0) parts.Add("Capacity " + Signed(capacity));
            return parts.Count == 0 ? "No change" : string.Join("  ·  ", parts.ToArray());
        }

        public static string Signed(int v)
        {
            return v > 0 ? "+" + v : (v < 0 ? "−" + (-v) : "0");
        }
    }
}

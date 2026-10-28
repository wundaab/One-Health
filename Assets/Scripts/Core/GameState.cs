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

        // Capabilities unlocked in Scenarios 1 and 2 (S01 blueprint: "Trust-related capability").
        public const string CommunityCoordination = "Community Coordination";
        public const string CrossSectorResponse = "Cross-Sector Response";

        /// <summary>Every badge the campaign can award, in the order they can be earned.</summary>
        public static readonly string[] AllBadges =
        {
            "Evidence Tracker", "Joint Response", "One Health Coordinator",
            "Community Partner", "Responsible Stewardship", "Precaution With Evidence",
            "Kivu Coordinator", "One Health Champion",
        };

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

        /// <summary>
        /// A decision's effects including an unlocked capability's bonus, e.g. Community Coordination
        /// adds Trust to community-engagement choices; Cross-Sector Response lowers a Capacity cost.
        /// </summary>
        public StatDelta WithCapability(StatDelta d, string capability, StatDelta bonus)
        {
            return CapabilityApplies(capability) ? d + bonus : d;
        }

        public bool CapabilityApplies(string capability)
        {
            return !string.IsNullOrEmpty(capability) && HasUnlock(capability);
        }

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

        public static StatDelta operator +(StatDelta a, StatDelta b)
        {
            return new StatDelta(a.money + b.money, a.timeWeeks + b.timeWeeks, a.trust + b.trust,
                a.resistance + b.resistance, a.capacity + b.capacity);
        }

        /// <summary>
        /// Resource line shown before the player commits, e.g. "Cost: 10 Money, 1 week, 2 Capacity".
        /// Trust and Resistance Pressure are outcomes and stay hidden until the decision is made
        /// (Vision: the player should not necessarily know the exact outcome). Matches Scenario 1's labels.
        /// </summary>
        public string ToCostString()
        {
            var costs = new List<string>();
            if (money < 0) costs.Add(-money + " Money");
            if (timeWeeks < 0) costs.Add(-timeWeeks + (timeWeeks == -1 ? " week" : " weeks"));
            if (capacity < 0) costs.Add(-capacity + " Capacity");
            var gains = new List<string>();
            if (money > 0) gains.Add(money + " Money");
            if (timeWeeks > 0) gains.Add(timeWeeks + (timeWeeks == 1 ? " week" : " weeks"));
            if (capacity > 0) gains.Add(capacity + " Capacity");
            var parts = new List<string>();
            parts.Add(costs.Count > 0 ? "Cost: " + string.Join(", ", costs.ToArray()) : "No direct cost");
            if (gains.Count > 0) parts.Add("Gain: " + string.Join(", ", gains.ToArray()));
            return string.Join("  ·  ", parts.ToArray());
        }

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

        /// <summary>
        /// Detail line on a menu option: signed values with the unit after the number, e.g.
        /// "−15 Money   −1 week   −2 Capacity". Trust and Resistance Pressure only when outcomes is true
        /// (blueprint full preview). Display only.
        /// </summary>
        public string ToOptionString(bool outcomes)
        {
            var parts = new List<string>();
            if (money != 0) parts.Add(Signed(money) + " Money");
            if (timeWeeks != 0) parts.Add(Signed(timeWeeks) + (Mathf.Abs(timeWeeks) == 1 ? " week" : " weeks"));
            if (outcomes && trust != 0) parts.Add(Signed(trust) + " Trust");
            if (outcomes && resistance != 0) parts.Add(Signed(resistance) + " Resistance Pressure");
            if (capacity != 0) parts.Add(Signed(capacity) + " Capacity");
            return parts.Count == 0 ? "No direct cost" : string.Join("   ", parts.ToArray());
        }

        public static string Signed(int v)
        {
            return v > 0 ? "+" + v : (v < 0 ? "−" + (-v) : "0");
        }
    }
}

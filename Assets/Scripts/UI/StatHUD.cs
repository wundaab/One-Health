using System.Collections;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Persistent HUD keeping Money, Time, Trust and Resistance in one consistent position.
    /// Hidden until the scenario's "Show HUD" command (S01 opening: "HUD appears").
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class StatHUD : MonoBehaviour
    {
        [SerializeField] bool visibleOnStart;

        CanvasGroup group;
        StatChip[] chips;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            chips = GetComponentsInChildren<StatChip>(true);
            group.alpha = visibleOnStart ? 1f : 0f;
            group.blocksRaycasts = visibleOnStart;
        }

        void Start()
        {
            // Scenario-specific stats (e.g. S02 Clinic Capacity) appear only in scenes that track them.
            foreach (var c in chips)
            {
                if (c.Kind == StatKind.FlowchartInteger)
                    c.gameObject.SetActive(c.HasSource());
                if (c.Kind == StatKind.Capacity)
                    c.gameObject.SetActive(GameSession.ShowCapacityStat);
            }
        }

        void OnEnable()
        {
            GameSession.StatsChanged += OnStatsChanged;
            GameSession.StateReset += OnStateReset;
        }

        void OnDisable()
        {
            GameSession.StatsChanged -= OnStatsChanged;
            GameSession.StateReset -= OnStateReset;
        }

        void OnStatsChanged(StatDelta d) { foreach (var c in chips) if (c.isActiveAndEnabled) c.Refresh(true); }
        void OnStateReset()
        {
            foreach (var c in chips)
            {
                if (c.Kind == StatKind.Time) c.gameObject.SetActive(GameSession.ShowTimeStat);
                if (c.Kind == StatKind.Capacity) c.gameObject.SetActive(GameSession.ShowCapacityStat);
                if (c.isActiveAndEnabled) c.Refresh(false);
            }
        }

        /// <summary>The Resistance Pressure disclaimer is shown once, the first time the HUD appears (U16).</summary>
        static bool introducedResistance;

        public void SetVisible(bool visible)
        {
            StopAllCoroutines();
            if (isActiveAndEnabled) StartCoroutine(Fade(visible ? 1f : 0f));
            group.blocksRaycasts = visible;
            // The onboarding tour explains Resistance Pressure (including that it is not a clinical measurement),
            // so the one-time tooltip is only a fallback for scenes without the tutorial overlay.
            if (visible && !introducedResistance && TutorialOverlay.Instance == null)
            {
                foreach (var c in chips)
                {
                    if (c.Kind != StatKind.Resistance || !c.isActiveAndEnabled) continue;
                    introducedResistance = true;
                    c.ShowTooltipFor(5f);
                }
            }
        }

        IEnumerator Fade(float to)
        {
            float from = group.alpha;
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / 0.4f);
                yield return null;
            }
            group.alpha = to;
        }
    }
}

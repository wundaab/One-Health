using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Persistent HUD keeping Money, Time, Trust and Resistance in one consistent position.
    /// Hidden until the scenario's "Show HUD" command (S01 opening: "HUD appears"). On narrow screens (4:3,
    /// portrait windows) the row moves left of the Pause button and scales down only as far as it must, so chips
    /// never run off screen or under Pause. Below 1080p the row grows a little (up to 25%) so the 18 pt chip
    /// labels stay at least 14 px tall on screen (L4: readable at 720p).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class StatHUD : MonoBehaviour
    {
        [SerializeField] bool visibleOnStart;
        [Tooltip("Element at the top-right the chips must stay clear of (the Pause button)")]
        [SerializeField] RectTransform reserveRight;

        const float Gap = 16f, EdgeMargin = 20f;
        const float LabelSize = 18f, MinLabelPixels = 14f, MaxBoost = 1.25f;

        CanvasGroup group;
        StatChip[] chips;
        RectTransform rect, canvasRect;
        Canvas rootCanvas;
        HorizontalLayoutGroup row;
        Vector2 basePosition;
        float fittedWidth = -1f, fittedScaleFactor = -1f;
        int fittedCount = -1;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            chips = GetComponentsInChildren<StatChip>(true);
            rect = (RectTransform)transform;
            basePosition = rect.anchoredPosition;
            row = GetComponent<HorizontalLayoutGroup>();
            var canvas = GetComponentInParent<Canvas>();
            rootCanvas = canvas != null ? canvas.rootCanvas : null;
            canvasRect = rootCanvas != null ? (RectTransform)rootCanvas.transform : null;
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
                {
                    c.gameObject.SetActive(GameSession.ShowCapacityStat);
                    c.SetCapacityRange(GameSession.CapacityScaleMax, GameSession.CapacityLowThreshold);
                }
            }
        }

        void LateUpdate() { Fit(); }

        /// <summary>Re-fits the row when the screen width or the number of chips changes.</summary>
        void Fit()
        {
            if (canvasRect == null) return;
            float width = canvasRect.rect.width;
            int count = 0;
            float content = 0f;
            foreach (var c in chips)
            {
                if (!c.gameObject.activeSelf) continue;
                content += ((RectTransform)c.transform).rect.width;
                count++;
            }
            float pixelsPerUnit = rootCanvas.scaleFactor;
            if (Mathf.Approximately(width, fittedWidth) && Mathf.Approximately(pixelsPerUnit, fittedScaleFactor) && count == fittedCount) return;
            fittedWidth = width;
            fittedScaleFactor = pixelsPerUnit;
            fittedCount = count;
            if (row != null && count > 1) content += row.spacing * (count - 1);

            float reserve = reserveRight != null ? -reserveRight.anchoredPosition.x + reserveRight.rect.width / 2f + Gap : 0f;
            float boost = Mathf.Clamp(MinLabelPixels / (LabelSize * Mathf.Max(0.01f, pixelsPerUnit)), 1f, MaxBoost);
            float scale = boost, x = basePosition.x;
            if (content * boost > width - 2f * reserve)
            {
                // Not enough room to stay centred: use the space left of Pause, shrinking only if needed.
                float room = width - reserve - EdgeMargin;
                scale = Mathf.Min(boost, room / Mathf.Max(1f, content));
                x = (EdgeMargin - reserve) / 2f;
            }
            float top = basePosition.y + rect.rect.height / 2f;
            rect.localScale = new Vector3(scale, scale, 1f);
            rect.anchoredPosition = new Vector2(x, top - rect.rect.height / 2f * scale);
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
                if (c.Kind == StatKind.Capacity)
                {
                    c.gameObject.SetActive(GameSession.ShowCapacityStat);
                    c.SetCapacityRange(GameSession.CapacityScaleMax, GameSession.CapacityLowThreshold);
                }
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

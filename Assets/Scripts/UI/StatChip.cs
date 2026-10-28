using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneHealth
{
    public enum StatKind { Money, Time, Trust, Resistance, FlowchartInteger, Capacity }

    /// <summary>
    /// One HUD parameter: icon, label, value, optional bar, a +/- change pill and a tooltip (hover, tap or
    /// keyboard select). Changes are always shown as text and an arrow, never colour alone; spending Money,
    /// Time or Capacity is shown in a neutral colour, green and red are kept for Trust and Resistance.
    /// </summary>
    public class StatChip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] StatKind kind;
        [Tooltip("For FlowchartInteger: variable key to display, e.g. ClinicResponseCapacity")]
        [SerializeField] string variableKey = "";
        [SerializeField] int maxValue = 100;
        [Tooltip("Higher values are bad (Resistance)")]
        [SerializeField] bool higherIsWorse;
        [Tooltip("For FlowchartInteger: at or below this value the chip shows LOW")]
        [SerializeField] int lowThreshold = 30;

        [SerializeField] TMP_Text valueText;
        [SerializeField] TMP_Text statusText;
        [Tooltip("Pill behind the status text; shown with it")]
        [SerializeField] GameObject statusPill;
        [SerializeField] Image fill;
        [SerializeField] TMP_Text deltaText;
        [Tooltip("Pill behind the change text; faded with it")]
        [SerializeField] CanvasGroup deltaPill;
        [SerializeField] GameObject tooltip;
        [Tooltip("Optional icon swapped by state: normal, low, critical (a second, non-colour cue)")]
        [SerializeField] Image icon;
        [SerializeField] Sprite[] stateIcons;

        const float DeltaHold = 1.6f, DeltaFade = 0.8f, PulseTime = 0.25f;

        int shown = int.MinValue;
        float deltaTimer, pulseTimer = -1f;
        bool hovered, selected, pinned;
        Flowchart flowchart;

        /// <summary>When the last change pill appeared (unscaled time), so a card can wait until it was seen.</summary>
        public static float LastDeltaTime = -100f;

        public StatKind Kind { get { return kind; } }

        /// <summary>For FlowchartInteger chips: whether any flowchart in the scene has the variable.</summary>
        public bool HasSource()
        {
            foreach (var f in FindObjectsByType<Flowchart>())
                if (f.HasVariable(variableKey)) return true;
            return false;
        }

        void OnEnable()
        {
            hovered = selected = pinned = false;
            UpdateTooltip();
            SetDeltaAlpha(0f);
            transform.localScale = Vector3.one;
            Refresh(false);
        }

        void Update()
        {
            if (kind == StatKind.FlowchartInteger || kind == StatKind.Capacity)
                Refresh(true);

            if (deltaTimer > 0f)
            {
                deltaTimer -= Time.unscaledDeltaTime;
                // Hold at full strength, then fade, so the change is read before it goes.
                SetDeltaAlpha(Mathf.Clamp01(deltaTimer / DeltaFade));
            }

            if (pulseTimer >= 0f)
            {
                pulseTimer += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(pulseTimer / PulseTime);
                transform.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(k * Mathf.PI));
                if (k >= 1f) { pulseTimer = -1f; transform.localScale = Vector3.one; }
            }
        }

        void SetDeltaAlpha(float a)
        {
            if (deltaPill != null) deltaPill.alpha = a;
            else if (deltaText != null) deltaText.alpha = a;
        }

        int ReadValue()
        {
            var s = GameSession.State;
            switch (kind)
            {
                case StatKind.Money: return s.money;
                case StatKind.Time: return s.timeWeeks;
                case StatKind.Trust: return s.trust;
                case StatKind.Resistance: return s.resistance;
                case StatKind.Capacity:
                    // Scenario 2 keeps its own live counter; elsewhere Capacity is the campaign value.
                    foreach (var f in FindObjectsByType<Flowchart>())
                        if (f.HasVariable(FlowchartSync.ScenarioCapacityKey))
                            return f.GetIntegerVariable(FlowchartSync.ScenarioCapacityKey);
                    return s.capacity;
                default:
                    if (flowchart == null || !flowchart.HasVariable(variableKey))
                    {
                        flowchart = null;
                        foreach (var f in FindObjectsByType<Flowchart>())
                            if (f.HasVariable(variableKey)) { flowchart = f; break; }
                    }
                    return flowchart != null ? flowchart.GetIntegerVariable(variableKey) : 0;
            }
        }

        /// <summary>Re-reads the value. With showDelta, a change pops up as +N / −N.</summary>
        public void Refresh(bool showDelta)
        {
            int v = ReadValue();
            if (v == shown) return;
            if (showDelta && shown != int.MinValue)
                ShowDelta(v - shown);
            shown = v;

            valueText.text = kind == StatKind.Time ? v + (v == 1 ? " week" : " weeks") : v.ToString();
            if (fill != null)
                fill.fillAmount = Mathf.Clamp01(v / (float)Mathf.Max(1, maxValue));

            string status = "";
            bool critical = false;
            if (kind == StatKind.Trust && v < GameState.TrustCriticalBelow) { status = "Low"; critical = true; }
            if (kind == StatKind.Resistance && v > GameState.ResistanceCriticalAbove) { status = "Severe"; critical = true; }
            if (kind == StatKind.Money && v <= 0) { status = "No funds"; critical = true; }
            if (kind == StatKind.Time && v <= 0) { status = "Deadline"; critical = true; }
            bool low = (kind == StatKind.FlowchartInteger || kind == StatKind.Capacity) && v <= lowThreshold;
            if (low) { status = "Low"; critical = true; }
            if (statusText != null)
            {
                statusText.text = status;
                statusText.gameObject.SetActive(critical);
            }
            if (statusPill != null) statusPill.SetActive(critical);
            valueText.color = critical ? UITheme.NegativeOnDark : Color.white;

            if (icon != null && stateIcons != null && stateIcons.Length >= 3)
            {
                int state = !low ? 0 : (v <= lowThreshold / 2 ? 2 : 1);
                if (stateIcons[state] != null) icon.sprite = stateIcons[state];
            }
        }

        void ShowDelta(int d)
        {
            if (deltaText == null || d == 0) return;
            Color arrow;
            if (kind == StatKind.Trust || kind == StatKind.Resistance || kind == StatKind.FlowchartInteger)
            {
                bool good = higherIsWorse ? d < 0 : d > 0;
                arrow = good ? UITheme.PositiveOnDark : UITheme.NegativeOnDark;
            }
            else
            {
                arrow = UITheme.SpendOnDark;   // spending is not "bad"
            }
            deltaText.text = "<color=" + UITheme.ToHex(arrow) + ">" + (d > 0 ? "▲" : "▼") + "</color> " + StatDelta.Signed(d);
            deltaText.color = Color.white;
            deltaTimer = DeltaHold + DeltaFade;
            SetDeltaAlpha(1f);
            pulseTimer = 0f;
            LastDeltaTime = Time.unscaledTime;
        }

        // Tooltip: hover, tap/click (toggles) or keyboard selection.
        public void OnPointerEnter(PointerEventData e) { hovered = true; UpdateTooltip(); }
        public void OnPointerExit(PointerEventData e) { hovered = false; UpdateTooltip(); }
        public void OnPointerClick(PointerEventData e) { pinned = !pinned; UpdateTooltip(); }
        public void OnSelect(BaseEventData e) { selected = true; UpdateTooltip(); }
        public void OnDeselect(BaseEventData e) { selected = false; pinned = false; UpdateTooltip(); }

        /// <summary>Shows the tooltip for a few seconds without input, e.g. the Resistance Pressure disclaimer on first view.</summary>
        public void ShowTooltipFor(float seconds)
        {
            if (tooltip == null || !isActiveAndEnabled) return;
            StopAllCoroutines();
            StartCoroutine(TimedTooltip(seconds));
        }

        System.Collections.IEnumerator TimedTooltip(float seconds)
        {
            tooltip.SetActive(true);
            yield return new WaitForSecondsRealtime(seconds);
            UpdateTooltip();
        }

        void UpdateTooltip()
        {
            if (tooltip != null) tooltip.SetActive(hovered || selected || pinned);
        }
    }
}

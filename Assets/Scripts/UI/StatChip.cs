using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneHealth
{
    public enum StatKind { Money, Time, Trust, Resistance, FlowchartInteger }

    /// <summary>
    /// One HUD parameter: icon, label, value, optional bar, a +/- change popup and a one-line
    /// tooltip. Changes are always shown as text and an arrow, never colour alone.
    /// </summary>
    public class StatChip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        [SerializeField] Image fill;
        [SerializeField] TMP_Text deltaText;
        [SerializeField] GameObject tooltip;

        int shown = int.MinValue;
        float deltaTimer;
        Flowchart flowchart;

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
            if (tooltip != null) tooltip.SetActive(false);
            if (deltaText != null) deltaText.alpha = 0f;
            Refresh(false);
        }

        void Update()
        {
            if (kind == StatKind.FlowchartInteger)
                Refresh(true);

            if (deltaTimer > 0f && deltaText != null)
            {
                deltaTimer -= Time.unscaledDeltaTime;
                deltaText.alpha = Mathf.Clamp01(deltaTimer / 0.6f);
                deltaText.rectTransform.anchoredPosition = new Vector2(0f, 6f * (2.2f - deltaTimer));
            }
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
            if (kind == StatKind.FlowchartInteger && v <= lowThreshold) { status = "Low"; critical = true; }
            if (statusText != null)
            {
                statusText.text = status;
                statusText.gameObject.SetActive(critical);
            }
            valueText.color = critical ? UITheme.Coral : Color.white;
        }

        void ShowDelta(int d)
        {
            if (deltaText == null || d == 0) return;
            bool good = higherIsWorse ? d < 0 : d > 0;
            deltaText.text = (d > 0 ? "▲ " : "▼ ") + StatDelta.Signed(d);
            deltaText.color = good ? UITheme.LeafGreen : UITheme.Coral;
            deltaTimer = 2.2f;
            deltaText.alpha = 1f;
        }

        public void OnPointerEnter(PointerEventData e) { if (tooltip != null) tooltip.SetActive(true); }
        public void OnPointerExit(PointerEventData e) { if (tooltip != null) tooltip.SetActive(false); }
    }
}

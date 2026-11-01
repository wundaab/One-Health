using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Presentation of one Fungus menu option: a neutral card for every option (so colour never hints
    /// at the "right" answer), an A/B/C position marker, a lock and grey text when disabled, and a
    /// focus ring on hover or keyboard selection. Display only: Fungus still owns the label text,
    /// interactable state and click handling.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuOptionStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] Text label;
        [SerializeField] GameObject marker;
        [SerializeField] Text markerLetter;
        [SerializeField] GameObject lockIcon;
        [SerializeField] GameObject chevron;
        [SerializeField] Graphic focusRing;

        Button button;
        bool hovered, selected;
        // Sibling options, collected once (L7) for the A/B/C lettering.
        MenuOptionStyle[] siblings;

        void Awake()
        {
            button = GetComponent<Button>();
            siblings = transform.parent != null ? transform.parent.GetComponentsInChildren<MenuOptionStyle>(true) : new[] { this };
        }

        void OnDisable() { hovered = selected = false; }

        void LateUpdate()
        {
            bool on = button.interactable;
            if (label != null) label.color = on ? UITheme.Ink : UITheme.DisabledInk;
            if (marker != null) marker.SetActive(on);
            if (lockIcon != null) lockIcon.SetActive(!on);
            if (chevron != null) chevron.SetActive(on);
            if (focusRing != null) focusRing.enabled = on && (hovered || selected);
            if (markerLetter != null) markerLetter.text = ((char)('A' + VisibleIndex())).ToString();
        }

        /// <summary>Position among the options shown (hidden options leave no gap in the lettering).</summary>
        int VisibleIndex()
        {
            int index = 0;
            foreach (var sibling in siblings)
            {
                if (sibling == this) return index;
                if (sibling != null && sibling.gameObject.activeSelf) index++;
            }
            return index;
        }

        public void OnPointerEnter(PointerEventData e) { hovered = true; }
        public void OnPointerExit(PointerEventData e) { hovered = false; }
        public void OnSelect(BaseEventData e) { selected = true; }
        public void OnDeselect(BaseEventData e) { selected = false; }
    }
}

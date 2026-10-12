using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>Small hover lift and a click sound (respecting the SFX toggle) for menu buttons.</summary>
    [RequireComponent(typeof(Selectable))]
    public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] AudioClip clickSound;
        [SerializeField] float hoverScale = 1.04f;
        [SerializeField] float speed = 14f;

        Selectable selectable;
        Vector3 baseScale;
        float target = 1f;

        void Awake()
        {
            selectable = GetComponent<Selectable>();
            baseScale = transform.localScale;
        }

        void OnDisable()
        {
            target = 1f;
            transform.localScale = baseScale;
        }

        void Update()
        {
            float current = transform.localScale.x / Mathf.Max(0.0001f, baseScale.x);
            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = baseScale * next;
        }

        public void OnPointerEnter(PointerEventData e) { if (selectable.IsInteractable()) target = hoverScale; }
        public void OnPointerExit(PointerEventData e) { target = 1f; }
        public void OnSelect(BaseEventData e) { if (selectable.IsInteractable()) target = hoverScale; }
        public void OnDeselect(BaseEventData e) { target = 1f; }

        public void OnPointerClick(PointerEventData e)
        {
            if (selectable.IsInteractable())
                AudioPreferences.PlayUISound(clickSound);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Sizes the Fungus menu options to their content (one, two or three lines; all options in a set
    /// share the tallest height so the set reads as one list) and fades in the scrim behind them.
    /// </summary>
    public class MenuDialogLayout : MonoBehaviour
    {
        [SerializeField] RectTransform buttonGroup;
        [SerializeField] CanvasGroup scrim;
        [SerializeField] float scrimFade = 0.2f;
        [SerializeField] float padding = 30f;
        [SerializeField] float minHeight = 88f;
        [SerializeField] float maxHeight = 150f;

        // Option labels and layout elements, collected once (L7) instead of searched every frame.
        Text[] labels;
        LayoutElement[] elements;

        void Awake()
        {
            if (buttonGroup == null) return;
            int n = buttonGroup.childCount;
            labels = new Text[n];
            elements = new LayoutElement[n];
            for (int i = 0; i < n; i++)
            {
                var option = buttonGroup.GetChild(i);
                labels[i] = option.GetComponentInChildren<Text>(true);
                elements[i] = option.GetComponent<LayoutElement>();
            }
        }

        void OnEnable()
        {
            if (scrim != null) scrim.alpha = 0f;
        }

        void LateUpdate()
        {
            if (scrim != null && scrim.alpha < 1f)
                scrim.alpha = Mathf.MoveTowards(scrim.alpha, 1f, Time.unscaledDeltaTime / Mathf.Max(0.01f, scrimFade));
            if (buttonGroup == null || labels == null) return;

            // Height of the tallest label at full size (long one-line actions can wrap), plus padding.
            float textHeight = 0f;
            for (int i = 0; i < labels.Length; i++)
            {
                var text = labels[i];
                if (text == null || !text.transform.parent.gameObject.activeSelf) continue;
                textHeight = Mathf.Max(textHeight, PreferredHeight(text));
            }
            float h = Mathf.Clamp(textHeight + padding, minHeight, maxHeight);
            foreach (var le in elements)
                if (le != null && Mathf.Abs(le.preferredHeight - h) > 0.5f) le.preferredHeight = h;
        }

        static float PreferredHeight(Text text)
        {
            var settings = text.GetGenerationSettings(new Vector2(text.rectTransform.rect.width, 0f));
            settings.resizeTextForBestFit = false;
            settings.fontSize = text.resizeTextForBestFit ? text.resizeTextMaxSize : text.fontSize;
            return text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, settings) / text.pixelsPerUnit;
        }
    }
}

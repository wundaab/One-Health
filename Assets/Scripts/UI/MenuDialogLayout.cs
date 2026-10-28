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

        void OnEnable()
        {
            if (scrim != null) scrim.alpha = 0f;
        }

        void LateUpdate()
        {
            if (scrim != null && scrim.alpha < 1f)
                scrim.alpha = Mathf.MoveTowards(scrim.alpha, 1f, Time.unscaledDeltaTime / Mathf.Max(0.01f, scrimFade));
            if (buttonGroup == null) return;

            // Height of the tallest label at full size (long one-line actions can wrap), plus padding.
            float textHeight = 0f;
            for (int i = 0; i < buttonGroup.childCount; i++)
            {
                var option = buttonGroup.GetChild(i);
                if (!option.gameObject.activeSelf) continue;
                var text = option.GetComponentInChildren<Text>();
                if (text != null) textHeight = Mathf.Max(textHeight, PreferredHeight(text));
            }
            float h = Mathf.Clamp(textHeight + padding, minHeight, maxHeight);
            for (int i = 0; i < buttonGroup.childCount; i++)
            {
                var le = buttonGroup.GetChild(i).GetComponent<LayoutElement>();
                if (le != null && Mathf.Abs(le.preferredHeight - h) > 0.5f) le.preferredHeight = h;
            }
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

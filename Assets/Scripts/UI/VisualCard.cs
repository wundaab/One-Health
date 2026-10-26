using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>Concept card: one idea, one visual, short caption, Continue.</summary>
    public class VisualCard : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] TMP_Text title;
        [SerializeField] Image image;
        [SerializeField] TMP_Text caption;
        [SerializeField] Button continueButton;

        System.Action onContinue;
        Vector2 maxImageSize;
        Rect captionDefault;

        // Re-attached on every Show (remove-then-add keeps exactly one listener), so Continue always
        // works however and whenever the card object was activated.
        void Wire()
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        void OnContinueClicked()
        {
            panel.Close();
            var done = onContinue;
            onContinue = null;
            if (done != null) done();
        }

        public void Show(string titleText, Sprite sprite, string captionText, System.Action continued)
        {
            Wire();
            title.text = titleText;
            image.sprite = sprite;
            image.gameObject.SetActive(sprite != null);
            if (sprite != null)
            {
                // Fit inside the image area, but never upscale small icons more than 1.5x (keeps them crisp).
                if (maxImageSize == Vector2.zero) maxImageSize = image.rectTransform.sizeDelta;
                var native = sprite.rect.size;
                float scale = Mathf.Min(maxImageSize.x / native.x, maxImageSize.y / native.y, 1.5f);
                image.rectTransform.sizeDelta = native * scale;
            }
            // Text-only cards (title cards, decision review) use the image area for the caption.
            if (captionDefault.size == Vector2.zero)
                captionDefault = new Rect(caption.rectTransform.anchoredPosition, caption.rectTransform.sizeDelta);
            bool textOnly = sprite == null;
            caption.rectTransform.anchoredPosition = textOnly ? new Vector2(0, 30) : captionDefault.position;
            caption.rectTransform.sizeDelta = textOnly ? new Vector2(captionDefault.width, 520) : captionDefault.size;
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 18;
            caption.fontSizeMax = textOnly ? 34 : 30;
            caption.text = captionText;
            onContinue = continued;
            panel.Open();
            continueButton.Select();
        }
    }
}

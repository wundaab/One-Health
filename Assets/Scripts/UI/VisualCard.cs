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

        void Awake()
        {
            continueButton.onClick.AddListener(() =>
            {
                panel.Close();
                var done = onContinue;
                onContinue = null;
                if (done != null) done();
            });
        }

        public void Show(string titleText, Sprite sprite, string captionText, System.Action continued)
        {
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
            caption.text = captionText;
            onContinue = continued;
            panel.Open();
            continueButton.Select();
        }
    }
}

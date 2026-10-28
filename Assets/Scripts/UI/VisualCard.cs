using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Concept card: one idea, one visual, short caption, Continue. The card fits its content: a large
    /// image, a small icon (badges, with a short pop), text only, or a scenario title shown as a chapter
    /// title on the dimmed scene. Titles written "Eyebrow — Title" show the first part as a small label.
    /// </summary>
    public class VisualCard : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] TMP_Text eyebrow;
        [SerializeField] TMP_Text title;
        [SerializeField] Image image;
        [SerializeField] Image imageWell;
        [SerializeField] GameObject sparkles;
        [SerializeField] TMP_Text caption;
        [SerializeField] Button continueButton;
        [SerializeField] Image card;
        [SerializeField] Image dim;

        System.Action onContinue;
        Color dimDefault;
        bool captured;

        /// <summary>The title as authored (eyebrow and title together), e.g. for logs.</summary>
        public string FullTitle { get; private set; }

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

        enum Layout { LargeImage, SmallIcon, TextOnly, Chapter }

        public void Show(string titleText, Sprite sprite, string captionText, System.Action continued)
        {
            Wire();
            if (!captured) { dimDefault = dim != null ? dim.color : Color.clear; captured = true; }
            FullTitle = titleText;

            // "Badge earned — Evidence Tracker" -> eyebrow "BADGE EARNED", title "Evidence Tracker". Same words.
            string head = "", main = titleText ?? "";
            int k = main.IndexOf(" — ");
            if (k > 0) { head = main.Substring(0, k); main = main.Substring(k + 3); }
            eyebrow.text = head.ToUpperInvariant();
            eyebrow.gameObject.SetActive(head.Length > 0);
            title.text = main;
            caption.text = captionText;

            bool small = sprite != null && sprite.rect.width <= 256 && sprite.rect.height <= 256;
            Layout layout = sprite == null
                ? (head.StartsWith("Scenario") ? Layout.Chapter : Layout.TextOnly)
                : (small ? Layout.SmallIcon : Layout.LargeImage);
            Arrange(layout, sprite, head.Length > 0);

            onContinue = continued;
            // A decision's change pills appear just before many cards: wait until they have been seen
            // (about 0.6 s) rather than dimming them straight away (U14).
            float wait = 0.6f - (Time.unscaledTime - StatChip.LastDeltaTime);
            if (wait > 0f)
            {
                gameObject.SetActive(true);
                var group = GetComponent<CanvasGroup>();
                group.alpha = 0f;
                group.blocksRaycasts = false;
                StartCoroutine(OpenAfter(wait, layout));
            }
            else
            {
                Open(layout);
            }
        }

        IEnumerator OpenAfter(float seconds, Layout layout)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (onContinue != null) Open(layout);
        }

        void Open(Layout layout)
        {
            panel.Open();
            continueButton.Select();
            if (layout == Layout.SmallIcon) StartCoroutine(Pop());
        }

        void Arrange(Layout layout, Sprite sprite, bool hasEyebrow)
        {
            float h = layout == Layout.LargeImage ? 820f : layout == Layout.SmallIcon ? 640f : layout == Layout.TextOnly ? 560f : 520f;
            float top = h / 2f;
            var content = (RectTransform)card.transform;
            content.sizeDelta = new Vector2(1080f, h);

            bool chapter = layout == Layout.Chapter;
            card.enabled = !chapter;
            if (dim != null) dim.color = chapter ? UITheme.WithAlpha(UITheme.TealDark, 0.82f) : dimDefault;
            title.color = chapter ? Color.white : UITheme.Ink;
            title.fontSize = chapter ? 64f : 44f;
            eyebrow.color = chapter ? UITheme.TealLight : UITheme.Teal;
            caption.color = chapter ? UITheme.Cream : UITheme.Ink;

            float eyebrowY = chapter ? 110f : top - 54f;
            float titleY = chapter ? 46f : (hasEyebrow ? top - 98f : top - 76f);
            Place(eyebrow.rectTransform, eyebrowY, 34f);
            Place(title.rectTransform, titleY, chapter ? 84f : 60f);

            image.sprite = sprite;
            image.gameObject.SetActive(sprite != null);
            imageWell.gameObject.SetActive(layout == Layout.LargeImage);
            sparkles.SetActive(layout == Layout.SmallIcon);
            float imageY = 0f;
            if (layout == Layout.LargeImage)
            {
                imageY = top - 350f;
                Fit(sprite, new Vector2(900f, 400f), 1.5f);
                var size = image.rectTransform.sizeDelta + new Vector2(24f, 24f);
                imageWell.rectTransform.sizeDelta = size;
                imageWell.rectTransform.anchoredPosition = new Vector2(0f, imageY);
            }
            else if (layout == Layout.SmallIcon)
            {
                imageY = top - 250f;
                Fit(sprite, new Vector2(200f, 200f), 1.5f);
                ((RectTransform)sparkles.transform).anchoredPosition = new Vector2(0f, imageY);
            }
            image.rectTransform.anchoredPosition = new Vector2(0f, imageY);

            caption.enableAutoSizing = true;
            caption.fontSizeMin = 18;
            switch (layout)
            {
                case Layout.LargeImage: caption.fontSizeMax = 30; Place(caption.rectTransform, -top + 206f, 112f); break;
                case Layout.SmallIcon: caption.fontSizeMax = 30; Place(caption.rectTransform, -top + 196f, 110f); break;
                case Layout.TextOnly: caption.fontSizeMax = 32; Place(caption.rectTransform, 6f, h - 300f); break;
                default: caption.fontSizeMax = 30; Place(caption.rectTransform, -86f, 130f); break;
            }
            caption.alignment = chapter ? TextAlignmentOptions.Top : TextAlignmentOptions.Center;
            ((RectTransform)continueButton.transform).anchoredPosition = new Vector2(0f, chapter ? -230f : -top + 82f);
        }

        void Fit(Sprite sprite, Vector2 area, float maxScale)
        {
            // Fit inside the area, but never upscale small icons more than maxScale (keeps them crisp).
            var native = sprite.rect.size;
            float scale = Mathf.Min(area.x / native.x, area.y / native.y, maxScale);
            image.rectTransform.sizeDelta = native * scale;
        }

        static void Place(RectTransform rt, float y, float height)
        {
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        /// <summary>Badge pop: 0.6 to 1.0 with a little overshoot (ease-out-back), 0.3 s.</summary>
        IEnumerator Pop()
        {
            var t = image.rectTransform;
            for (float e = 0f; e < 0.3f; e += Time.unscaledDeltaTime)
            {
                float x = e / 0.3f - 1f;
                float back = 1f + 2.70158f * x * x * x + 1.70158f * x * x;
                t.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, back);
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}

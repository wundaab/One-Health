using Fungus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>
    /// Presentation of the Fungus say dialog: the speaker's name always white on a navy tab that grows
    /// with the name (hidden for narration), and the speaker's portrait as a circular bust at the left of
    /// the box. Full-body sprites are cropped to head and shoulders by the circle mask; square icons
    /// (sector placeholders) fill the circle. Text margins follow so text never runs under the portrait.
    /// Display only: Fungus still sets the name, the portrait sprite and the text.
    /// </summary>
    public class SayDialogStyle : MonoBehaviour
    {
        [SerializeField] Text nameText;
        [SerializeField] GameObject nameTab;
        [SerializeField] RectTransform nameTabRect;
        [SerializeField] GameObject portraitSlot;
        [SerializeField] Image characterImage;
        [SerializeField] RectTransform portraitMask;
        [SerializeField] TMP_Text storyText;

        [SerializeField] float tabX = 36f;
        [SerializeField] float tabXWithPortrait = 214f;
        [SerializeField] float textLeft = 48f;
        [SerializeField] float textLeftWithPortrait = 214f;

        Sprite fitted;
        SayDialog sayDialog;

        void Awake() { sayDialog = GetComponent<SayDialog>(); }

        void LateUpdate()
        {
            if (nameText != null)
            {
                nameText.color = Color.white;   // names never rely on per-character colours for legibility
                if (nameTab != null) nameTab.SetActive(!string.IsNullOrEmpty(nameText.text));
            }

            var sprite = characterImage != null ? (characterImage.overrideSprite != null ? characterImage.overrideSprite : characterImage.sprite) : null;
            bool portrait = characterImage != null && characterImage.gameObject.activeSelf && sprite != null;

            // A named speaker who is not standing on the stage still gets a face: their first portrait as a bust.
            // (Scenarios 1 and 2 stage most speakers; Samuel and others talk from off stage.)
            var who = sayDialog != null ? sayDialog.SpeakingCharacter : null;
            if (!portrait && characterImage != null && who != null && !who.State.onScreen
                && who.Portraits != null && who.Portraits.Count > 0 && who.Portraits[0] != null)
            {
                characterImage.overrideSprite = who.Portraits[0];
                characterImage.gameObject.SetActive(true);
                sprite = who.Portraits[0];
                portrait = true;
            }
            if (portraitSlot != null && portraitSlot.activeSelf != portrait) portraitSlot.SetActive(portrait);
            if (portrait && sprite != fitted) Fit(sprite);

            if (nameTabRect != null)
                nameTabRect.anchoredPosition = new Vector2(portrait ? tabXWithPortrait : tabX, nameTabRect.anchoredPosition.y);
            if (storyText != null)
            {
                var m = storyText.margin;
                float left = portrait ? textLeftWithPortrait : textLeft;
                if (!Mathf.Approximately(m.x, left)) storyText.margin = new Vector4(left, m.y, m.z, m.w);
            }
        }

        /// <summary>Head-and-shoulders crop for tall full-body sprites; cover-fit for everything else.</summary>
        void Fit(Sprite sprite)
        {
            fitted = sprite;
            var rt = characterImage.rectTransform;
            float d = portraitMask.rect.width;
            var size = sprite.rect.size;
            characterImage.preserveAspect = false;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            if (size.y / size.x > 1.6f)
            {
                // Full figure: about 1.3 circle-widths wide, top of the head just inside the circle.
                float w = d * 1.3f;
                rt.sizeDelta = new Vector2(w, w * size.y / size.x);
                rt.anchoredPosition = new Vector2(0f, -d * 0.06f);
            }
            else
            {
                float s = d / Mathf.Min(size.x, size.y);
                rt.sizeDelta = size * s;
                rt.anchoredPosition = Vector2.zero;
            }
        }
    }
}

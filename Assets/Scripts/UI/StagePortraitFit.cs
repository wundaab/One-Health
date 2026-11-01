using Fungus;
using UnityEngine;

namespace OneHealth
{
    /// <summary>
    /// Keeps Fungus stage portraits clear of the rest of the UI at every screen shape (QA S3). Portraits stand
    /// in the band between the HUD (with its change pills) and the say dialog: their height follows that band,
    /// and Left/Right move inwards on narrow screens so figures stay on screen. While options, a concept card or
    /// the results screen are up, the stage fades out (those sit in the same band); it fades back for dialogue.
    /// The speaker then appears as a bust in the say dialog instead. Display only: Fungus still decides who is
    /// on stage and where.
    /// </summary>
    public class StagePortraitFit : MonoBehaviour
    {
        [SerializeField] Stage stage;

        [Header("Game UI layout, in GameUI units (1920 x 1080 reference, match 0.5)")]
        [Tooltip("Lowest point of the HUD, including the change pills under the chips (and the HUD's up-to-25% growth below 1080p)")]
        [SerializeField] float hudBottom = 210f;
        [Tooltip("Highest point of the say dialog: box top (314) plus the portrait circle above it")]
        [SerializeField] float dialogTop = 350f;
        [SerializeField] float margin = 12f;

        [Header("Stage")]
        [Tooltip("Largest portrait height, in stage canvas units (the Fungus default position size)")]
        [SerializeField] float maxHeight = 820f;
        [Tooltip("Half a figure's width as a share of its height, used to keep Left/Right figures on screen")]
        [SerializeField] float halfWidthRatio = 0.25f;
        [SerializeField] float fadeTime = 0.2f;

        /// <summary>True while the stage is faded out for a menu, card or results screen.</summary>
        public static bool Hidden { get; private set; }

        Canvas canvas;
        RectTransform canvasRect;
        CanvasGroup group;
        RectTransform[] positions;
        float[] baseX;
        float[] fittedX;
        Vector2 fittedSize = new Vector2(-1f, -1f);
        VisualCard card;
        ResultsPanel results;

        void Awake()
        {
            if (stage == null) stage = GetComponent<Stage>();
            if (stage == null || stage.PortraitCanvas == null) { enabled = false; return; }
            canvas = stage.PortraitCanvas;
            canvasRect = (RectTransform)canvas.transform;
            group = canvas.GetComponent<CanvasGroup>();
            if (group == null) group = canvas.gameObject.AddComponent<CanvasGroup>();
            // The portrait canvas also hosts interactive UI (Scenario 2's case board), so it must keep receiving
            // clicks. It sorts below the dialogue, menu and HUD canvases, so portraits never block those.
            group.blocksRaycasts = true;
            positions = stage.Positions.ToArray();
            baseX = new float[positions.Length];
            fittedX = new float[positions.Length];
            for (int i = 0; i < positions.Length; i++)
                fittedX[i] = baseX[i] = positions[i] != null ? positions[i].anchoredPosition.x : 0f;
        }

        void Start()
        {
            card = FindAnyObjectByType<VisualCard>(FindObjectsInactive.Include);
            results = FindAnyObjectByType<ResultsPanel>(FindObjectsInactive.Include);
        }

        void OnDestroy() { Hidden = false; }

        void LateUpdate()
        {
            if (canvasRect.rect.size != fittedSize) Fit();

            var menu = MenuDialog.ActiveMenuDialog;
            bool hide = (menu != null && menu.isActiveAndEnabled && menu.DisplayedOptionsCount > 0)
                || (card != null && card.gameObject.activeInHierarchy)
                || (results != null && results.gameObject.activeInHierarchy);
            Hidden = hide;
            float target = hide ? 0f : 1f;
            if (!Mathf.Approximately(group.alpha, target))
                group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeTime));
        }

        /// <summary>Sizes and places the stage positions (and portraits already on stage) for this screen.</summary>
        void Fit()
        {
            fittedSize = canvasRect.rect.size;
            float stageScale = Mathf.Max(0.0001f, canvas.scaleFactor);
            Vector2 pixels = fittedSize * stageScale;
            // GameUI and the dialogs scale with the screen (1920 x 1080, match 0.5): UI units -> stage units.
            float uiScale = Mathf.Sqrt(pixels.x / 1920f * (pixels.y / 1080f));
            float k = uiScale / stageScale;

            float bottom = (dialogTop + margin) * k;                    // from the bottom of the screen
            float top = fittedSize.y - (hudBottom + margin) * k;        // from the bottom of the screen
            float height = Mathf.Clamp(top - bottom, 0f, maxHeight);
            float y = bottom - fittedSize.y;                             // positions hang from the top edge, pivot at the feet
            float halfWidth = fittedSize.x / 2f;
            float inner = Mathf.Max(0f, halfWidth - height * halfWidthRatio - margin * k);
            var previousX = (float[])fittedX.Clone();

            for (int i = 0; i < positions.Length; i++)
            {
                var p = positions[i];
                if (p == null) continue;
                float x = baseX[i];
                // Offscreen positions stay just past the edge; on-screen ones move in if the screen is narrow.
                x = Mathf.Abs(x) >= 1000f ? Mathf.Sign(x) * (halfWidth + height) : Mathf.Sign(x) * Mathf.Min(Mathf.Abs(x), inner);
                fittedX[i] = x;
                p.anchoredPosition = new Vector2(x, y);
                p.sizeDelta = new Vector2(height, height);
            }

            // Portraits already on stage copied the old position rects when they were shown.
            foreach (Transform child in canvasRect)
            {
                if (!child.name.EndsWith(" holder")) continue;
                var holder = (RectTransform)child;
                holder.sizeDelta = new Vector2(height, height);
                holder.anchoredPosition = new Vector2(NearestFittedX(holder.anchoredPosition.x, previousX), y);
            }
        }

        /// <summary>A portrait sits at a position's previous x (its authored x or the last fit): move it with that position.</summary>
        float NearestFittedX(float x, float[] previousX)
        {
            int best = -1;
            float distance = float.MaxValue;
            for (int i = 0; i < positions.Length; i++)
            {
                if (positions[i] == null) continue;
                float d = Mathf.Min(Mathf.Abs(baseX[i] - x), Mathf.Abs(previousX[i] - x));
                if (d < distance) { distance = d; best = i; }
            }
            return best >= 0 ? fittedX[best] : x;
        }
    }
}

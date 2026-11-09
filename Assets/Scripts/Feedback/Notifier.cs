using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    public enum NoticeKind { Discovery, Task, Positive, Warning, Milestone }

    /// <summary>
    /// Shared toast notifications (Enhancement Plan §3): short, specific messages for discoveries, progress,
    /// parameter changes and warnings. One at a time, queued and staggered, shown under the HUD so they never
    /// cover dialogue or options. They wait while a tip, card or the pause menu is open and are dropped when the
    /// results screen opens (it lists the same outcomes). Each kind has its own accent colour and sound cue.
    /// </summary>
    public class Notifier : MonoBehaviour
    {
        struct Notice { public NoticeKind kind; public string title, message; }

        const float HudBottom = 222f, FadeIn = 0.25f, Hold = 2.6f, FadeOut = 0.3f, Gap = 0.2f;

        static Notifier instance;
        readonly Queue<Notice> queue = new Queue<Notice>();
        CanvasGroup group;
        RectTransform toast;
        Image accent;
        TMP_Text titleText, messageText;
        ResultsPanel results;
        VisualCard card;
        string current;
        bool showing;

        public static void Show(NoticeKind kind, string title, string message = "")
        {
            if (!Application.isPlaying) return;
            if (instance == null) Create();
            if (instance.showing && instance.current == title + message) return;
            foreach (var n in instance.queue)
                if (n.title == title && n.message == message) return;   // the same notice is already waiting
            instance.queue.Enqueue(new Notice { kind = kind, title = title, message = message });
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            // Trust changes are announced for every scenario from one place.
            GameSession.StatsChanged -= OnStatsChanged;
            GameSession.StatsChanged += OnStatsChanged;
        }

        static void OnStatsChanged(StatDelta d)
        {
            if (d.trust > 0) Show(NoticeKind.Positive, "Community trust is growing.");
            else if (d.trust < 0) Show(NoticeKind.Warning, "Community trust has dropped.", "Engagement and honest communication can rebuild it.");
        }

        static void Create()
        {
            var go = new GameObject("Notifier", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;   // over the game UI, under the pause menu (999)
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            instance = go.AddComponent<Notifier>();
            instance.Build();
        }

        void Build()
        {
            toast = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            toast.SetParent(transform, false);
            toast.anchorMin = toast.anchorMax = toast.pivot = new Vector2(0.5f, 1f);
            toast.anchoredPosition = new Vector2(0f, -HudBottom);
            toast.GetComponent<Image>().color = UITheme.WithAlpha(UITheme.SurfaceDark, 0.96f);
            group = toast.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            var row = toast.GetComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(0, 28, 0, 0);
            row.spacing = 20f;
            row.childControlWidth = row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = true;
            var fit = toast.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            accent = new GameObject("Accent", typeof(RectTransform), typeof(Image), typeof(LayoutElement)).GetComponent<Image>();
            accent.transform.SetParent(toast, false);
            accent.raycastTarget = false;
            accent.GetComponent<LayoutElement>().preferredWidth = 10f;

            var column = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<VerticalLayoutGroup>();
            column.transform.SetParent(toast, false);
            column.padding = new RectOffset(0, 0, 16, 16);
            column.spacing = 2f;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandHeight = false;

            var font = FindFont();
            titleText = Label(column.transform, font, 28f, FontStyles.Bold);
            messageText = Label(column.transform, font, 22f, FontStyles.Normal);
        }

        static TMP_Text Label(Transform parent, TMP_FontAsset font, float size, FontStyles style)
        {
            var t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(ContentSizeFitter)).GetComponent<TextMeshProUGUI>();
            t.transform.SetParent(parent, false);
            if (font != null) t.font = font;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = Color.white;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            return t;
        }

        /// <summary>The game's UI font, taken from the HUD so toasts match it.</summary>
        static TMP_FontAsset FindFont()
        {
            var chip = FindAnyObjectByType<StatChip>(FindObjectsInactive.Include);
            var text = chip != null ? chip.GetComponentInChildren<TMP_Text>(true) : FindAnyObjectByType<TMP_Text>(FindObjectsInactive.Include);
            return text != null ? text.font : TMP_Settings.defaultFontAsset;
        }

        void Update()
        {
            if (results == null) results = FindAnyObjectByType<ResultsPanel>(FindObjectsInactive.Include);
            if (results != null && results.isActiveAndEnabled) { queue.Clear(); return; }
            if (showing || queue.Count == 0) return;
            var tutorial = TutorialOverlay.Instance;
            if ((tutorial != null && tutorial.IsOpen) || ModalStack.Top != null) return;
            // Concept and badge cards are focused moments with their own sound: wait until they close.
            if (card == null) card = FindAnyObjectByType<VisualCard>(FindObjectsInactive.Include);
            if (card != null && card.isActiveAndEnabled) return;
            StartCoroutine(Play(queue.Dequeue()));
        }

        IEnumerator Play(Notice n)
        {
            showing = true;
            current = n.title + n.message;
            accent.color = Accent(n.kind);
            titleText.text = n.title;
            messageText.text = n.message;
            messageText.gameObject.SetActive(!string.IsNullOrEmpty(n.message));
            UICues.Play(CueFor(n.kind));
            var rest = new Vector2(0f, -HudBottom);
            for (float t = 0f; t < FadeIn; t += Time.unscaledDeltaTime)
            {
                float k = t / FadeIn;
                group.alpha = k;
                toast.anchoredPosition = rest + new Vector2(0f, 16f * (1f - k));
                yield return null;
            }
            group.alpha = 1f;
            toast.anchoredPosition = rest;
            for (float t = 0f; t < Hold; t += Time.unscaledDeltaTime)
            {
                if (results != null && results.isActiveAndEnabled) break;
                yield return null;
            }
            for (float t = 0f; t < FadeOut; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / FadeOut;
                yield return null;
            }
            group.alpha = 0f;
            yield return new WaitForSecondsRealtime(Gap);
            showing = false;
        }

        static Color Accent(NoticeKind kind)
        {
            switch (kind)
            {
                case NoticeKind.Discovery: return UITheme.TealLight;
                case NoticeKind.Positive: return UITheme.LeafGreen;
                case NoticeKind.Warning:
                case NoticeKind.Milestone: return UITheme.Amber;
                default: return UITheme.Teal;
            }
        }

        static Cue CueFor(NoticeKind kind)
        {
            switch (kind)
            {
                case NoticeKind.Discovery: return Cue.Discovery;
                case NoticeKind.Warning: return Cue.Warning;
                case NoticeKind.Milestone: return Cue.Achievement;
                default: return Cue.Confirm;
            }
        }
    }
}

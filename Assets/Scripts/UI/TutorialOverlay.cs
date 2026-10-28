using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OneHealth
{
    /// <summary>
    /// Onboarding overlay: dims the screen around one element (a HUD chip, the options, the Pause
    /// button), rings it, and explains it in a card with Back / Next / Skip tips. Sequences come from
    /// <see cref="Tutorial"/>. "hud" is started by the Show Tutorial command in Scenario 1; the others
    /// start by themselves the first time they apply (first costed decision, first locked option,
    /// first time Capacity appears), once nothing else (card, results, pause) is on screen. A
    /// full-screen blocker under the card stops clicks reaching the highlighted element.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class TutorialOverlay : MonoBehaviour
    {
        [SerializeField] RectTransform dimTop;
        [SerializeField] RectTransform dimBottom;
        [SerializeField] RectTransform dimLeft;
        [SerializeField] RectTransform dimRight;
        [SerializeField] RectTransform ring;
        [SerializeField] RectTransform callout;
        [SerializeField] TMP_Text stepLabel;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text body;
        [SerializeField] Button backButton;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] Button skipButton;

        const float HolePadding = 12f;
        const float Margin = 28f;
        const float CalloutWidth = 720f;
        /// <summary>Wait for a menu's layout to settle (and the player to see it) before pointing at it.</summary>
        const float MenuDelay = 0.45f;

        public static TutorialOverlay Instance { get; private set; }
        public bool IsOpen { get { return steps != null; } }
        public string CurrentTitle { get { return title.text; } }

        /// <summary>A tip is about to open for the menu on screen (lets automated playthroughs wait for it).</summary>
        public bool TipPending { get { return !IsOpen && (pendingChoices || pendingLocked); } }

        CanvasGroup group;
        RectTransform area;
        TutorialStep[] steps;
        string sequence;
        int index;
        Action done;
        GameObject previousSelection;
        RectTransform target;
        bool pendingChoices, pendingLocked;
        float menuVisibleSince = -1f;
        bool wired;

        void Awake()
        {
            Instance = this;
            group = GetComponent<CanvasGroup>();
            area = (RectTransform)transform;
            Wire();
            // Stays active (so it can watch for its triggers) but invisible and click-through until a tip opens.
            group.alpha = 0f;
            group.blocksRaycasts = group.interactable = false;
        }

        void OnEnable() { CostedOption.OptionShown += NoteOptionShown; }
        void OnDisable() { CostedOption.OptionShown -= NoteOptionShown; }

        void Wire()
        {
            if (wired) return;
            wired = true;
            backButton.onClick.AddListener(() => Go(index - 1));
            nextButton.onClick.AddListener(() => Go(index + 1));
            skipButton.onClick.AddListener(() => { Tutorial.SkipAll(); Close(); });
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsOpen) Tutorial.IsOpen = false;
        }

        void NoteOptionShown(bool enabled)
        {
            if (!Tutorial.Seen(Tutorial.Choices)) pendingChoices = true;
            if (!enabled && !Tutorial.Seen(Tutorial.Locked)) pendingLocked = true;
        }

        void CheckTriggers()
        {
            if (IsOpen || !QuietScreen()) { menuVisibleSince = -1f; return; }

            var menu = Fungus.MenuDialog.ActiveMenuDialog;
            bool menuShowing = menu != null && menu.isActiveAndEnabled && menu.DisplayedOptionsCount > 0;
            if (!menuShowing) { pendingChoices = pendingLocked = false; menuVisibleSince = -1f; }
            else if (menuVisibleSince < 0f) menuVisibleSince = Time.unscaledTime;

            if (menuShowing && Time.unscaledTime - menuVisibleSince >= MenuDelay)
            {
                if (pendingChoices) { pendingChoices = false; Show(Tutorial.Choices, null); return; }
                if (pendingLocked && FindLockedOption() != null) { pendingLocked = false; Show(Tutorial.Locked, null); return; }
            }

            if (!Tutorial.Seen(Tutorial.Capacity) && !menuShowing && HudVisible())
            {
                var chip = FindChip(StatKind.Capacity);
                if (chip != null) Show(Tutorial.Capacity, null);
            }
        }

        /// <summary>Shows a sequence unless it was already seen; calls done when it closes (or at once).</summary>
        public void Show(string id, Action onDone)
        {
            if (Tutorial.Seen(id) || IsOpen)
            {
                if (onDone != null) onDone();
                return;
            }
            sequence = id;
            steps = Tutorial.Steps(id);
            done = onDone;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            Tutorial.IsOpen = true;
            SetDialogInput(false);
            group.blocksRaycasts = group.interactable = true;
            StopAllCoroutines();
            StartCoroutine(Fade(0f, 1f));
            Go(0);
        }

        void Go(int i)
        {
            // Skip steps whose element is not on screen in this scene (e.g. a hidden chip).
            int dir = i < index ? -1 : 1;
            while (i >= 0 && i < steps.Length && steps[i].target != TutorialTarget.None && Resolve(steps[i].target) == null) i += dir;
            if (i >= steps.Length) { Tutorial.MarkSeen(sequence); Close(); return; }
            if (i < 0) i = 0;
            index = i;

            var step = steps[i];
            target = step.target == TutorialTarget.None ? null : Resolve(step.target);
            int shown = steps.Length;
            stepLabel.text = shown > 1
                ? Tutorial.Heading(sequence).ToUpperInvariant() + "  ·  " + (i + 1) + " OF " + shown
                : Tutorial.Heading(sequence).ToUpperInvariant();
            title.text = step.title;
            body.text = step.body;
            backButton.gameObject.SetActive(i > 0);
            bool last = i == steps.Length - 1;
            nextLabel.text = last ? (sequence == Tutorial.Hud ? "Let’s start" : "Got it") : "Next";
            // A single-tip sequence needs no "Skip tips"; "Got it" already closes it.
            skipButton.gameObject.SetActive(steps.Length > 1 && !last);
            Layout();
            nextButton.Select();
        }

        void Close()
        {
            if (sequence != null) Tutorial.MarkSeen(sequence);
            steps = null;
            target = null;
            Tutorial.IsOpen = false;
            SetDialogInput(true);
            var cb = done;
            done = null;
            if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousSelection);
            group.blocksRaycasts = group.interactable = false;
            StopAllCoroutines();
            if (isActiveAndEnabled) StartCoroutine(Fade(group.alpha, 0f));
            if (cb != null) cb();
        }

        void Update()
        {
            if (!IsOpen) { CheckTriggers(); return; }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
#else
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
#endif
        }

        void LateUpdate()
        {
            if (!IsOpen) return;
            Layout();   // follows targets that animate or resize (menu heights, chip widths)
            float pulse = 1f + 0.025f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f));
            ring.localScale = new Vector3(pulse, pulse, 1f);
        }

        // ------------------------------------------------------------------ layout

        void Layout()
        {
            Rect full = area.rect;
            Rect hole;
            if (target != null && TryLocalRect(target, out hole))
            {
                hole = new Rect(hole.xMin - HolePadding, hole.yMin - HolePadding, hole.width + 2 * HolePadding, hole.height + 2 * HolePadding);
                SetRect(dimTop, full.xMin, hole.yMax, full.xMax, full.yMax);
                SetRect(dimBottom, full.xMin, full.yMin, full.xMax, hole.yMin);
                SetRect(dimLeft, full.xMin, hole.yMin, hole.xMin, hole.yMax);
                SetRect(dimRight, hole.xMax, hole.yMin, full.xMax, hole.yMax);
                ring.gameObject.SetActive(true);
                SetRect(ring, hole.xMin, hole.yMin, hole.xMax, hole.yMax);
                PlaceCallout(full, hole);
            }
            else
            {
                SetRect(dimTop, full.xMin, full.yMin, full.xMax, full.yMax);
                SetRect(dimBottom, 0, 0, 0, 0);
                SetRect(dimLeft, 0, 0, 0, 0);
                SetRect(dimRight, 0, 0, 0, 0);
                ring.gameObject.SetActive(false);
                callout.sizeDelta = new Vector2(CalloutWidth, callout.sizeDelta.y);
                LayoutRebuilder.ForceRebuildLayoutImmediate(callout);
                callout.anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>Beside the element, on whichever side has more room, never covering it.</summary>
        void PlaceCallout(Rect full, Rect hole)
        {
            // Over a wide element (the option list) the card matches its width, so it is shorter and fits beside it.
            float width = hole.width > 900f ? Mathf.Min(hole.width, full.width - 2 * Margin) : CalloutWidth;
            callout.sizeDelta = new Vector2(width, callout.sizeDelta.y);
            LayoutRebuilder.ForceRebuildLayoutImmediate(callout);
            Vector2 size = callout.rect.size;
            float above = full.yMax - hole.yMax, below = hole.yMin - full.yMin;
            float y = above >= below
                ? hole.yMax + Margin + size.y / 2f
                : hole.yMin - Margin - size.y / 2f;
            y = Mathf.Clamp(y, full.yMin + Margin + size.y / 2f, full.yMax - Margin - size.y / 2f);
            float x = Mathf.Clamp(hole.center.x, full.xMin + Margin + size.x / 2f, full.xMax - Margin - size.x / 2f);
            callout.anchoredPosition = new Vector2(x, y);
        }

        bool TryLocalRect(RectTransform rt, out Rect rect)
        {
            rect = default(Rect);
            if (rt == null || !rt.gameObject.activeInHierarchy) return false;
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            // Screen-space overlay canvases share screen coordinates, so this works across canvases.
            Vector2 a = area.InverseTransformPoint(corners[0]), b = area.InverseTransformPoint(corners[2]);
            rect = Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
            return rect.width > 1f && rect.height > 1f;
        }

        static void SetRect(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2((xMin + xMax) / 2f, (yMin + yMax) / 2f);
            rt.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
        }

        // ------------------------------------------------------------------ targets

        RectTransform Resolve(TutorialTarget t)
        {
            switch (t)
            {
                case TutorialTarget.Money: return FindChip(StatKind.Money);
                case TutorialTarget.Time: return FindChip(StatKind.Time);
                case TutorialTarget.Trust: return FindChip(StatKind.Trust);
                case TutorialTarget.Resistance: return FindChip(StatKind.Resistance);
                case TutorialTarget.Capacity: return FindChip(StatKind.Capacity);
                case TutorialTarget.Hud: return Bounds(FindObjectsByType<StatChip>(FindObjectsSortMode.None).Where(c => c.isActiveAndEnabled).Select(c => (RectTransform)c.transform), "~HudBounds");
                case TutorialTarget.Pause:
                    var pause = transform.root.GetComponentsInChildren<Button>(false).FirstOrDefault(b => b.name == "PauseButton");
                    return pause != null ? (RectTransform)pause.transform : null;
                case TutorialTarget.Options: return Bounds(VisibleOptions().Select(b => (RectTransform)b.transform), "~OptionBounds");
                case TutorialTarget.LockedOption: return FindLockedOption();
                default: return null;
            }
        }

        static RectTransform FindChip(StatKind kind)
        {
            var chip = FindObjectsByType<StatChip>(FindObjectsSortMode.None).FirstOrDefault(c => c.Kind == kind && c.isActiveAndEnabled);
            return chip != null ? (RectTransform)chip.transform : null;
        }

        static IEnumerable<Button> VisibleOptions()
        {
            var menu = Fungus.MenuDialog.ActiveMenuDialog;
            if (menu == null || !menu.isActiveAndEnabled) return Enumerable.Empty<Button>();
            return menu.CachedButtons.Where(b => b != null && b.gameObject.activeInHierarchy);
        }

        static RectTransform FindLockedOption()
        {
            var locked = VisibleOptions().FirstOrDefault(b => !b.interactable);
            return locked != null ? (RectTransform)locked.transform : null;
        }

        readonly Dictionary<string, RectTransform> proxies = new Dictionary<string, RectTransform>();

        /// <summary>An invisible rect covering several elements, so a group can be highlighted as one.</summary>
        RectTransform Bounds(IEnumerable<RectTransform> items, string name)
        {
            bool any = false;
            Rect r = default(Rect);
            foreach (var it in items)
            {
                Rect one;
                if (!TryLocalRect(it, out one)) continue;
                r = any ? Rect.MinMaxRect(Mathf.Min(r.xMin, one.xMin), Mathf.Min(r.yMin, one.yMin), Mathf.Max(r.xMax, one.xMax), Mathf.Max(r.yMax, one.yMax)) : one;
                any = true;
            }
            if (!any) return null;
            RectTransform proxy;
            if (!proxies.TryGetValue(name, out proxy) || proxy == null)
            {
                proxy = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
                proxy.SetParent(area, false);
                proxy.SetAsFirstSibling();
                proxies[name] = proxy;
            }
            SetRect(proxy, r.xMin, r.yMin, r.xMax, r.yMax);
            return proxy;
        }

        // ------------------------------------------------------------------ state of the rest of the UI

        /// <summary>No card, results screen or pause menu is up, so a tip would not cover something else.</summary>
        static bool QuietScreen()
        {
            var card = FindAnyObjectByType<VisualCard>();
            if (card != null && card.gameObject.activeInHierarchy) return false;
            var results = FindAnyObjectByType<ResultsPanel>();
            if (results != null && results.gameObject.activeInHierarchy) return false;
            var pause = FindAnyObjectByType<PauseMenu>();
            if (pause != null && pause.IsPaused) return false;
            return true;
        }

        static bool HudVisible()
        {
            var hud = FindAnyObjectByType<StatHUD>();
            if (hud == null) return false;
            var g = hud.GetComponent<CanvasGroup>();
            return g != null && g.alpha > 0.99f;
        }

        /// <summary>Fungus "click anywhere" and keyboard input would otherwise advance dialogue behind the tip.</summary>
        static void SetDialogInput(bool enabled)
        {
            foreach (var input in FindObjectsByType<Fungus.DialogInput>(FindObjectsSortMode.None))
                input.enabled = enabled;
        }

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / 0.2f);
                yield return null;
            }
            group.alpha = to;
        }
    }
}

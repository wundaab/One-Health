using System.Collections;
using UnityEngine;

namespace OneHealth
{
    /// <summary>Fade/scale-in panel used by every overlay (settings, pause, results, cards).</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] RectTransform content;
        [SerializeField] float duration = 0.18f;

        CanvasGroup group;
        Coroutine running;

        public bool IsOpen { get { return gameObject.activeSelf; } }

        CanvasGroup Group { get { if (group == null) group = GetComponent<CanvasGroup>(); return group; } }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Animate(true);
        }

        public void Close()
        {
            if (!gameObject.activeSelf) return;
            Animate(false);
        }

        void Animate(bool opening)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(opening));
        }

        IEnumerator Run(bool opening)
        {
            Group.blocksRaycasts = opening;
            Group.interactable = opening;
            float from = opening ? 0f : 1f, to = opening ? 1f : 0f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(from, to, t / duration);
                Group.alpha = k;
                if (content != null) content.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
                yield return null;
            }
            Group.alpha = to;
            if (content != null) content.localScale = Vector3.one;
            running = null;
            if (!opening) gameObject.SetActive(false);
        }
    }
}

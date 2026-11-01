using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>Simple two-button confirmation, e.g. before New Game overwrites saved progress.</summary>
    public class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text message;
        [SerializeField] Button confirmButton;
        [SerializeField] TMP_Text confirmLabel;
        [SerializeField] Button cancelButton;

        System.Action onConfirm;

        bool wired;

        void Awake() { Wire(); }

        // Buttons are wired on first use as well as in Awake, so the overlay works however it is activated.
        void Wire()
        {
            if (wired) return;
            wired = true;
            confirmButton.onClick.AddListener(() => { Close(); if (onConfirm != null) onConfirm(); });
            cancelButton.onClick.AddListener(Close);
        }

        void Close()
        {
            ModalStack.Remove(this);
            panel.Close();
        }

        // Esc cancels when this is the topmost dialog (M3); the dialog beneath stays open.
        void Update()
        {
            if (panel.IsOpen && ModalStack.TryConsumeEscape(this)) Close();
        }

        void OnDisable() { ModalStack.Remove(this); }

        public void Show(string titleText, string messageText, string confirmText, System.Action confirmed)
        {
            Wire();
            title.text = titleText;
            message.text = messageText;
            confirmLabel.text = confirmText;
            onConfirm = confirmed;
            panel.Open();
            ModalStack.Push(this);
            cancelButton.Select();
        }
    }
}

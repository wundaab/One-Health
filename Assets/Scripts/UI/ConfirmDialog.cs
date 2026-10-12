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

        void Awake()
        {
            confirmButton.onClick.AddListener(() => { panel.Close(); if (onConfirm != null) onConfirm(); });
            cancelButton.onClick.AddListener(() => panel.Close());
        }

        public void Show(string titleText, string messageText, string confirmText, System.Action confirmed)
        {
            title.text = titleText;
            message.text = messageText;
            confirmLabel.text = confirmText;
            onConfirm = confirmed;
            panel.Open();
            cancelButton.Select();
        }
    }
}

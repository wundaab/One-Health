using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneHealth
{
    /// <summary>Music and SFX toggles. Used by both the main menu and the pause menu.</summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] UIPanel panel;
        [SerializeField] Toggle musicToggle;
        [SerializeField] Toggle sfxToggle;
        [SerializeField] TMP_Text musicState;
        [SerializeField] TMP_Text sfxState;
        [SerializeField] Button backButton;

        System.Action onClosed;

        bool wired;

        void Awake() { Wire(); }

        // Buttons are wired on first use as well as in Awake, so the overlay works however it is activated.
        void Wire()
        {
            if (wired) return;
            wired = true;
            musicToggle.onValueChanged.AddListener(v => { AudioPreferences.MusicOn = v; Refresh(); });
            sfxToggle.onValueChanged.AddListener(v => { AudioPreferences.SfxOn = v; Refresh(); });
            backButton.onClick.AddListener(Close);
        }

        public void Open(System.Action closed = null)
        {
            Wire();
            onClosed = closed;
            musicToggle.SetIsOnWithoutNotify(AudioPreferences.MusicOn);
            sfxToggle.SetIsOnWithoutNotify(AudioPreferences.SfxOn);
            Refresh();
            panel.Open();
            backButton.Select();
        }

        public void Close()
        {
            panel.Close();
            if (onClosed != null) onClosed();
            onClosed = null;
        }

        void Refresh()
        {
            // State is always written as text too, so it never relies on colour alone.
            musicState.text = musicToggle.isOn ? "On" : "Off";
            sfxState.text = sfxToggle.isOn ? "On" : "Off";
        }
    }
}

using System;
using Fungus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OneHealth
{
    /// <summary>
    /// Independent Music and SFX toggles (S01 blueprint: "Music and SFX are independently
    /// toggleable in Settings"). Persisted in PlayerPrefs and applied to Fungus' audio
    /// sources, which live on the persistent FungusManager.
    /// </summary>
    public static class AudioPreferences
    {
        const string MusicKey = "OHI_MusicOn";
        const string SfxKey = "OHI_SfxOn";

        public static event Action Changed;

        public static bool MusicOn
        {
            get { return PlayerPrefs.GetInt(MusicKey, 1) == 1; }
            set { PlayerPrefs.SetInt(MusicKey, value ? 1 : 0); PlayerPrefs.Save(); Apply(); }
        }

        public static bool SfxOn
        {
            get { return PlayerPrefs.GetInt(SfxKey, 1) == 1; }
            set { PlayerPrefs.SetInt(SfxKey, value ? 1 : 0); PlayerPrefs.Save(); Apply(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            // Older menu builds paused the whole AudioListener; make sure that never sticks.
            AudioListener.pause = false;
            SceneManager.sceneLoaded += (s, m) => Apply();
            Apply();
        }

        /// <summary>
        /// Fungus MusicManager audio sources, in order: music, ambiance, sound effect,
        /// default voice, writer sound effect.
        /// </summary>
        public static void Apply()
        {
            var manager = FungusManager.Instance.MusicManager;
            if (manager == null) return;

            var sources = manager.GetComponents<AudioSource>();
            bool music = MusicOn, sfx = SfxOn;
            for (int i = 0; i < sources.Length; i++)
                sources[i].mute = (i == 0) ? !music : !sfx;

            if (Changed != null) Changed();
        }

        /// <summary>Plays a UI sound through Fungus so it respects the SFX toggle.</summary>
        public static void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || !SfxOn) return;
            FungusManager.Instance.MusicManager.PlaySound(clip, volume);
        }
    }
}

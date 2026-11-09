using System.Collections.Generic;
using UnityEngine;

namespace OneHealth
{
    public enum Cue { Confirm, Discovery, Warning, Achievement, Transition }

    /// <summary>
    /// The small set of feedback sounds (confirmation, discovery, warning, achievement, transition), generated
    /// once as soft sine tones so they stay consistent and need no audio assets. Played through Fungus, so they
    /// respect the Sound Effects setting.
    /// </summary>
    public static class UICues
    {
        static readonly Dictionary<Cue, AudioClip> clips = new Dictionary<Cue, AudioClip>();

        public static void Play(Cue cue) { AudioPreferences.PlayUISound(Clip(cue), 0.6f); }

        static AudioClip Clip(Cue cue)
        {
            AudioClip c;
            if (clips.TryGetValue(cue, out c) && c != null) return c;
            switch (cue)
            {
                case Cue.Confirm: c = Tones(cue, 0.09f, 784f); break;                        // G5
                case Cue.Discovery: c = Tones(cue, 0.1f, 659f, 988f); break;                  // E5 B5, rising
                case Cue.Warning: c = Tones(cue, 0.14f, 392f, 349f); break;                   // G4 F4, soft and low
                case Cue.Achievement: c = Tones(cue, 0.11f, 523f, 659f, 784f, 1047f); break;  // C major arpeggio
                default: c = Tones(cue, 0.16f, 440f, 587f); break;                            // A4 D5
            }
            clips[cue] = c;
            return c;
        }

        static AudioClip Tones(Cue cue, float noteLength, params float[] notes)
        {
            const int rate = 44100;
            int per = (int)(noteLength * rate), tail = (int)(0.18f * rate);
            var data = new float[per * notes.Length + tail];
            for (int n = 0; n < notes.Length; n++)
            {
                int length = per + tail;   // each note rings on under the next
                for (int i = 0; i < length && n * per + i < data.Length; i++)
                {
                    float t = i / (float)rate;
                    float envelope = Mathf.Min(1f, t / 0.008f) * Mathf.Exp(-t * 9f);
                    float wave = Mathf.Sin(2f * Mathf.PI * notes[n] * t) + 0.25f * Mathf.Sin(4f * Mathf.PI * notes[n] * t);
                    data[n * per + i] += 0.22f * envelope * wave;
                }
            }
            var clip = AudioClip.Create("Cue_" + cue, data.Length, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}

using UnityEngine;
using TMPro;

public class SoundManager : MonoBehaviour
{
    public TMP_Text soundText;

    void Start()
    {
        UpdateSoundText();
    }

    public void ToggleSound()
    {
        AudioListener.pause = !AudioListener.pause;
        UpdateSoundText();
    }

    void UpdateSoundText()
    {
        if (soundText != null)
        {
            soundText.text = AudioListener.pause ? "Sound: OFF" : "Sound: ON";
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text volumeLabel;

    private Coroutine animationRoutine;

    void Start()
    {
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = AudioListener.volume;

        volumeSlider.onValueChanged.AddListener(ChangeVolume);
        UpdateLabel(volumeSlider.value);
    }

    public void ChangeVolume(float volume)
    {
        AudioListener.volume = volume;
        UpdateLabel(volume);

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimateVolume(volume));
    }

    void UpdateLabel(float volume)
    {
        if (volumeLabel != null)
            volumeLabel.text = "Volume: " + Mathf.RoundToInt(volume * 100f) + "%";
    }

    IEnumerator AnimateVolume(float targetVolume)
    {
        // Small visual feedback: smoothly update the label.
        yield return null;
        animationRoutine = null;
    }
}
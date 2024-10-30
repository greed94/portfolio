using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocentWriterInfo : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Image artImg;
    public TMP_Text nameValueTMP;
    public TMP_Text birthdayValueTMP;
    public TMP_Text introValueTMP;
    public TMP_Text educationValueTMP;
    public TMP_Text soloExhibitionValueTMP;
    public TMP_Text groupExhibitionValueTMP;
    public AudioSource audioSource;
    public Slider slider;
    public GameObject playerButton;
    public GameObject stopButton;

    public void Play()
    {
        playerButton.SetActive(false);
        stopButton.SetActive(true);
        audioSource.Play();
        StartCoroutine("OnPlaytimeUI");
    }

    public void Pause()
    {
        playerButton.SetActive(true);
        stopButton.SetActive(false);
        audioSource.Pause();
    }

    public void ResetPlaytimeUI()
    {
        audioSource.Stop();
        slider.value = 0;
    }

    private bool sliderValue;

    private IEnumerator OnPlaytimeUI()
    {
        while (true)
        {
            sliderValue = false;
            slider.value = audioSource.time / audioSource.clip.length;
            sliderValue = true;

            yield return new WaitForSeconds(1);
        }
    }

    public void UpdateSliderValue()
    {
        if (sliderValue)
        {
            audioSource.time = slider.value * audioSource.clip.length;
        }
    }
}

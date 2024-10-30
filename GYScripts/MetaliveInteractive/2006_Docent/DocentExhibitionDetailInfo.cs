using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocentExhibitionDetailInfo : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Image ArtImg;
    public TMP_Text TitleTMP;
    public TMP_Text PeriodTMP;
    public TMP_Text LocationTMP;
    public TMP_Text WriterTMP;
    public TMP_Text WorkKeyTMP;
    public TMP_Text WorkValueTMP;
    public TMP_Text ExplanationTMP;
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

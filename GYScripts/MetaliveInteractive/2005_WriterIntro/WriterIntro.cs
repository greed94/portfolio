using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class WriterIntro : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Image artImg;
    public TMP_Text nameValueTMP;
    public TMP_Text birthdayValueTMP;
    public TMP_Text introValueTMP;
    public TMP_Text educationValueTMP;
    public TMP_Text soloExhibitionValueTMP;
    public TMP_Text groupExhibitionValueTMP;
    private AudioSource audioSource;
    public Slider slider;
    public GameObject playerButton;
    public GameObject stopButton;
    private AsyncOperationHandle<InteractiveEventData> data;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Sound.Mute();
        SetInfo().Forget();
    }

    private async UniTaskVoid SetInfo()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            var eventTable = data.Result.eventTable;
            var eventSpriteTable = data.Result.eventSpriteTable;
            var eventAudioTable = data.Result.eventAudioTable;

            nameValueTMP.text = eventTable.Find(x => x.key == "name").value;
            birthdayValueTMP.text = eventTable.Find(x => x.key == "birthday").value;
            introValueTMP.text = eventTable.Find(x => x.key == "intro").value;
            educationValueTMP.text = eventTable.Find(x => x.key == "education").value;
            soloExhibitionValueTMP.text = eventTable.Find(x => x.key == "soloExhibition").value;
            groupExhibitionValueTMP.text = eventTable.Find(x => x.key == "groupExhibition").value;
            artImg.sprite = eventSpriteTable.Find(x => x.key == "image").value;
            audioSource.clip = eventAudioTable.Find(x => x.key == "audio").value;

            SetRect();

            Play();
        }
    }

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

    private void ResetPlaytimeUI()
    {
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

    private void OnDestroy()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }

        Sound.Play();
    }

    private void SetRect()
    {
        scrollRect.enabled = false;
        RectTransform content = scrollRect.content;
        // BirthdayValue(TMP) 생년월일 밑으로 텍스트 박스 계산
        float trY = content.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y - content.GetChild(1).GetComponent<RectTransform>().sizeDelta.y;

        for (int i = 1; i < content.childCount; i++)
        {
            RectTransform prefab = content.GetChild(i).GetComponent<RectTransform>();
            if (Setting.World.code == 4066 && prefab.name.Contains("Education"))
            {
                break;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(prefab);

            if (prefab.name.Contains("Key"))
            {
                trY -= 80f;
            }
            else
            {
                trY -= 16f;
            }
            prefab.anchoredPosition = new Vector2(prefab.anchoredPosition.x, trY);
            trY -= prefab.sizeDelta.y;
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, -trY);
        content.anchoredPosition = Vector2.zero;

        scrollRect.enabled = true;
    }
}

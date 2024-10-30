using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ArtInfoManager : MonoBehaviour
{
    #region Variable

    public ArtInfoController artInfoController;
    public ArtInfoController artInfoPlayBarController;
    private ArtInfoController selectArtInfoController;
    private ArtInfoPlayBarController selectArtInfoPlayBarController;
    private AsyncOperationHandle<InteractiveEventData> data;
    public ScrollRect scrollRect;


    #endregion

    #region Lifecycle

    private void Start()
    {
        if (Setting.World.code == 4057)
        {
            selectArtInfoController = artInfoController;
        }
        else if (Setting.World.code == 4066)
        {
            selectArtInfoController = artInfoPlayBarController;
            selectArtInfoPlayBarController = selectArtInfoController.artInfoPlayBarController;
            Sound.Mute();
        }


        Patch();
    }

    private void OnDestroy()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }

        if (Setting.World.code == 4066)
        {
            Sound.Play();
        }
    }

    #endregion

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        OnCanvas();

        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            var eventTable = data.Result.eventTable;
            var eventSpriteTable = data.Result.eventSpriteTable;
            var eventAudioTable = data.Result.eventAudioTable;

            selectArtInfoController.titleValueTMP.text = eventTable.Find(x => x.key == "title").value;
            selectArtInfoController.textureTMP.text = eventTable.Find(x => x.key == "texture").value;
            selectArtInfoController.sizeTMP.text = eventTable.Find(x => x.key == "size").value;
            selectArtInfoController.yearTMP.text = eventTable.Find(x => x.key == "year").value;
            selectArtInfoController.artImage.sprite = eventSpriteTable.Find(x => x.key == "image").value;

            if (eventSpriteTable.Exists(x => x.key == "zoom"))
            {
                selectArtInfoController.zoomButton.gameObject.SetActive(true);
                selectArtInfoController.zoomImg.sprite = eventSpriteTable.Find(x => x.key == "zoom").value;
                selectArtInfoController.zoomImg.SetNativeSize();
                // zoomImg.rectTransform.sizeDelta = zoomImg.sprite.rect.size;
            }
            else if (eventSpriteTable.Exists(x => x.key == "video"))
            {
                Setting.Video.option = true;
                Setting.Video.url = data.Result.eventSpriteTable.Find(x => x.key == "video").link;
                Setting.Video.cameraRotation = Quaternion.Euler(40f, 110f, 0);
                selectArtInfoController.videoButton.gameObject.SetActive(true);
            }

            if (Setting.World.code == 4066)
            {
                selectArtInfoPlayBarController.introTMP.text = eventTable.Find(x => x.key == "intro").value;
                selectArtInfoPlayBarController.audioSource.clip = eventAudioTable.Find(x => x.key == "audio").value;
                SetRect();
                Play();
            }
        }
    }

    private void SetRect()
    {
        scrollRect.enabled = false;
        RectTransform content = scrollRect.content;
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectArtInfoPlayBarController.introTMP.rectTransform);
        content.sizeDelta = new Vector2(content.sizeDelta.x, 1162 + selectArtInfoPlayBarController.introTMP.rectTransform.sizeDelta.y);
        content.anchoredPosition = Vector2.zero;

        scrollRect.enabled = true;
    }

    public void Play()
    {
        selectArtInfoPlayBarController.playerButton.SetActive(false);
        selectArtInfoPlayBarController.stopButton.SetActive(true);
        selectArtInfoPlayBarController.audioSource.Play();
        StartCoroutine("OnPlaytimeUI");
    }

    public void Pause()
    {
        selectArtInfoPlayBarController.playerButton.SetActive(true);
        selectArtInfoPlayBarController.stopButton.SetActive(false);
        selectArtInfoPlayBarController.audioSource.Pause();
    }

    private void ResetPlaytimeUI()
    {
        selectArtInfoPlayBarController.slider.value = 0;
    }

    private bool sliderValue;

    private IEnumerator OnPlaytimeUI()
    {
        while (true)
        {
            sliderValue = false;
            selectArtInfoPlayBarController.slider.value = selectArtInfoPlayBarController.audioSource.time / selectArtInfoPlayBarController.audioSource.clip.length;
            sliderValue = true;

            yield return new WaitForSeconds(1);
        }
    }

    public void UpdateSliderValue()
    {
        if (sliderValue)
        {
            selectArtInfoPlayBarController.audioSource.time = selectArtInfoPlayBarController.slider.value * selectArtInfoPlayBarController.audioSource.clip.length;
        }
    }

    public void OnCanvas()
    {
        selectArtInfoController.GetComponentInParent<Canvas>().enabled = true;
    }
}
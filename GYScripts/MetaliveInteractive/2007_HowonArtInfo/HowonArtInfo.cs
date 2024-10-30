using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HowonArtInfo : MonoBehaviour
{
    public Image artImg;
    public TMP_Text titleValueTMP;
    public TMP_Text nameTMP;
    public TMP_Text explanationTMP;
    public Button videoButton;
    private AsyncOperationHandle<InteractiveEventData> data;
    public int artNo;
    public RectTransform content;

    private void Start()
    {
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

            titleValueTMP.text = eventTable.Find(x => x.key == "title").value;
            nameTMP.text = eventTable.Find(x => x.key == "name").value;
            explanationTMP.text = eventTable.Find(x => x.key == "explanation").value;
            LayoutRebuilder.ForceRebuildLayoutImmediate(explanationTMP.rectTransform);
            artNo = int.Parse(eventTable.Find(x => x.key == "artNo").value);
            artImg.sprite = eventSpriteTable.Find(x => x.key == "image").value;
            float contentSizeDeltaY = 0;
            for (int i = 0; i < content.childCount; i++)
            {
                contentSizeDeltaY += content.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
            }
            contentSizeDeltaY += explanationTMP.rectTransform.sizeDelta.y;
            content.sizeDelta = new Vector2(0, contentSizeDeltaY);
            // Setting.Video.option = true;

            // Setting.Video.url = data.Result.eventSpriteTable.Find(x => x.key == Setting.Server.platform + "video").link;

            // Setting.Video.cameraRotation = Quaternion.Euler(40f, 110f, 0);
            videoButton.onClick.AddListener(WatchVideoButton);
            videoButton.gameObject.SetActive(true);
        }
    }

    public void WatchVideoButton()
    {
        OpenURLMetalive.OpenURL(data.Result.eventSpriteTable.Find(x => x.key == "video").link);
    }

    private void OnDisable()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
    }
}
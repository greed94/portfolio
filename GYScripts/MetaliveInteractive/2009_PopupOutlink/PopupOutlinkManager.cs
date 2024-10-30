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

public class PopupOutlinkManager : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text subTitle;
    public Image contentImage;
    private AsyncOperationHandle<InteractiveEventData> data;
    private string outlinkURL;

    private void Start()
    {
        Patch();
    }

    private void OnDestroy()
    {
        PatchRelease();
    }

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);
        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            InteractiveEventData result = data.Result;
            title.text = result.eventTable.Find(x => x.key == "title").value;
            subTitle.text = result.eventTable.Find(x => x.key == "subTitle").value;
            contentImage.sprite = result.eventSpriteTable.Find(x => x.key == "outlink").value;
            outlinkURL = result.eventSpriteTable.Find(x => x.key == "outlink").link;
            contentImage.enabled = true;
        }
    }

    private void PatchRelease()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
    }

    public void OutlinkButton()
    {
        //Application.OpenURL(outlinkURL);
        OpenURLMetalive.OpenURL(outlinkURL);
    }
}

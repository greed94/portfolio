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

public class OneCampusArtInfoManager : MonoBehaviour
{
    #region Variable

    public TMP_Text titleValueTMP;
    public Image artImage;
    private AsyncOperationHandle<InteractiveEventData> data;


    #endregion

    #region Lifecycle

    private void Start()
    {
        Patch();
    }

    private void OnDestroy()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
    }

    #endregion

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            var eventTable = data.Result.eventTable;
            var eventSpriteTable = data.Result.eventSpriteTable;

            titleValueTMP.text = eventTable.Find(x => x.key == "title").value;
            artImage.sprite = eventSpriteTable.Find(x => x.key == "image").value;
        }
    }

    public void WatchVideoButton()
    {
        OpenURLMetalive.OpenURL(data.Result.eventSpriteTable.Find(x => x.key == "video").link);
    }
}
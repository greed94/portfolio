using Cysharp.Threading.Tasks;
using Metalive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ZoomImageManager : MonoBehaviour
{
    public Image image;

    private AsyncOperationHandle<InteractiveEventData> data;

    private List<InteractiveSpriteTable> spriteTable;

    void Start()
    {
        InteractiveInit();
    }

    private void OnDestroy()
    {
        Addressables.Release(data);
    }

    public async void InteractiveInit()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);
        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            InteractiveEventData result = data.Result;
            spriteTable = result.eventSpriteTable;
        }
        UpdateImage();
    }

    private void UpdateImage()
    {
        image.sprite = spriteTable[0].value;
    }
}

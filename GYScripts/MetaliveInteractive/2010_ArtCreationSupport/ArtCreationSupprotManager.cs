using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ArtCreationSupprotManager : MonoBehaviour
{
    #region Variable

    public ArtCreationSupprotItem artCreationSupprotItem;
    private AsyncOperationHandle<InteractiveEventData> data;

    #endregion

    #region Lifecycle

    private void Start()
    {
        Patch();
    }

    private void OnDestroy()
    {
        if (artCreationSupprotItem)
        {
            artCreationSupprotItem = null;
        }

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
            var table = data.Result.eventTable;
            artCreationSupprotItem.image.sprite = data.Result.eventSpriteTable.Find(x => x.key == "image").value;
            artCreationSupprotItem.title.text = table.Find(x => x.key == "title").value;
            artCreationSupprotItem.subTitle.text = table.Find(x => x.key == "subTitle").value;
            artCreationSupprotItem.explanation.text = table.Find(x => x.key == "explanation").value;
        }
    }
}

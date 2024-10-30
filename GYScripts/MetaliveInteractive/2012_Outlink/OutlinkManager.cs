using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class OutlinkManager : MonoBehaviour
{
    private AsyncOperationHandle<InteractiveEventData> data;

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

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            string url = data.Result.eventTable.Find(x => x.key == "url").link;
            Debug.Log(url);
            Outlink(url);
            Close();
        }
    }

    private void Outlink(string url)
    {
        //Application.OpenURL(url);
        OpenURLMetalive.OpenURL(url);
    }

    private void Close()
    {
        Scene scene = SceneManager.GetSceneByName("2012_Outlink");
        SceneManager.UnloadSceneAsync(scene);
    }
}

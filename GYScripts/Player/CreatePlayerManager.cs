using Cysharp.Threading.Tasks;
using Metalive;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CreatePlayerManager : MonoBehaviour
{
    public static GameObject CreatePlayer;
    public static GameObject CreateMeti;
    public static GameObject CreateNPCBot;
    public static UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> data;
    public static UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> data2;

    private void Start()
    {
        CreatePlayerAsync().Forget();
        //CreateNPCBotAsync().Forget();
    }

    private async UniTask CreatePlayerAsync()
    {
        DefaultPool defaultPool = PhotonNetwork.PrefabPool as DefaultPool;
        var location = "Player";
        var size = Addressables.GetDownloadSizeAsync(location);
        await size;

        if (size.Result > 0)
        {
            var download = Addressables.DownloadDependenciesAsync(location);
            await download;

            if (download.IsValid())
            {
                Addressables.Release(download);
            }
        }

        if (size.IsValid())
        {
            Addressables.Release(size);
        }

        data = Addressables.LoadAssetAsync<GameObject>(location);
        await data;
        CreatePlayer = data.Result;
        CreatePlayer.name = "Player";

        CreateMeti = CreatePlayer.transform.Find("meti").gameObject;
        CreateMeti.name = "meti";

        //포톤 캐릭생성용 Resource Cache 등록
        defaultPool.ResourceCache.Add(CreatePlayer.name, CreatePlayer);

        //포톤 NPC생성용 Resource Cache 등록
        defaultPool.ResourceCache.Add(CreateMeti.name, CreateMeti);
    }

    //private async UniTask CreateNPCBotAsync()
    //{
    //    DefaultPool defaultPool = PhotonNetwork.PrefabPool as DefaultPool;
    //    var location = "WanderingNPC";
    //    var size = Addressables.GetDownloadSizeAsync(location);
    //    await size;

    //    if (size.Result > 0)
    //    {
    //        var download = Addressables.DownloadDependenciesAsync(location);
    //        await download;

    //        if (download.IsValid())
    //        {
    //            Addressables.Release(download);
    //        }
    //    }

    //    if (size.IsValid())
    //    {
    //        Addressables.Release(size);
    //    }

    //    data2 = Addressables.LoadAssetAsync<GameObject>(location);
    //    await data2;
    //    CreateNPCBot = data2.Result;
    //    CreateNPCBot.name = "WanderingNPC";

    //    //포톤 NPC생성용 Resource Cache 등록
    //    defaultPool.ResourceCache.Add(CreateNPCBot.name, CreateNPCBot);
    //}

    public static void SystemInit()
    {
        CreatePlayer = null;
        CreateMeti = null;
        CreateNPCBot = null;
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
        if (data2.IsValid())
        {
            Addressables.Release(data2);
        }
    }
}
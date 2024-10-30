using System.Collections;
using System.Collections.Generic;
using Metalive;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AdvertisementManager : MonoBehaviour
{
    public AudioSource bgm;
    public List<string> videoUrlList;
    private GameObject advertisementPrefab;
    private AsyncOperationHandle<GameObject> advertisementClone;
    private GameObject advertisement { get; set; }

    private void Start()
    {
        advertisementPrefab = Instantiate(Resources.Load<GameObject>("Advertisement" + Setting.World.code), transform);
        advertisementPrefab.transform.GetChild(0).GetComponent<YoutubePlayerLivestream>().advertisementManager = this;
        advertisementPrefab.name = "Advertisement";
        if (bgm != null)
        {
            advertisementPrefab.GetComponent<AdvertisementSoundCheck>().bgm = bgm;
        }
        if (videoUrlList.Count != 0)
        {
            advertisementPrefab.transform.GetChild(0).GetComponent<YoutubePlayerLivestream>().videoUrlList = videoUrlList;
        }
        //AdvertisementPatch();
    }

    private void OnDestroy()
    {
        if (advertisementClone.IsValid())
        {
            Addressables.Release(advertisementClone);

            if (advertisement)
            {
                DestroyImmediate(advertisement);
            }
        }
    }

    private void AdvertisementPatch()
    {
        advertisementClone = Addressables.LoadAssetAsync<GameObject>("Advertisement" + Setting.World.code);
        advertisementClone.WaitForCompletion();

        if (advertisementClone.Status == AsyncOperationStatus.Succeeded)
        {
            advertisement = Instantiate(advertisementClone.Result, transform);
            advertisement.name = "Advertisement";
            if (bgm != null)
            {
                advertisement.GetComponent<AdvertisementSoundCheck>().bgm = bgm;
            }
            if (videoUrlList != null)
            {
                advertisement.transform.GetChild(0).GetComponent<YoutubePlayerLivestream>().videoUrlList = videoUrlList;
            }
        }
    }
}
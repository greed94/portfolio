using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HowonDocentMain : MonoBehaviour
{
    private AsyncOperationHandle<InteractiveEventData> data;
    [HideInInspector]
    public InteractiveEventData result;
    [HideInInspector]
    public List<InteractiveTable> table = new List<InteractiveTable>();
    public Image backgroundImg;
    [HideInInspector]
    public Canvas mainCanvas;
    public ScrollRect mainScrollRect;

    private void OnEnable()
    {
        SetInfo().Forget();
        mainCanvas = GetComponentInParent<Canvas>();
    }

    private async UniTaskVoid SetInfo()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            GetComponentInParent<Canvas>().enabled = true;
            result = data.Result;
            table = result.eventTable;
            backgroundImg.sprite = result.eventSpriteTable.Find(x => x.key == "background").value;
            mainScrollRect.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
    }
}

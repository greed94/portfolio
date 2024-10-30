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

public class DodalssuMain : MonoBehaviour
{
    [HideInInspector]
    public InteractiveEventData result;
    [HideInInspector]
    public List<InteractiveTable> table = new List<InteractiveTable>();
    public Image backgroundImg;
    public TextMeshProUGUI comment2TMP;
    public TextMeshProUGUI locationInroTMP;
    public TextMeshProUGUI tripTMP;
    public TextMeshProUGUI advertisementTMP;
    private Dictionary<string, string> dic = new Dictionary<string, string>();
    private AsyncOperationHandle<InteractiveEventData> data;

    private void OnEnable()
    {
        SetInfo().Forget();
        if (Setting.World.code == 4055)
        {
            Quest.Send("30", "4055");
            // Quest.Send("23", Setting.World.code.ToString());
        }
    }


    private async UniTaskVoid SetInfo()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        // Setting.Video.url = data.Result.boraUrl;
        // Setting.Video.option = data.Result.boraOption;
        // Setting.Video.option = false;

        // Setting.Video.cameraPosition = data.Result.boraPosition;
        // Setting.Video.cameraRotation = Quaternion.Euler(data.Result.boraRotation);
        // Setting.Video.cameraScale = data.Result.boraScale;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            result = data.Result;
            table = result.eventTable;
            backgroundImg.sprite = result.eventSpriteTable.Find(x => x.key == "background").value;
            comment2TMP.text = comment2TMP.text.Replace("location", table.Find(x => x.key == "locationKr").value);
            LayoutRebuilder.ForceRebuildLayoutImmediate(comment2TMP.rectTransform);
            var sizeDelta = comment2TMP.transform.parent.GetComponent<RectTransform>().sizeDelta;
            comment2TMP.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(comment2TMP.rectTransform.sizeDelta.x + 120, sizeDelta.y);
            locationInroTMP.text = locationInroTMP.text.Replace("location", table.Find(x => x.key == "locationKr").value);
            tripTMP.text = table.Find(x => x.key == "trip").value;
            advertisementTMP.text = table.Find(x => x.key == "advertisement").value;
            GetComponentInParent<Canvas>().enabled = true;
        }
    }

    public void Video360Button()
    {
        Setting.Video.url = table.Find(x => x.key == "trip").link;
        Setting.Video.option = true;

        if (Setting.World.code == 4056 || Setting.World.code == 4058 || Setting.World.code == 4061 || Setting.World.code == 4065)
        {
            InteractiveScene("101_Video").Forget();
        }
        else
        {
            InteractiveScene("102_Video360").Forget();
        }
    }

    public void VideoButton()
    {
        Setting.Video.url = table.Find(x => x.key == "advertisement").link;
        Debug.Log($"URL: {Setting.Video.url}");
        Setting.Video.option = true;
        InteractiveScene("101_Video").Forget();
    }

    private async UniTaskVoid InteractiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
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
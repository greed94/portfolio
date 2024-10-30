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

public class DocentMainManager : MonoBehaviour
{
    private AsyncOperationHandle<InteractiveEventData> data;
    [HideInInspector]
    public InteractiveEventData result;
    [HideInInspector]
    public List<InteractiveTable> table = new List<InteractiveTable>();
    [HideInInspector]
    public List<InteractiveTable> subTable = new List<InteractiveTable>();
    public Image backgroundImg;
    public TMP_Text NPCNameTMP;
    public TMP_Text comment1TMP;
    public TMP_Text comment2TMP;
    public TMP_Text exhibitionTMP;
    public TMP_Text writerTMP;
    public TMP_Text videoTMP;
    public TMP_Text video2TMP;
    [HideInInspector]
    public Canvas mainCanvas;

    private void OnEnable()
    {
        Sound.Mute();
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
            subTable = result.eventSubTable;
            backgroundImg.sprite = result.eventSpriteTable.Find(x => x.key == "background").value;
            exhibitionTMP.text = exhibitionTMP.text.Replace("exhibition", table.Find(x => x.key == "exhibition").value);
            writerTMP.text = table.Find(x => x.key == "writer").value;
            videoTMP.text = table.Find(x => x.key == "video").value;
            try
            {
                NPCNameTMP.text = subTable.Find(x => x.key == "npcName").value;
                comment1TMP.text = subTable.Find(x => x.key == "comment1").value;
                LayoutRebuilder.ForceRebuildLayoutImmediate(comment1TMP.rectTransform);
                comment1TMP.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(comment1TMP.rectTransform.sizeDelta.x + 96, 121);
                // comment1TMP.GetComponentInParent<RectTransform>().sizeDelta = new Vector2(comment1TMP.rectTransform.sizeDelta.x + 96, 121);
                comment2TMP.text = subTable.Find(x => x.key == "comment2").value;
                LayoutRebuilder.ForceRebuildLayoutImmediate(comment2TMP.rectTransform);
                comment2TMP.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(comment2TMP.rectTransform.sizeDelta.x + 96, 121);
                // comment2TMP.GetComponentInParent<RectTransform>().sizeDelta = new Vector2(comment2TMP.rectTransform.sizeDelta.x + 96, 121);
                video2TMP.text = subTable.Find(x => x.key == "video1").value;
                video2TMP.text = subTable.Find(x => x.key == "video2").value;
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void VideoButton(string key)
    {
        //Application.OpenURL(subTable.Find(x => x.key == key).link);
        OpenURLMetalive.OpenURL(subTable.Find(x => x.key == key).link);
    }

    private void OnDisable()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }

        Sound.Play();
    }
}

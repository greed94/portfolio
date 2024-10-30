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

public class GalleryExternalBookManager : MonoBehaviour
{
    #region Variable

    public TMP_Text subTitle;
    public List<RectTransform> prefabList;
    private AsyncOperationHandle<InteractiveEventData> data;
    public ScrollRect scrollRect;
    private List<InteractiveTable> table;
    private List<InteractiveTable> subTable;
    private List<InteractiveSpriteTable> spriteTable;

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
            table = data.Result.eventTable;
            subTable = data.Result.eventSubTable;
            spriteTable = data.Result.eventSpriteTable;
            subTitle.text = subTable.Find(x => x.key == "subTitle1").value;
            SetPrefab("zero");
        }
    }

    private void SetPrefab(string prefabNo)
    {
        if (scrollRect.enabled)
        {
            scrollRect.enabled = false;
        }

        if (prefabNo.Equals("end"))
        {
            Scene scene = SceneManager.GetSceneByName("2013_GalleryExternalBook");
            SceneManager.UnloadSceneAsync(scene);
            return;
        }

        RectTransform content = scrollRect.content;

        content.anchoredPosition = Vector2.zero;

        for (int i = 0; i < content.childCount;)
        {
            DestroyImmediate(content.GetChild(i).gameObject);
        }

        RectTransform prefabRect = null;

        foreach (var prefab in prefabList)
        {
            if (prefab.name.Contains(prefabNo))
            {
                prefabRect = prefab;
                break;
            }
        }

        if (prefabNo.Equals("first"))
        {
            subTitle.text = subTable.Find(x => x.key == "subTitle2").value;
            scrollRect.enabled = true;
        }
        else
        {
            scrollRect.enabled = false;
        }

        var selectTable = table.FindAll(x => x.key.Contains(prefabNo));
        var selectSpriteTable = spriteTable.FindAll(x => x.key.Contains(prefabNo));

        int j;

        for (j = 1; j <= selectSpriteTable.Count; j++)
        {
            RectTransform prefab = Instantiate(prefabRect, content);
            prefab.gameObject.SetActive(true);
            prefab.anchoredPosition = new Vector2(80 + (prefabRect.sizeDelta.x + 40) * (j - 1), 0);
            var prefabItem = prefab.GetComponent<GalleryExternalBookPrefab>();
            prefabItem.comment.text = selectTable.Find(x => x.key == prefabNo + "Prefab" + j).value;
            prefabItem.image.sprite = selectSpriteTable.Find(x => x.key == prefabNo + "Prefab" + j).value;
            string link = selectSpriteTable.Find(x => x.key == prefabNo + "Prefab" + j).link;
            //prefabItem.button.onClick.AddListener(() => { Application.OpenURL(link); });
            prefabItem.button.onClick.AddListener(() => { OpenURLMetalive.OpenURL(link); });
        }

        content.sizeDelta = new Vector2(120 + (prefabRect.sizeDelta.x + 40) * (j - 1), 0);
    }

    public void OpenLinkButton(string prefabNo)
    {
        SetPrefab(prefabNo);
    }
}

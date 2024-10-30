using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine.UI;
using System;

public class VRExhibitionManager : MonoBehaviour
{
    #region Variable

    public TMP_Text title;
    public TMP_Text subTitle;
    public ScrollRect scrollRect;
    public RectTransform VRExhibitionPrefab;
    public ScrollRect festivalScrollRect;
    public RectTransform festivalPrefab;
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

    InteractiveTable test;

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            if (Setting.World.code == 4067)
            {
                scrollRect.gameObject.SetActive(true);
                festivalScrollRect.gameObject.SetActive(false);
                RectTransform content = scrollRect.content;
                var table = data.Result.eventTable;
                var subTable = data.Result.eventSubTable;
                var spriteTable = data.Result.eventSpriteTable;
                title.SetText(table.Find(x => x.key == "title").value);
                subTitle.SetText("");
                int i;

                for (i = 1; i < table.Count; i++)
                {
                    var prefab = Instantiate(VRExhibitionPrefab, content);
                    prefab.gameObject.SetActive(true);
                    prefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(80 + 760 * (i - 1), 0);
                    var VRExhibitionItem = prefab.GetComponent<VRExhibitionItem>();
                    VRExhibitionItem.title.text = table.Find(x => x.key == "prefabTitle" + i).value;
                    VRExhibitionItem.subTitle.text = subTable.Find(x => x.key == "prefabSubTitle" + i).value;
                    VRExhibitionItem.image.sprite = spriteTable.Find(x => x.key == "prefabImage" + i).value;
                    string link = spriteTable.Find(x => x.key == "prefabImage" + i).link;
                    //VRExhibitionItem.button.onClick.AddListener(() => { Application.OpenURL(link); });
                    VRExhibitionItem.button.onClick.AddListener(() => { OpenURLMetalive.OpenURL(link); });
                }

                content.sizeDelta = new Vector2(120 + 760 * (i - 1), 0);
            }
            else if (Setting.World.code == 4060 || Setting.World.code == 4064 || Setting.World.code == 4059)
            {
                scrollRect.gameObject.SetActive(false);
                festivalScrollRect.gameObject.SetActive(true);
                RectTransform content = festivalScrollRect.content;
                var table = data.Result.eventTable;
                var subTable = data.Result.eventSubTable;
                var spriteTable = data.Result.eventSpriteTable;
                title.SetText(table.Find(x => x.key == "title").value);
                subTitle.SetText(table.Find(x => x.key == "subTitle").value);

                int i;
                int j;

                //2023-10-17 PDK 윤딴딴 3개월 이후 내려가게 설정하는 코드, 추후 값 바뀔수있음. 메모
                var endDate = new DateTime(2023, 12, 31);
                var clientDate = DateTime.Now;
                if (clientDate < endDate)
                {
                    j = 0;
                }
                else
                {
                    j = 1;
                }

                for (i = j; i < spriteTable.Count; i++)
                {
                    var prefab = Instantiate(festivalPrefab, content);
                    prefab.gameObject.SetActive(true);
                    prefab.GetComponent<RectTransform>().anchoredPosition = new Vector2(40 + 760 * (i - j), 0);
                    var festivalItem = prefab.GetComponent<VRExhibitionItem>();
                    festivalItem.image.sprite = spriteTable.Find(x => x.key == "prefabImage" + i).value;
                    string link = spriteTable.Find(x => x.key == "prefabImage" + i).link;
                    //festivalItem.button.onClick.AddListener(() => { Application.OpenURL(link); });
                    festivalItem.button.onClick.AddListener(() => { OpenURLMetalive.OpenURL(link); });
                }

                content.sizeDelta = new Vector2(80 + 760 * (i - j), 0);
            }

        }
    }
}

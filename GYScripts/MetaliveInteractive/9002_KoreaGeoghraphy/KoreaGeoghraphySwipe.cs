using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class KoreaGeoghraphySwipe : MonoBehaviour
{
    public Scrollbar scrollbar;
    private float scroll_pos = 0;
    private float[] pos;
    private float distance;
    public TMP_Text count;
    private Canvas canvas;
    private AsyncOperationHandle<InteractiveEventData> data;
    public Image artPrefab;
    public Image zoomImg;
    private RectTransform content;
    private int selectedImgNo;
    public TMP_Text nameTMP;
    public TMP_Text affiliatedOrganizationTMP;
    public TMP_Text titleTMP;
    public TMP_Text theFilmLocationTMP;
    public TMP_Text explanationTMP;
    private List<InteractiveTable> subTable;
    public RectTransform rightContent;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        content = GetComponent<RectTransform>();
        Patch();
    }

    private void Update()
    {
        if (!canvas.enabled)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.value;
        }
        else if (pos != null)
        {
            for (selectedImgNo = 0; selectedImgNo < pos.Length; selectedImgNo++)
            {
                if (scroll_pos < pos[selectedImgNo] + (distance / 2) && scroll_pos > pos[selectedImgNo] - (distance / 2))
                {
                    scrollbar.value = Mathf.Lerp(scrollbar.value, pos[selectedImgNo], 0.1f);
                    count.text = (selectedImgNo + 1).ToString() + "/" + pos.Length;
                    try
                    {
                        nameTMP.text = subTable.Find(x => x.key == "name" + selectedImgNo).value;
                        affiliatedOrganizationTMP.text = subTable.Find(x => x.key == "affiliatedOrganization" + selectedImgNo).value;
                        titleTMP.text = subTable.Find(x => x.key == "title" + selectedImgNo).value;
                        theFilmLocationTMP.text = subTable.Find(x => x.key == "theFilmLocation" + selectedImgNo).value;
                        explanationTMP.text = subTable.Find(x => x.key == "explanation" + selectedImgNo).value;
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }
    }

    private void Patch() => PatchAsync().Forget();

    private async UniTask PatchAsync()
    {
        data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);

        await data;

        if (data.Status == AsyncOperationStatus.Succeeded)
        {
            subTable = data.Result.eventSubTable;
            var eventSpriteTable = data.Result.eventSpriteTable;

            for (int i = 0; i < eventSpriteTable.Count; i++)
            {
                Image artImage = Instantiate(artPrefab, content);
                artImage.gameObject.SetActive(true);
                artImage.sprite = eventSpriteTable.Find(x => x.key == "image" + i).value;
            }

            // 개수 및 이동 거리 설정
            pos = new float[transform.childCount];
            if (pos.Length == 1)
            {
                distance = 1;
            }
            else
            {
                distance = 1f / (pos.Length - 1f);
            }
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * i;
            }
            try
            {
                nameTMP.text = subTable.Find(x => x.key == "name" + selectedImgNo).value;
                affiliatedOrganizationTMP.text = subTable.Find(x => x.key == "affiliatedOrganization" + selectedImgNo).value;
                titleTMP.text = subTable.Find(x => x.key == "title" + selectedImgNo).value;
                theFilmLocationTMP.text = subTable.Find(x => x.key == "theFilmLocation" + selectedImgNo).value;
                explanationTMP.text = subTable.Find(x => x.key == "explanation" + selectedImgNo).value;
                LayoutRebuilder.ForceRebuildLayoutImmediate(explanationTMP.rectTransform);
                // 48은 밑에 여백
                rightContent.sizeDelta += new Vector2(0, explanationTMP.rectTransform.sizeDelta.y + 48);
            }
            catch
            {
            }


            // if (eventSpriteTable.Exists(x => x.key == "zoom"))
            // {
            //     selectArtInfoController.zoomButton.gameObject.SetActive(true);
            //     selectArtInfoController.zoomImg.sprite = eventSpriteTable.Find(x => x.key == "zoom").value;
            //     selectArtInfoController.zoomImg.SetNativeSize();
            //     // zoomImg.rectTransform.sizeDelta = zoomImg.sprite.rect.size;
            // }
            canvas.enabled = true;
        }
    }

    public void ZoomBtn()
    {
        zoomImg.sprite = content.GetChild(selectedImgNo).GetComponent<Image>().sprite;
        // zoomImg.SetNativeSize();
        zoomImg.rectTransform.sizeDelta = new Vector2(724, 1024);
    }

    private void OnDestroy()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }
    }
}
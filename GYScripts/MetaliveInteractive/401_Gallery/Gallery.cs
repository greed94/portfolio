using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Gallery : MonoBehaviour
{
    public Image artImg;
    public TMP_Text titleValueTMP;
    public Button titleLinkButton;
    public TMP_Text categoryTMP;
    public TMP_Text dateTMP;
    public RectTransform writerPrefab;
    public RectTransform writerRecttransform;
    private float test;
    private List<InteractiveTable> artTable;
    public RectTransform scroll;
    private AsyncOperationHandle<InteractiveEventData> data;
    private bool openURLBool;

    private void OnEnable()
    {
        Popup.AddCallback(this);
    }

    private void Start()
    {
        // Test();
        SetInfo().Forget();
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
            artTable = data.Result.eventTable;
            var artSpriteTable = data.Result.eventSpriteTable;

            artImg.sprite = artSpriteTable.Find(x => x.key == "art").value;
            titleValueTMP.text = artTable.Find(x => x.key == "title").value;
            //titleLinkButton.onClick.AddListener(() => { OpenURL(artTable.Find(x => x.key == "title").link).Forget(); });
            titleLinkButton.onClick.AddListener(() => OpenURLMetalive.OpenURL(artTable.Find(x => x.key == "title").link));
            categoryTMP.text = artTable.Find(x => x.key == "category").value;
            SetWriterPrefab("writer");
            SetWriterPrefab("painting");
            SetWriterPrefab("original");
            dateTMP.text = artTable.Find(x => x.key == "date").value;
        }
    }

    private void SetWriterPrefab(string key)
    {
        if (!artTable.Exists(x => x.key == key))
            return;

        RectTransform prefab = Instantiate(writerPrefab, writerRecttransform);
        prefab.gameObject.SetActive(true);
        WriterPrefab writerPrefabData = prefab.GetComponent<WriterPrefab>();
        prefab.anchoredPosition = new Vector2(0, test);
        scroll.offsetMax += new Vector2(0, -112f);
        test += -112f;
        string keyKr = null;

        switch (key)
        {
            case "writer":
                keyKr = "작가";
                break;
            case "painting":
                keyKr = "그림";
                break;
            case "original":
                keyKr = "원작";
                break;
        }

        writerPrefabData.Key.text = keyKr;
        writerPrefabData.Value.text = artTable.Find(x => x.key == key).value;
        writerPrefabData.Link.onClick.RemoveAllListeners();
        //writerPrefabData.Link.onClick.AddListener(() => { OpenURL(artTable.Find(x => x.key == key).link).Forget(); });
        writerPrefabData.Link.onClick.AddListener(() => OpenURLMetalive.OpenURL(artTable.Find(x => x.key == key).link));
    }

    private void OnDisable()
    {
        if (data.IsValid())
        {
            Addressables.Release(data);
        }

        Popup.RemoveCallback(this);
    }

    private async UniTaskVoid OpenURL(string url)
    {
        openURLBool = false;
        Application.OpenURL(url);
        await UniTask.Delay(1000);
        if (!openURLBool)
        {
            var hash = new PopupHash()
            {
                title = "\"웹브라우저\"를 설치해 주세요",
                message = "웹 브라우저가 설치되어 있지 않습니다.\n\"Samsung Internet, Chrome, FireFox\" 등의 웹브라우저를 설치해 주세요",
                confirm = "확인"
            };
            Popup.Message(PopupType.General, hash);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            openURLBool = true;
        }
    }
}
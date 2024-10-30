using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Metalive;

public class MapManager : MonoBehaviour, IPopupCallback
{
    [SerializeField]
    private GameObject warpPrefab;
    [SerializeField]
    private Canvas worldMapCanvas;
    [SerializeField]
    private Transform MapCameraTr;
    [SerializeField]
    private Image WorldMapImage;
    [SerializeField]
    private TextMeshProUGUI WorldMapNameTMP;
    // [SerializeField]
    // private RawImage PointRawImage;
    private AsyncOperationHandle<Sprite> ImageData;
    // private RenderTexture renderTexture;
    private AsyncOperationHandle<MetaliveMapData> mapData;
    private Vector3 warpPosition;
    private Vector3 warpRotation;

    private void OnEnable()
    {
        Popup.AddCallback(this);
    }

    private void Start()
    {
        MapSettingAsync().Forget();
    }

    private async UniTask MapSettingAsync()
    {
        GameObject player = PortalManager.player;
        string address = $"{Setting.World.code}_0_4";
        // string rendertextureLabel = "rendertexture_map".Trim();
        mapData = Addressables.LoadAssetAsync<MetaliveMapData>(address);
        await mapData;
        MapCameraTr.gameObject.SetActive(true);
        worldMapCanvas.enabled = true;
        // renderTexture = await Addressables.LoadAssetAsync<RenderTexture>(rendertextureLabel);
        GameObject characterPoint;
        int playerLayerNo = LayerMask.NameToLayer("Player");
        characterPoint = player.transform.Find("PlayerRoot").Find("CharacterPoint").gameObject;
        // characterPoint.SetActive(true);
        // MapCameraTr.GetComponent<Camera>().targetTexture = renderTexture;
        // PointRawImage.texture = renderTexture;

        MapCameraTr.position = mapData.Result.cameraPosition;
        MapCameraTr.rotation = Quaternion.Euler(mapData.Result.cameraRotation);
        MapCameraTr.GetComponent<Camera>().orthographicSize = mapData.Result.cameraSize;
        WorldMapNameTMP.text = mapData.Result.mapName;
        characterPoint.transform.localScale = mapData.Result.pointScale;
        WorldMapImage.sprite = mapData.Result.mapImage;

        List<MapWarpTable> mapWarpTable = mapData.Result.mapWarpTable;

        // 빠른이동 없을 떄
        if (mapWarpTable.Count == 0)
        {
            return;
        }
        else
        {
            float width = WorldMapImage.GetComponent<RectTransform>().rect.width / 2800;
            float height = WorldMapImage.GetComponent<RectTransform>().rect.height / 1156;

            for (int i = 0; i < mapWarpTable.Count; i++)
            {
                GameObject obj = Instantiate(warpPrefab, warpPrefab.transform.parent);
                obj.SetActive(true);
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(mapWarpTable[i].clickPosition.x * width, mapWarpTable[i].clickPosition.y * height);
                Vector3 warpPosition = mapWarpTable[i].warpPosition;
                Vector3 warpRotation = mapWarpTable[i].warpRotation;
                obj.GetComponent<Button>().onClick.AddListener(() => WarpBtn(warpPosition, warpRotation));
            }
        }
    }

    private async UniTaskVoid MapImageSetting(string mapImageName)
    {
        ImageData = Addressables.LoadAssetAsync<Sprite>(mapImageName);
        await ImageData;
        WorldMapImage.sprite = ImageData.Result;
    }

    public void WarpBtn(Vector3 _warpPosition, Vector3 _warpRotation)
    {
        var hash = new PopupHash()
        {
            title = "빠른 이동",
            message = "해당 위치로 이동하시겠어요?",
            yes = "확인",
            no = "취소"
        };
        Popup.Message(PopupType.Choice, PopupCallbackType.Callback, "MapWarp", hash);

        warpPosition = _warpPosition;
        warpRotation = _warpRotation;
    }

    private void OnDisable()
    {
        Popup.RemoveCallback(this);
    }

    private void OnDestroy()
    {
        if (mapData.IsValid())
        {
            Addressables.Release(mapData);
        }
    }

    public void OnPopupUpdate(PopupRedirect redirect)
    {
        if (redirect.code.Equals("MapWarp"))
        {
            if (redirect.type == PopupRedirectType.Yes)
            {
                Player.Reset.positon = warpPosition;
                Player.Reset.rotation = Quaternion.Euler(warpRotation);
                Player.ResetUpdate();
                Portal.Close("13_Map");
            }
            else
            {
                //Debug.Log("닫기");
            }
        }
    }
}
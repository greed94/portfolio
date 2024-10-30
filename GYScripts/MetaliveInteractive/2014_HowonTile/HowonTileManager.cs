using Cysharp.Threading.Tasks;
using Metalive;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public struct HowonTileCount
{
    public string result { get; set; }
    public int data { get; set; }
    public string message { get; set; }
}

public class HowonTileManager : MonoBehaviour, IPopupCallback
{
    #region HowonTileColors
    public Color TileWhite => new Color(1, 1, 1);
    public Color TileLime => new Color(0.878f, 1, 0.682f);
    public Color TileSkyBlue => new Color(0.7137f, 0.9255f, 1);
    public Color TilePurple => new Color(0.741f, 0.8f, 1);
    public Color TilePink => new Color(0.9059f, 0.749f, 1);

    private List<Color> colorList = new List<Color>();
    #endregion HowonTileColors

    //검색결과 타일
    public HowonTile searchTile;
    //검색결과들
    [HideInInspector]
    public List<HowonTile> searchTileList = new List<HowonTile>();
    //HOWON 글자 타일들
    public List<HowonTile> tileList = new List<HowonTile>();

    //DB 응답 체크용
    private WebResponseRoot webResponse;
    //DB 데이터
    public TileRoot tileRoot;
    //검색 DB 데이터
    public TileRoot searchRoot;

    //Search 검색결과 ScrollRect
    public ScrollRect searchTileScroll;

    //등록 및 수정에 쓰일 inputField
    public TMP_InputField uploadInputField;
    //등록 및 수정시 비속어 표시할 text
    public TextMeshProUGUI dirtyTmp;
    //Search 에 쓰일 inputField
    public TMP_InputField searchInputField;

    //클릭하면 나올 타일
    public GameObject messageTile;
    //내용 표시, 수정, 삭제를 담당할 메시지타일
    public LoveTileMessage loveTileMessage;

    //검색결과가 없으면 활성화
    public GameObject noResultObj;

    public GameObject defaultScreen;
    public GameObject uploadScreen;
    public GameObject searchScreen;

    //등록상태,수정상태 체크용 bool
    [SerializeField]
    private bool editStatus;

    //Howon Tile Cnt
    private HowonTileCount howonTileCnt;

    //서버 Howon Tile 갯수(호원대 타일 갯수는 최대 83개)
    private int allTileCnt;
    private int maxTileCnt;

    private int colorArrayCheck;

    //안드로이드에서 두번 보내지는거같은거 체크용 bool
    private bool sendStatus;

    private int editMessageNo;



    private void OnEnable()
    {
        Popup.AddCallback(this);
    }

    private void OnDisable()
    {
        Popup.RemoveCallback(this);
    }

    private async void Start()
    {
        colorList = new List<Color>() { TileWhite, TileLime, TileSkyBlue, TilePurple, TilePink };

        sendStatus = false;

        await GetAllTileCount();

        await GetTileList();

        ActiveTilePrefab();

    }

    //호원대 타일은 기존 러브메시지 타일과 다르게 같은 위치에 덮어쓰기 방식으로 진행됨
    //따라서 전체 타일 갯수(HOWON 타일 글자최대치 83)를 가져와서 계산하는 방식으로 진행
    public async UniTask GetAllTileCount()
    {
        string _url = $"https://{Setting.Server.api}/api/howon/guestListCnt";

        using (UnityWebRequest _request = UnityWebRequest.Get(_url))
        {
            try
            {
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);

                await _request.SendWebRequest();

                howonTileCnt = JsonConvert.DeserializeObject<HowonTileCount>(_request.downloadHandler.text);

                allTileCnt = howonTileCnt.data;

                //Debug.Log($"총 타일갯수: {allTileCnt}");
            }

            catch
            {
                throw;
            }
        }
    }

    public async UniTask GetTileList()
    {
        //전체 타일 갯수(HOWON 타일 글자최대치 83)를 가져와서 계산하는 방식으로 진행
        string _url = $"https://{Setting.Server.api}/api/howon/guestList?page=83";

        using (UnityWebRequest _request = UnityWebRequest.Get(_url))
        {
            try
            {
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);

                await _request.SendWebRequest();

                if (tileRoot.data != null)
                {
                    tileRoot.data.Clear();
                }

                tileRoot = JsonConvert.DeserializeObject<TileRoot>(_request.downloadHandler.text);
            }
            catch
            {
                throw;
            }
        }
    }

    private void ActiveTilePrefab()
    {
        //Debug.Log($"ActiveTileCount: {allTileCnt}");
        if (tileRoot.result == "false")
        {
            return;
        }
        int startTileNo;
        if (allTileCnt > 83)
        {
            maxTileCnt = 83;
            startTileNo = allTileCnt % 83;
            //Debug.Log($"시작 타일번호: {startTileNo}");
            //GetTileList로 불러오는 값은 그대로이나 타일 덮어쓰기 연출을 위해 타일 위치(순서)를 변경
            colorArrayCheck = allTileCnt / 83;
            RelocationTileList(startTileNo, maxTileCnt);

        }
        else
        {
            maxTileCnt = allTileCnt;
            colorArrayCheck = 0;
            for (int i = 0; i < maxTileCnt; i++)
            {
                tileList[i].gameObject.SetActive(true);
                tileList[i].tileImage.color = colorList[0];
                TileListUpdateEvent(i, i);
            }
        }

    }

    //덮어쓰기를 위한 리스트 순서변경
    private void RelocationTileList(int startIndex, int listCount)
    {
        int j = 0;
        startIndex = startIndex % listCount;

        for (int i = startIndex; i < listCount; i++)
        {
            tileList[i].gameObject.SetActive(true);
            if (colorArrayCheck % 5 != 0)
            {
                tileList[i].tileImage.color = colorList[(colorArrayCheck % 5) - 1];
            }
            else
            {
                tileList[i].tileImage.color = colorList[4];
            }

            TileListUpdateEvent(i, j);
            j++;
        }

        for (int i = 0; i < startIndex; i++)
        {
            tileList[i].gameObject.SetActive(true);

            tileList[i].tileImage.color = colorList[colorArrayCheck % 5];


            TileListUpdateEvent(i, j);
            j++;
        }
    }

    private void TileListUpdateEvent(int selectTile, int selectData)
    {
        //Debug.Log("이름이뭐에요: " + tileList[selectTile].gameObject.name);
        //메시지
        tileList[selectTile].message = tileRoot.data[selectData].content;
        //유저코드넘버
        tileList[selectTile].userNo = tileRoot.data[selectData].userCodeNo;
        //메시지번호
        tileList[selectTile].messageNo = tileRoot.data[selectData].messageNo;
        //닉네임
        tileList[selectTile].nickName = tileRoot.data[selectData].nickName;

        tileList[selectTile].SetMessage();
    }

    public async UniTask PostHowonTile()
    {
        WWWForm _form = new WWWForm();
        string _url = $"https://{Setting.Server.api}/api/howon/addGuest";

        _form.AddField("content", uploadInputField.text);

        using (UnityWebRequest _request = UnityWebRequest.Post(_url, _form))
        {
            try
            {
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);

                await _request.SendWebRequest();

                //성공하면 뭐시기
                webResponse = JsonConvert.DeserializeObject<WebResponseRoot>(_request.downloadHandler.text);
                //Debug.Log($"왜안되: {_request.downloadHandler.text}");
                if (webResponse.result == "true")
                {
                    uploadInputField.text = "";
                    defaultScreen.SetActive(true);
                    uploadScreen.SetActive(false);
                    for (int i = 0; i < maxTileCnt; i++)
                    {
                        tileList[i].gameObject.SetActive(false);
                    }

                    await GetAllTileCount();

                    await GetTileList();

                    ActiveTilePrefab();

                }
                //실패하면 저시기
                else if (webResponse.result == "false")
                {
                    dirtyTmp.SetText(webResponse.message);
                }
                sendStatus = false;
            }
            catch (System.Exception e)
            {
                //Debug.Log(e);
                throw;
            }
        }
    }

    public async UniTask EditHowonTile()
    {

        WWWForm _form = new WWWForm();
        string _url = $"https://{Setting.Server.api}/api/howon/updateGuest";

        _form.AddField("content", uploadInputField.text);
        _form.AddField("messageNo", editMessageNo);

        using (UnityWebRequest _request = UnityWebRequest.Post(_url, _form))
        {
            try
            {
                _request.method = "PATCH";
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);
                await _request.SendWebRequest();

                //성공하면 뭐시기
                webResponse = JsonConvert.DeserializeObject<WebResponseRoot>(_request.downloadHandler.text);
                if (webResponse.result == "true")
                {
                    uploadInputField.text = "";
                    defaultScreen.SetActive(true);
                    uploadScreen.SetActive(false);

                    for (int i = 0; i < maxTileCnt; i++)
                    {
                        tileList[i].gameObject.SetActive(false);
                    }

                    await GetAllTileCount();
                    await GetTileList();
                    ActiveTilePrefab();
                }
                //실패하면 저시기
                else if (webResponse.result == "false")
                {
                    dirtyTmp.SetText(webResponse.message);
                }
                sendStatus = false;
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
    }

    //SearchScreen -= SearchButton 할당
    public async void SearchButtonClick()
    {
        for (int i = 0; i < searchTileList.Count; i++)
        {
            Destroy(searchTileList[i].gameObject);
        }
        searchTileList.Clear();

        await SearchTileListAction();
    }

    public async UniTask SearchTileListAction()
    {
        string _url = $"https://{Setting.Server.api}/api/howon/searchGuest?content={searchInputField.text}&offset=0&page=999";

        using (UnityWebRequest _request = UnityWebRequest.Get(_url))
        {
            try
            {
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);

                await _request.SendWebRequest();

                searchRoot = JsonConvert.DeserializeObject<TileRoot>(_request.downloadHandler.text);

                if (searchRoot.result == "true")
                {
                    noResultObj.SetActive(false);
                    CreateSearchTileListPrefab();
                }
                else if (searchRoot.result == "false")
                {
                    noResultObj.SetActive(true);
                }
            }
            catch
            {
                throw;
            }
        }
    }


    public async UniTask DeleteHowonTile()
    {
        string _url = $"https://{Setting.Server.api}/api/howon/deleteGuest?messageNo={editMessageNo}";

        using (UnityWebRequest _request = UnityWebRequest.Delete(_url))
        {
            try
            {
                _request.downloadHandler = new DownloadHandlerBuffer();
                _request.SetRequestHeader("Authorization", Setting.User.accessToken);

                await _request.SendWebRequest();

                if (_request.result == UnityWebRequest.Result.Success)
                {
                    for (int i = 0; i < maxTileCnt; i++)
                    {
                        tileList[i].gameObject.SetActive(false);
                    }

                    await GetAllTileCount();

                    await GetTileList();

                    ActiveTilePrefab();
                }
            }
            catch
            {
                //Debug.Log(e);
                throw;
            }
        }
    }

    private void CreateSearchTileListPrefab()
    {
        if (searchRoot.result == "false")
        {
            return;
        }

        int _count = searchRoot.data.Count;
        searchTileScroll.content.sizeDelta = new Vector2(504 * _count, 0);
        for (int i = 0; i < _count; i++)
        {
            HowonTile _prefab = Instantiate(searchTile, searchTileScroll.content);
            searchTileList.Add(_prefab);
            _prefab.tileImage.color = colorList[i % 5];
            SearchTileListPrefabUpdateEvent(i);
        }
    }

    //검색 결과 타일 값 설정
    private void SearchTileListPrefabUpdateEvent(int select)
    {
        searchTileList[select].message = searchRoot.data[select].content;
        searchTileList[select].nickName = searchRoot.data[select].nickName;
        searchTileList[select].tileManager = this;
        searchTileList[select].rectTransform.anchoredPosition = new Vector3((504 * select), 0, 0);
        searchTileList[select].userNo = searchRoot.data[select].userCodeNo;
        searchTileList[select].messageNo = searchRoot.data[select].messageNo;

        searchTileList[select].SetMessage();
    }

    //UICanvas-UploadEditScreen-SendButton할당
    public async void MessageSendButton()
    {
        if (!sendStatus)
        {
            if (editStatus)
            {
                sendStatus = true;
                await EditHowonTile();
            }
            else
            {
                sendStatus = true;
                await PostHowonTile();
            }
        }
    }


    public void MessageClick(string message, int userNo, int messageNo, string nickName, Color tileColor)
    {
        loveTileMessage.message.SetText("");
        if (userNo == int.Parse(Setting.User.userCodeNo.ToString()))
        {
            loveTileMessage.editBtn.SetActive(true);
            loveTileMessage.deleteBtn.SetActive(true);
        }
        else
        {
            loveTileMessage.editBtn.SetActive(false);
            loveTileMessage.deleteBtn.SetActive(false);
        }
        loveTileMessage.message.SetText(message);
        loveTileMessage.nickName.SetText($"- {nickName} -");
        editMessageNo = messageNo;
        loveTileMessage.tileImage.color = tileColor;
        messageTile.SetActive(true);

    }


    //DefaultScreen-RightMenu-AddMessageTileBtn
    public void AddMessageTileButtonClick()
    {
        uploadInputField.text = "";
        dirtyTmp.SetText("");
        defaultScreen.SetActive(false);
        uploadScreen.SetActive(true);
        messageTile.SetActive(false);
        editStatus = false;
    }
    //DefaultScreen-RightMenu-SearchMessageTileBtn
    public void SearchMessageTileButtonClick()
    {
        searchInputField.text = "";
        for (int i = 0; i < searchTileList.Count; i++)
        {
            Destroy(searchTileList[i].gameObject);
        }
        defaultScreen.SetActive(false);
        searchScreen.SetActive(true);
        searchTileList.Clear();

    }

    //MessageTile-EditButton 할당
    public void EditButtonClick()
    {
        dirtyTmp.SetText("");
        defaultScreen.SetActive(false);
        searchScreen.SetActive(false);
        messageTile.SetActive(false);
        uploadScreen.SetActive(true);
        uploadInputField.text = loveTileMessage.message.text;
        editStatus = true;
    }

    //MessageTile-DeleteButton 할당
    public void DeleteButtonClick()
    {
        var hash = new PopupHash()
        {
            title = "메세지를 삭제하시겠습니까",
            message = "삭제된 메세지는 복구되지 않습니다.",
            yes = "확인",
            no = "취소"
        };

        Popup.Message(PopupType.Choice, PopupCallbackType.Callback, "DeleteTile", hash);
    }

    public void OnPopupUpdate(PopupRedirect redirect)
    {
        if (redirect.code.Equals("DeleteTile"))
        {
            if (redirect.type == PopupRedirectType.Yes)
            {
                DeleteTile();
            }
            else
            {
                //Debug.Log("닫기");
            }
        }
    }

    public async void DeleteTile()
    {
        searchScreen.SetActive(false);
        for (int i = 0; i < searchTileList.Count; i++)
        {
            Destroy(searchTileList[i].gameObject);
        }
        searchTileList.Clear();
        messageTile.SetActive(false);
        defaultScreen.SetActive(true);
        await DeleteHowonTile();
    }
}

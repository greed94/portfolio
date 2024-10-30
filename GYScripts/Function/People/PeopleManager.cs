using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Metalive
{

    [Serializable]
    public class PeopleRedirect
    {
        public string result { get; set; }

        public PeolpeData data { get; set; }

        public string message { get; set; }

        [Serializable]
        public class PeolpeData
        {
            public string no { get; set; }
            public string userCodeNo { get; set; }
            public string nickname { get; set; }
            public string userId { get; set; }
            public string followingYn { get; set; }

            public PeopleImagePath profileImgPath { get; set; }
        }

        [Serializable]
        public class PeopleImagePath
        {
            public string SD { get; set; }
            public string ORIGIN { get; set; }
        }
    }

    [RequireComponent(typeof(PeopleController))]
    public class PeopleManager : MonoBehaviour, IInRoomCallbacks
    {

        #region Variable

        private PeopleController controller;
        private List<PeopleItem> controllerItems;

        //PC Dummy 코드
        private RandomNickNameList rnnl;
        #endregion



        #region Lifecycle

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            controller = GetComponent<PeopleController>();
            if (controller != null)
            {
                Patch();
            }
            //PC Dummy 코드
            RandomNickNameRead();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller = null;
            }

            if (controllerItems != null || controllerItems.Count > 0)
            {
                for (int i = 0; i < controllerItems.Count; i++)
                {
                    if (controllerItems[i].peopleImage != null)
                    {
                        Destroy(controllerItems[i].peopleImage);
                    }
                }
            }
        }

        #endregion



        #region Callback

        // 플레이어 입장
        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            AddPatch(newPlayer.NickName);
        }

        // 플레이어 퇴장
        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            RemovePatch(otherPlayer.NickName);
        }

        public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            return;
        }

        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            return;
        }

        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            return;
        }

        #endregion



        #region Method

        // ==================================================
        // [ General ]
        // ==================================================
        private async void Patch()
        {
            if (controller.peopleItem.gameObject.activeSelf)
            {
                controller.peopleItem.gameObject.SetActive(false);
            }

            if (controllerItems == null)
            {
                controllerItems = new List<PeopleItem>();
            }

            // 현재 접속중인 방 플레이어 목록 가져오기
            var list = new List<Photon.Realtime.Player>(PhotonNetwork.CurrentRoom.Players.Values);
            list.Remove(list.Find(x => x.NickName == Setting.User.userCodeNo.ToString()));

            controller.peopleScroll.content.sizeDelta = new Vector2(1280f, 220f * (list.Count + 1));

            // 나 자신에 대한 표시 
            await PatchAsync(Setting.User.userCodeNo.ToString());
            for (int i = 0; i < list.Count; i++)
            {
                // 리스트 목록에 있는 유저 표시
                PatchAsync(list[i].NickName).Forget();
            }
        }
        //PC Dummy Code
        private void RandomNickNameRead()
        {
            TextAsset json = Resources.Load<TextAsset>("RandomNickname");
            rnnl = JsonConvert.DeserializeObject<RandomNickNameList>(json.text);
        }

        // 서버 
        private async UniTask PatchAsync(string userCodeNumber)
        {
            var url = $"https://{Setting.Server.api}/api/ingame/mate?userCodeNo={userCodeNumber}";
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var redirect = JsonConvert.DeserializeObject<PeopleRedirect>(request.downloadHandler.text);
                    if (redirect == null)
                    {
                        return;
                    }
                    else
                    {
                        // 아이템 생성
                        var obj = Instantiate(controller.peopleItem, controller.peopleScroll.content);
                        var script = obj.GetComponent<PeopleItem>();
                        script.peopleCode = redirect.data.userCodeNo;
                        controllerItems.Add(script);
                        var index = controllerItems.IndexOf(script);

                        // 아이템이 있을경우 위치 재조정
                        if (controllerItems.Count > 0)
                        {
                            obj.anchoredPosition = new Vector3(0, -220 * index, 0);
                            obj.sizeDelta = controller.peopleItemSize;
                        }

                        // 이름이 있을 경우만 설정
                        if (!string.IsNullOrEmpty(redirect.data.nickname))
                        {
                            script.peopleName.text = redirect.data.nickname;
                            script.peopleName.rectTransform.sizeDelta = new Vector2(script.peopleName.preferredWidth + 20f, 80f);
                        }
                        //PC Dummy용 코드
                        else if (string.IsNullOrEmpty(redirect.data.nickname))
                        {
                            int random1 = UnityEngine.Random.Range(0, 10);
                            int random2 = UnityEngine.Random.Range(0, 50);
                            script.peopleName.text = rnnl.Data[random1][random2];


                            script.peopleName.rectTransform.sizeDelta = new Vector2(script.peopleName.preferredWidth + 20f, 80f);
                        }

                        // 유저 아이디가 있을경우 이름 지정
                        if (!string.IsNullOrEmpty(redirect.data.userId))
                        {
                            script.peopleID.text = redirect.data.userId;
                        }

                        // 유저 코드가 나인지 다른사람인지 체크
                        if (userCodeNumber == Setting.User.userCodeNo.ToString())
                        {
                            script.peopleMy.gameObject.SetActive(true);
                            script.peopleFollow.gameObject.SetActive(false);
                        }
                        else
                        {
                            // 팔로잉 체크
                            if (redirect.data.followingYn.Equals("Y"))
                            {
                                script.IsFollow = true;
                                script.peopleFollowShadow.color = new Color(203 / 255f, 203 / 255f, 255 / 255f, 255 / 255f);
                                script.peopleFollowBackground.color = Color.white;
                                script.peopleFollowText.text = "팔로잉";
                                script.peopleFollowText.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                            }
                            else
                            {
                                script.IsFollow = false;
                                script.peopleFollowShadow.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                                script.peopleFollowBackground.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                                script.peopleFollowText.text = "팔로우";
                                script.peopleFollowText.color = Color.white;
                            }

                            script.peopleFollowButton.onClick.RemoveAllListeners();
                            script.peopleFollowButton.onClick.AddListener(() => { FollowAsync(index).Forget(); });

                            script.peopleMy.gameObject.SetActive(false);
                            script.peopleFollow.gameObject.SetActive(true);
                            //PC Dummy용 코드
                            if (script.peopleCode == "0")
                            {
                                script.peopleFollow.gameObject.SetActive(false);
                            }
                        }

                        // 이미지 체크
                        if (string.IsNullOrEmpty(redirect.data.profileImgPath.SD))
                        {
                            script.peopleImage.gameObject.SetActive(false);
                        }
                        else
                        {
                            script.peopleImage.sprite = await GetSprite(redirect.data.profileImgPath.SD);
                            if (script.peopleImage.sprite == null)
                            {
                                script.peopleImage.gameObject.SetActive(false);
                            }
                            else
                            {
                                script.peopleImage.gameObject.SetActive(true);
                            }
                        }

                        if (!script.peopleItem.gameObject.activeSelf)
                        {
                            script.peopleItem.gameObject.SetActive(true);
                        }
                    }
                }
            }
            controller.peopleScroll.content.sizeDelta = new Vector2(1280f, 220f * (controller.peopleScroll.content.childCount - 1));
        }

        // 유저 목록 추가
        private void AddPatch(string userCodeNumber)
        {
            PatchAsync(userCodeNumber).Forget();
        }

        // 유저 목록 제거
        private void RemovePatch(string userCodeNumber)
        {
            var item = controllerItems.Find(x => x.peopleCode == userCodeNumber);
            if (item != null)
            {
                controllerItems.Remove(item);
                DestroyImmediate(item.gameObject);

                for (int i = 0; i < controllerItems.Count; i++)
                {
                    controllerItems[i].peopleItem.anchoredPosition = new Vector3(0, (-220f * i), 0);
                    controllerItems[i].peopleItem.sizeDelta = controller.peopleItemSize;
                }
            }
            controller.peopleScroll.content.sizeDelta = new Vector2(1280f, 220f * (controller.peopleScroll.content.childCount - 1));
        }

        // 이미지 가져오기
        private async UniTask<Sprite> GetSprite(string url)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    return null;
                }
            }
        }


        // ==================================================
        // [ Funciton ]
        // ==================================================

        // 서버에 팔로잉, 팔로우 전달
        private async UniTaskVoid FollowAsync(int index)
        {
            if (controllerItems[index].IsFollow)
            {
                // 팔로우 취소
                WWWForm form = new WWWForm();
                form.AddField("targetUserCodeNo", int.Parse(controllerItems[index].peopleCode));

                string url = $"https://{Setting.Server.api}/api/ingame/following-cancel";
                using (var request = UnityWebRequest.Post(url, form))
                {
                    request.SetRequestHeader("Authorization", Setting.User.accessToken);
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        controllerItems[index].IsFollow = false;
                        controllerItems[index].peopleFollowShadow.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                        controllerItems[index].peopleFollowBackground.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                        controllerItems[index].peopleFollowText.text = "팔로우";
                        controllerItems[index].peopleFollowText.color = Color.white;
                    }
                }
            }
            else
            {
                // 팔로우
                WWWForm form = new WWWForm();
                form.AddField("targetUserCodeNo", int.Parse(controllerItems[index].peopleCode));

                string url = $"https://{Setting.Server.api}/api/ingame/following";
                using (var request = UnityWebRequest.Post(url, form))
                {
                    request.SetRequestHeader("Authorization", Setting.User.accessToken);
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        controllerItems[index].IsFollow = true;
                        controllerItems[index].peopleFollowShadow.color = new Color(203 / 255f, 203 / 255f, 255 / 255f, 255 / 255f);
                        controllerItems[index].peopleFollowBackground.color = Color.white;
                        controllerItems[index].peopleFollowText.text = "팔로잉";
                        controllerItems[index].peopleFollowText.color = new Color(131 / 255f, 164 / 255f, 245 / 255f, 255 / 255f);
                    }
                }
            }
        }

        #endregion

    }

}
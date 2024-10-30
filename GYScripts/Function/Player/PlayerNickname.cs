using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Metalive
{
    [Serializable]
    public class NicknameTable
    {
        public string result;
        public string data;
        public string message;
    }
    //PC Dummy 코드
    public class RandomNickNameList
    {
        public List<List<string>> Data { get; set; }
    }



    [RequireComponent(typeof(PhotonView))]
    public class PlayerNickname : MonoBehaviour
    {

        #region Variable

        [SerializeField]
        private TMP_Text nickname;
        private Transform nicknameCamera;

        //PC Dummy 코드
        private RandomNickNameList rnnl;

        public int countNickname = 0;

        #endregion



        #region Lifecycle

        private void Start()
        {
            nicknameCamera = Camera.main.transform;

            // 포톤을 통해 유저 코드를 가져온다
            var photon = GetComponent<PhotonView>();
            if (photon != null)
            {
                Patch(photon.Owner.NickName).Forget();
            }
            //PC Dummy 코드
            RandomNickNameRead();
        }

        //PC Dummy Code
        private void RandomNickNameRead()
        {
            TextAsset json = Resources.Load<TextAsset>("RandomNickname");
            rnnl = JsonConvert.DeserializeObject<RandomNickNameList>(json.text);
        }

        private void Update()
        {
            if (nickname)
            {
                nickname.transform.rotation = nicknameCamera.rotation;
            }
        }

        #endregion



        #region Patch

        // 유저코드를 통해 서버에서 닉네임 정보를 가져온다
        private async UniTaskVoid Patch(string userCode)
        {
            string url = $"https://{Setting.Server.api}/api/user/others-nickname?userCodeNo={userCode}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return;
                }
                else
                {
                    var table = JsonConvert.DeserializeObject<NicknameTable>(request.downloadHandler.text);

                    if (table != null)
                    {
                        if (Setting.User.userCodeNo.ToString() == userCode)
                        {
                            // 유저코드랑 현재 셋팅에 등록된 유저코드가 같을경우 노랑색 색깔지정
                            nickname.color = new Color(255 / 255f, 250 / 255f, 109 / 255f, 255 / 255f);
                            nickname.text = table.data;
                            //nickname.text = table.data + countNickname.ToString();
                            //countNickname++;
                        }
                        else
                        {
                            // 유저코드랑 셋팅에 등록된 유저코드가 다를경우 흰색으로 색깔지정
                            nickname.color = Color.white;
                            nickname.text = table.data;
                            //PC Dummy용 코드
                            if (string.IsNullOrEmpty(table.data))
                            {
                                int random1 = UnityEngine.Random.Range(0, 10);
                                int random2 = UnityEngine.Random.Range(0, 50);
                                nickname.text = rnnl.Data[random1][random2];

                            }
                        }
                    }
                }
            }
        }

        #endregion



        #region Method

        // 닉네임에 Transform 위치를 재 지정한다
        public void NicknameLocation(float location)
        {
            if (nickname)
            {
                nickname.rectTransform.anchoredPosition = new Vector3(0, location, 0);
            }
        }

        #endregion

    }

}
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Metalive
{
    public class WanderingNPCNickname : MonoBehaviour
    {
        #region Variable

        [SerializeField]
        private TMP_Text nickname;
        private Transform nicknameCamera;

        public int countNickname = 0;

        #endregion

        private void Start()
        {
            var photon = GetComponent<PhotonView>();
            if (photon != null)
            {
                //Patch(photon.Owner.NickName).Forget();
                nickname.color = Color.white;
                nickname.text = photon.ViewID.ToString();
            }
        }

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
                        }
                    }
                }
            }
        }

        // 닉네임에 Transform 위치를 재 지정한다
        public void NicknameLocation(float location)
        {
            if (nickname)
            {
                nickname.rectTransform.anchoredPosition = new Vector3(0, location, 0);
            }
        }
    }
}
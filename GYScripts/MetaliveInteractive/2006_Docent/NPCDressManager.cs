using Cysharp.Threading.Tasks;
using Metalive;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class NPCDressManager : MonoBehaviour
{
    public PlayerAvatar pa;
    private void Start()
    {
        //string npcNo = Setting.World.code.ToString();
        string npcNo = "2";
        pa = GetComponent<PlayerAvatar>();
        NPCMeti(npcNo);
    }

    public void NPCMeti(string code)
    {
        PlayerMetiAsync(code).Forget();
    }

    private async UniTaskVoid PlayerMetiAsync(string code)
    {
        string url = $"https://{Setting.Server.api}/api/ingame/npc?npcNo={code}";
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
                Debug.Log(request.downloadHandler.text);
                pa.playerAvatarData = JsonConvert.DeserializeObject<AvatarDressRedirect>(request.downloadHandler.text).data;
                pa.MetiSex(pa.playerAvatarData.sex);
                pa.MetiDress();
            }
        }
    }
}

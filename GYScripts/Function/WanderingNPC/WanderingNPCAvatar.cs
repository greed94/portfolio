using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Metalive
{
    public class WanderingNPCAvatar : PlayerAvatar
    {
        //public async UniTask WanderingNPC(string npcNo)
        //{
        //    await WanderingNPCAsync(npcNo);
        //}

        //private async UniTask WanderingNPCAsync(string npcNo)
        //{
        //    string url = $"https://{Setting.Server.api}/api/ingame/npc?npcNo={npcNo}";
        //    using (UnityWebRequest request = UnityWebRequest.Get(url))
        //    {
        //        request.SetRequestHeader("Authorization", Setting.User.accessToken);
        //        await request.SendWebRequest();

        //        if (request.result != UnityWebRequest.Result.Success)
        //        {
        //            return;
        //        }
        //        else
        //        {
        //            playerAvatarData = JsonConvert.DeserializeObject<AvatarDressRedirect>(request.downloadHandler.text).data;
        //            MetiSex(playerAvatarData.sex);
        //            MetiDress();
        //        }
        //    }
        //}
    }
}
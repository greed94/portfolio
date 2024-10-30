using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metalive
{
    [RequireComponent(typeof(PhotonView))]
    public class WanderingNPCManager : MonoBehaviour
    {

        //[SerializeField]
        //private Transform wanderingNPCAvatar;
        //private PhotonView pv;

        //private void Start()
        //{
        //    pv = GetComponent<PhotonView>();

        //    NPCAvatarPatch();
        //}

        // NPC 아바타 변경
        private void NPCAvatarPatch()
        {
            //var script = wanderingNPCAvatar.GetComponentInChildren<WanderingNPCAvatar>();
            //if (script != null)
            //{
            //    string user = GetComponent<PhotonView>().ViewID.ToString();
            //    string userNo = user.Substring(user.Length - 1);
                //Debug.Log("적용할 유저넘버: " + userNo);

                //script.WanderingNPC(userNo).Forget();
            //}
        }
    }
}
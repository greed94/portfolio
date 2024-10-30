using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metalive
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerManager : MonoBehaviour
    {

        #region Variable

        [SerializeField]
        private Transform player;
        [SerializeField]
        private Transform playerAvatar;

        #endregion



        #region Check

        private bool IsMine { get; set; } = false;

        #endregion



        #region Lifecycle

        private void Awake()
        {
            IsMine = GetComponent<PhotonView>().IsMine;
        }

        private void Start()
        {
            PlayerPatch();
            PlayerAvatarPatch();
        }

        #endregion



        #region Method

        // 플레이어 레이어 지정
        private void PlayerPatch()
        {
            if (IsMine)
            {
                player.gameObject.layer = 15;
            }
            else
            {
                player.gameObject.layer = 16;
            }
        }

        // 플레이어 아바타 변경
        private void PlayerAvatarPatch()
        {
            var script = playerAvatar.GetComponentInChildren<PlayerAvatar>();
            if (script != null)
            {
                var user = GetComponent<PhotonView>().Owner.NickName;
                script.PlayerMeti(user).Forget();
            }
        }

        #endregion

    }

}
using Metalive;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class WanderingNPCController : MonoBehaviour, IPunObservable
{

    // =============================================
    // [ Player ]
    // ============================================= 
    [Header("[ Player ]")]
    [SerializeField]
    private Transform player;

    // 플레이어 네트워크가 자기 자신인지 상대인지를 체크
    public bool IsMine { get; set; } = true;

    private PhotonView pv { get; set; }

    private Vector3 playerPosition = Vector3.zero;
    private Quaternion playerRotation = Quaternion.identity;

    [SerializeField]
    private Animator playerAnimator;
    private int playerHash = 0;
    private int playerTargetHash = 0;
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int jumpHash = Animator.StringToHash("Jump");


    [SerializeField]
    private int playerState = 0;
    [SerializeField]
    private int playerCalculateState = 0;

    private float playerSpeed = 0;
    private float walkSpeed = 1f;
    private float runSpeed = 2f;

    //private void Awake()
    //{
    //    pv = GetComponent<PhotonView>();
    //    IsMine = pv.IsMine;
    //}

    //private void Start()
    //{
    //    playerCalculateState = 2;
    //    playerAnimator.Play(runHash);
    //}

    //private void FixedUpdate()
    //{
    //    if (IsMine)
    //    {

    //    }
    //    else
    //    {
    //        WanderingNPCNetwork();
    //    }
    //}

    //// 플레이어 상태 업데이트
    //private void PlayerUpdate()
    //{
    //    if (playerState != playerCalculateState)
    //    {
    //        playerState = playerCalculateState;
    //        if (playerState == 2)
    //        {
    //            // 뛰기 상태
    //            playerSpeed = runSpeed;
    //            playerAnimator.Play(runHash);

    //        }
    //        else if (playerState == 1)
    //        {
    //            // 걷기 상태
    //            playerSpeed = walkSpeed;
    //            playerAnimator.Play(walkHash);

    //        }
    //        else
    //        {
    //            // 멈춤 상태
    //            playerSpeed = 0;
    //            playerAnimator.Play(idleHash);

    //        }
    //    }
    //}

    //// 플레이어 네트워크에서 값을 받아 이동 변경
    //private void WanderingNPCNetwork()
    //{
    //    player.position = Vector3.Lerp(player.position, playerPosition, Time.deltaTime * 30);
    //    player.rotation = Quaternion.Lerp(player.rotation, playerRotation, Time.deltaTime * 30);
    //}



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting)
        //{
        //    stream.SendNext(player.position);
        //    stream.SendNext(player.rotation);
        //    stream.SendNext(playerState);
        //    stream.SendNext(playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        //}
        //else
        //{
        //    playerPosition = (Vector3)stream.ReceiveNext();
        //    playerRotation = (Quaternion)stream.ReceiveNext();
        //    playerCalculateState = (int)stream.ReceiveNext();
        //    playerTargetHash = (int)stream.ReceiveNext();
        //}
    }
}

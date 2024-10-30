using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine.Networking;

namespace Metalive
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerController : MonoBehaviour, InputSystem.ICommandActions, IPlayerCallback, IKeyboardCallback, IPopupCallback, IPunObservable
    {

        #region Variable

        // =============================================
        // [ PlayerInput -> InputSystem ]
        // =============================================
        [Header("[ InputSystem ]")]
        [SerializeField]
        private PlayerInput playerInput;
        private InputSystem playerInputSystem { get; set; }

        [SerializeField]
        private Vector3 playerInputVector = Vector3.zero;
        [SerializeField]
        private Vector3 playerMoveVector = Vector3.zero;


        // =============================================
        // [ Player ]
        // ============================================= 
        [Header("[ Player ]")]
        [SerializeField]
        private Transform player;
        [SerializeField]
        private Transform playerTarget;
        [SerializeField]
        private Transform playerAvatar;
        public Transform playerCamera { get; set; }


        // =============================================
        // [ PlayerInput -> InputSystem ]
        // =============================================
        [SerializeField]
        private CharacterController playerController;
        private Vector3 playerPosition = Vector3.zero;
        private Quaternion playerRotation = Quaternion.identity;

        [SerializeField]
        private int playerState = 0;
        [SerializeField]
        private int playerCalculateState = 0;


        private float playerSpeed = 0;
        private float walkSpeed = 1f;
        private float runSpeed = 2f;

        // Velocity        
        private Vector3 playerVelocity = Vector3.zero;
        [SerializeField]
        private float jumpHeight = 0.55f;
        [SerializeField]
        private float gravityValue = -9.81f;

        // Slope
        private RaycastHit playerSlopeHit;


        // =============================================
        // [ PlayerAnimator ]
        // => Animator.StringToHash() => 애니메이터의 애니메이션을 미니 지정하면 메모리를 효과적으로 사용할수 있다.
        // => Animator.Play() => 애니메이션 전체를 찾기때문에 시간이 다소 걸림 => 검색 시간이 길다는건 결국 연산을 많이 한다는 소리
        // ============================================= 
        [SerializeField]
        private Animator playerAnimator;
        private int playerHash = 0;
        private int playerTargetHash = 0;
        private readonly int idleHash = Animator.StringToHash("Idle");
        private readonly int walkHash = Animator.StringToHash("Walk");
        private readonly int runHash = Animator.StringToHash("Run");
        private readonly int jumpHash = Animator.StringToHash("Jump");


        // =============================================
        // [ PlayerInteractive ]
        // ============================================= 
        private string playerKey = "";
        private string playerValue = "";


        // =============================================
        // [ PlayerSound ]
        // =============================================
        [SerializeField]
        private AudioSource playerAudioSource;
        private AudioClip idleClip;
        private AudioClip walkClip;
        private AudioClip runClip;


        // =============================================
        // [ PlayerRiding ]
        // ============================================= 
        private string riding = "";


        // =============================================
        // [ PlayerPedometer ]
        // ============================================= 
        private float pedometerTimer = 0f;
        private float pedometerTargetTimer = 0.5f;

        // =============================================
        // [ PlayerNetwork ]
        // ============================================= 
        private PhotonView playerNetwork { get; set; }

        #endregion



        #region Check

        [Space(20)]
        [Header("[ Option ]")]

        // 플레이어의 이동중인 여부 체크
        public bool IsMove = true;

        // 플레이어 인터렉티브 진행 여부 => 캐릭터의 애니메이션을 진행중인지를 체크하기 위해 사용
        public bool IsInteractive = false;

        // 플레이어의 라이딩 여부 체크
        public bool IsRiding = false;



        // 플레이어 네트워크가 자기 자신인지 상대인지를 체크
        public bool IsMine { get; set; } = true;

        // 플레이어의 멈춤을 체크하기 위해 사용 => 이동중인 여부 체크하는 IsMove랑 통합이 가능하지 않을까?
        public bool IsPause
        {
            get
            {
                return playerMoveVector == Vector3.zero;
            }
        }

        // 플레이어가 땅인지 아닌지 체크
        public bool IsGrounded
        {
            get
            {
                return playerController.isGrounded;
            }
        }

        // 플레이어가 경사면에 있는지 아닌지를 체크
        public bool IsSlope
        {
            get
            {
                if (Physics.Raycast(player.position, Vector3.down, out playerSlopeHit, 1f))
                {
                    if (playerSlopeHit.normal != Vector3.up)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        // 플레이어의 중력 부분을 체크
        public bool IsVelocity
        {
            get
            {
                return playerVelocity.y <= 0 ? true : false;
            }
        }

        #endregion



        #region Lifecycle

        private void Awake()
        {
            // 네트워크를 체크해서 스크립트의 삭제 여부를 판단
            playerNetwork = GetComponent<PhotonView>();
            IsMine = playerNetwork.IsMine;
            if (!IsMine)
            {
                Clear();
            }
            else
            {
                if (Interactive.IsEvent)
                {
                    Interactive.EventUpdate(false);
                }
            }

            if (!IsMove)
            {
                IsMove = true;
            }

            if (IsInteractive)
            {
                IsInteractive = false;
            }
        }

        private void OnEnable()
        {
            if (IsMine)
            {
                Connect();
            }
        }

        private void Start()
        {
            if (IsMine)
            {
                playerCamera = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (IsMine)
            {
                PlayerCalculate();
            }

        }

        private void FixedUpdate()
        {
            if (IsMine)
            {
                if (IsMove)
                {
                    PlayerMove();
                    PlayerUpdate();
                    PlayerPedometer();
                }
            }
            else
            {
                if (IsMove)
                {
                    PlayerNetwork();
                    PlayerNetworkUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            if (IsMine)
            {
                PlayerInteractive();
            }
        }

        private void OnDisable()
        {
            if (IsMine)
            {
                DisConnect();
            }
        }

        private void OnDestroy()
        {
            if (idleClip != null)
            {
                Resources.UnloadAsset(idleClip);
                idleClip = null;
            }

            if (walkClip != null)
            {
                Resources.UnloadAsset(walkClip);
                walkClip = null;
            }

            if (runClip != null)
            {
                Resources.UnloadAsset(runClip);
                runClip = null;
            }
        }

        #endregion



        #region Patch

        // ==================================================
        // [ Component ]
        // ==================================================
        private void Clear()
        {
            InputClear();
        }

        private void Connect()
        {
            InputConnect();
            PlayerConnect();
            KeyboardConnect();
            PopupConnect();
        }

        private void DisConnect()
        {
            InputDisConnect();
            PlayerDisConnect();
            KeyboardDisConnect();
            PopupDisConnect();
        }


        // ==================================================
        // [ Input ]
        // ==================================================

        // Input 활성화
        private void InputConnect()
        {
            if (playerInput)
            {
                playerInputSystem = new InputSystem();
                playerInputSystem.Command.SetCallbacks(instance: this);
                playerInputSystem.Enable();
            }
        }

        // Input 비 활성화
        private void InputDisConnect()
        {
            if (playerInput)
            {
                playerInputSystem.Disable();
                playerInputSystem.Command.RemoveCallbacks(instance: this);
                playerInputSystem = null;
            }
        }

        // Input 삭제
        private void InputClear()
        {
            if (playerInput)
            {
                DestroyImmediate(playerInput);
            }
        }



        // ==================================================
        // [ Player ]
        // ==================================================

        // 플레이어 Callback 등록
        private void PlayerConnect()
        {
            Player.AddCallback(this);
        }

        // 플레이어 Callback 삭제
        private void PlayerDisConnect()
        {
            Player.RemoveCallback(this);
        }


        // ==================================================
        // [ Keyboard ]
        // ==================================================

        // 키보드 Callback 등록
        private void KeyboardConnect()
        {
            Keyboard.AddCallback(this);

            // ↓수정
            if ((SettingWorldType)Setting.World.type == SettingWorldType.Gallery)
            {
                Keyboard.GalleryKeyboard();
            }
            else
            {
                Keyboard.GeneralKeyboard();
            }
        }

        // 키보드 Callback 삭제
        private void KeyboardDisConnect()
        {
            Keyboard.RemoveCallback(this);
        }


        // ==================================================
        // [ Popup ]
        // ==================================================

        // 팝업 Callback 등록
        private void PopupConnect()
        {
            Popup.AddCallback(this);
        }

        // 팝업 Callback 삭제
        private void PopupDisConnect()
        {
            Popup.RemoveCallback(this);
        }

        #endregion



        #region Callback

        // ==================================================
        // [ InputSystem.ICommandActions ]
        // ==================================================

        // ==[ Move ] =======================================
        // Input system에서 이동 값을 받도록 설정
        // Left 조이스틱으로 사용
        public void OnMove(InputAction.CallbackContext context)
        {
            playerInputVector = context.ReadValue<Vector2>();
            if (playerInputVector != Vector3.zero)
            {
                IsMove = true;
            }
        }

        // ==[ Behavior ] =======================================
        // Right 메인 키
        public void OnZero(InputAction.CallbackContext context)
        {
            PlayerBehavior(Player.Keyboard.Zero.code, Player.Keyboard.Zero.hash);
        }

        // Right 선택 키
        public void OnFrist(InputAction.CallbackContext context)
        {
            PlayerBehavior(Player.Keyboard.Frist.code, Player.Keyboard.Frist.hash);
        }


        // ==================================================
        // [ IPlayerCallback ]
        // ==================================================

        // 플레이어 위치 값 제설정
        public void OnPlayerResetUpdate()
        {
            PlayerReset(Player.Reset.positon, Player.Reset.rotation);
        }

        // 플레이어 이모션 실행
        public void OnPlayerEmotionUpdate()
        {
            PlayerAnimation(Player.Emotion.animation);
        }

        // 플레이어 라이딩 실행 + 네트워크 처리
        public void OnPlayerRidingUpdate()
        {
            if (Player.Riding.IsUse)
            {
                // ↑
                if (riding.Equals(Player.Riding.address))
                {
                    return;
                }
                else
                {
                    if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                    {
                        playerNetwork.RPC("PlayerRidingClear", RpcTarget.All, Setting.User.userCodeNo.ToString(), "");
                    }
                    else
                    {
                        PlayerRidingClear(Setting.User.userCodeNo.ToString(), "");
                    }
                }

                if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                {
                    playerNetwork.RPC("PlayerRiding", RpcTarget.All, Setting.User.userCodeNo.ToString(), Player.Riding.address);
                }
                else
                {
                    RidingAsync(Setting.User.userCodeNo.ToString(), Player.Riding.address).Forget();
                }
            }
            else
            {
                // ↓
                if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                {
                    playerNetwork.RPC("PlayerRidingClear", RpcTarget.All, Setting.User.userCodeNo.ToString(), "");
                }
                else
                {
                    PlayerRidingClear(Setting.User.userCodeNo.ToString(), "");
                }
            }
        }

        // 플레이어 값 재설정
        public void OnPlayerPropertyUpdate()
        {
            if (Player.Property.walkSpeed <= 0)
            {
                walkSpeed = 1;
            }
            else
            {
                walkSpeed = Player.Property.walkSpeed;
            }

            if (Player.Property.runSpeed <= 0)
            {
                runSpeed = 2;
            }
            else
            {
                runSpeed = Player.Property.runSpeed;
            }

            if (Player.Property.jumpHeight <= 0)
            {
                jumpHeight = 0.55f;
            }
            else
            {
                jumpHeight = Player.Property.jumpHeight;
            }

            if (Player.Property.gravityValue >= 0)
            {
                gravityValue = -9.81f;
            }
            else
            {
                gravityValue = Player.Property.gravityValue;
            }

            if (playerAnimator != null)
            {
                var script = playerAnimator.GetComponent<PlayerAvatar>();

                if (playerState == 2)
                {
                    playerSpeed = runSpeed;
                    playerAnimator.Play(runHash);
                    script.AnimationSynchUpdate(runHash);
                }
                else if (playerState == 1)
                {
                    playerSpeed = walkSpeed;
                    playerAnimator.Play(walkHash);
                    script.AnimationSynchUpdate(walkHash);
                }
                else
                {
                    playerSpeed = 0;
                    playerAnimator.Play(idleHash);
                    script.AnimationSynchUpdate(idleHash);
                }
            }
        }


        // ==================================================
        // [ IKeyboardCallback ]
        // ==================================================

        // 일반 월드 키 설정
        public void OnGeneralKeyboard()
        {
            var zero = "Jump";
            Player.Keyboard.Zero.code = zero;
            Player.Keyboard.Zero.hash = Animator.StringToHash(zero);

            var frist = "Bridge";
            Player.Keyboard.Frist.code = frist;
            Player.Keyboard.Frist.hash = Animator.StringToHash(frist);
        }

        // 갤러리 키 설정
        public void OnGalleryKeyboard()
        {
            var zero = "Idle";
            Player.Keyboard.Zero.code = zero;
            Player.Keyboard.Zero.hash = Animator.StringToHash(zero);

            var frist = "Bridge";
            Player.Keyboard.Frist.code = frist;
            Player.Keyboard.Frist.hash = Animator.StringToHash(frist);
        }

        // 라이딩 키 설정
        public void OnRidingKeyboard()
        {
            var zero = "Idle";
            Player.Keyboard.Zero.code = zero;
            Player.Keyboard.Zero.hash = Animator.StringToHash(zero);

            var frist = "Riding";
            Player.Keyboard.Frist.code = frist;
            Player.Keyboard.Frist.hash = Animator.StringToHash(frist);
        }


        // ==================================================
        // [ IPopupCallback ]
        // ==================================================

        // 팝업 리턴값 받기
        public void OnPopupUpdate(PopupRedirect redirect)
        {
            if (redirect.code.Equals("player_teleport"))
            {
                Debug.Log("Teleport");
            }
        }

        #endregion



        #region Method

        // ==================================================
        // [ Reset ]
        // ==================================================

        // 플레이어 위치값 재조정
        private void PlayerReset() => PlayerReset(playerPosition, playerRotation);
        internal void PlayerReset(Vector3 resetPosition, Quaternion resetRotation)
        {
            if (playerController)
            {
                // 플레이어 컨트롤러 사용시 플레이어 값 재설정을 위해 컨트롤러 업데이트 끄는 형태?
                playerController.enabled = false;
                player.SetLocalPositionAndRotation(resetPosition, resetRotation);
                playerController.enabled = true;
            }
            else
            {
                // 컨트롤러없이 위치값 재 지정시 바로 지정
                player.SetLocalPositionAndRotation(resetPosition, resetRotation);
            }
        }


        // ==================================================
        // [ Move ]
        // ==================================================

        // 캐릭터 이동
        private void PlayerMove()
        {
            // 중력
            if (IsGrounded && IsVelocity)
            {
                playerVelocity.y = -0.5f;
            }

            // 캐릭터 이동
            playerController.Move(playerMoveVector * playerSpeed * Time.deltaTime);
            if (playerMoveVector != Vector3.zero)
            {
                // 캐릭터 회전
                Quaternion rotation = Quaternion.LookRotation(playerMoveVector);
                player.rotation = Quaternion.Slerp(player.rotation, rotation, Time.deltaTime * 30f);

                // 왼쪽키를 누르거나, 조이스틱에 최상단부분으로 작업을 할때 달리기를 실행한다.
                if (Input.GetKey(KeyCode.LeftShift) || Gamepad.current.leftStick.ReadValue().magnitude >= 0.98f)
                {
                    // 뛰기 (Run)
                    playerCalculateState = 2;
                }
                else
                {
                    // 걷기 (Walk)
                    playerCalculateState = 1;
                }
            }
            else
            {
                // 정지 ( Idle )
                playerCalculateState = 0;
            }

            // Gravity
            playerVelocity.y += gravityValue * Time.deltaTime;
            playerController.Move(playerVelocity * Time.deltaTime);
        }

        // 플레이어 상태 업데이트
        private void PlayerUpdate()
        {
            var script = playerAnimator.GetComponent<PlayerAvatar>();

            if (playerState != playerCalculateState)
            {
                playerState = playerCalculateState;
                if (playerState == 2)
                {
                    // 뛰기 상태
                    playerSpeed = runSpeed;
                    playerAnimator.Play(runHash);
                    script.AnimationSynchUpdate(runHash);

                    // 라이딩 뛰기 상태
                    if (IsRiding)
                    {
                        // 오디오 실행
                        playerAudioSource.clip = runClip;
                        playerAudioSource.Play();
                    }
                }
                else if (playerState == 1)
                {
                    // 걷기 상태
                    playerSpeed = walkSpeed;
                    playerAnimator.Play(walkHash);
                    script.AnimationSynchUpdate(walkHash);

                    // 라이딩 걷기 상태
                    if (IsRiding)
                    {
                        playerAudioSource.clip = walkClip;
                        playerAudioSource.Play();
                    }
                }
                else
                {
                    // 멈춤 상태
                    playerSpeed = 0;
                    playerAnimator.Play(idleHash);
                    script.AnimationSynchUpdate(idleHash);

                    if (IsRiding)
                    {
                        playerAudioSource.clip = idleClip;
                        playerAudioSource.Play();
                    }
                }
            }
        }

        // 플레이어 만보기 계산
        private void PlayerPedometer()
        {
            // 땅이고 라이딩상태가 아닐때 반영
            if (IsGrounded && !IsRiding)
            {
                // 애니메이션 값을 바탕으로 계산
                // 원래 계산은 거속시로 지정할려고했으나 걷기, 뛰기가 애니메이션 시간이 정해져있어 바로 지정
                pedometerTimer += Time.deltaTime;
                if (pedometerTimer > (pedometerTargetTimer / playerSpeed))
                {
                    pedometerTimer = 0.0f;
                    Pedometer.CountUpdate(Pedometer.currentCount + 1);
                }
            }
        }

        // 플레이어 네트워크에서 값을 받아 이동 변경
        private void PlayerNetwork()
        {
            player.position = Vector3.Lerp(player.position, playerPosition, Time.deltaTime * 30);
            player.rotation = Quaternion.Lerp(player.rotation, playerRotation, Time.deltaTime * 30);
        }

        // 플레이어가 네트워크에서 값을 받을때 상태 변경
        private void PlayerNetworkUpdate()
        {
            if (playerHash != playerTargetHash)
            {
                var script = playerAnimator.GetComponent<PlayerAvatar>();

                playerHash = playerTargetHash;
                playerAnimator.Play(playerHash);
                script.AnimationSynchUpdate(playerHash);

                if (IsRiding)
                {
                    if (playerState == 2)
                    {
                        playerAudioSource.clip = runClip;
                        playerAudioSource.Play();
                    }
                    else if (playerState == 1)
                    {
                        playerAudioSource.clip = walkClip;
                        playerAudioSource.Play();
                    }
                    else
                    {
                        playerAudioSource.clip = idleClip;
                        playerAudioSource.Play();
                    }
                }
            }
        }


        // ==================================================
        // [ Behavior ]
        // ==================================================

        // 플레이어 행동 처리
        private void PlayerBehavior(string animationCode, int animationHash)
        {
            // 이동중일 경우 플레이어 애니메이션 등을 풀기위해 사용
            if (!IsMove)
            {
                PlayerReset();
                IsMove = true;
            }

            // 점프, 라이딩, 기타 => 애니메이션 실행
            switch (animationCode)
            {
                case "Jump":
                    PlayerJump();
                    break;
                case "Riding":

                    break;
                default:
                    PlayerAnimation(animationHash);
                    break;
            }
        }

        // 플레이어 점프 처리
        private void PlayerJump()
        {
            var script = playerAnimator.GetComponent<PlayerAvatar>();

            // 플레이어가 땅에 있을경우 점프
            if (IsGrounded)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
                playerController.Move(playerVelocity * Time.deltaTime);

                // 플레이어가 정지상태일때는 애니메이션 실행
                if (IsPause)
                {
                    playerAnimator.Play(jumpHash);
                    script.AnimationSynchUpdate(jumpHash);
                }
            }
        }


        // ==================================================
        // [ Animation ] => Jesture, Motion
        // ==================================================

        // 플레이어 애니메이션 실행
        private void PlayerAnimation(string animation) => PlayerAnimation(Animator.StringToHash(animation));

        private void PlayerAnimation(int animationHash)
        {
            // 정지상태일때
            if (!IsMove)
            {
                // 애니메이션 초기화를 위해 사용
                PlayerReset();
                IsMove = true;
            }

            // 정지상태이고 땅에 있을때 애니메이션 실행
            if (IsPause && IsGrounded)
            {
                var script = playerAnimator.GetComponent<PlayerAvatar>();
                playerAnimator.Play(animationHash);
                script.AnimationSynchUpdate(animationHash);
            }
        }


        // ==================================================
        // [ Interactive ]
        // ==================================================

        // 플레이어 상호작용 
        private void PlayerInteractive()
        {
            // 정지상태이고 땅에 있을때만 사용
            if (IsPause && IsGrounded)
            {
                // 마우스 사용시 진행
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100, 1 << 10))
                    {
                        // Layer :: 10 => Point
                        var click = hit.collider.GetComponent<InteractiveClick>();
                        if (click != null)
                        {
                            // Setting에 backup
                            Setting.Interactive.category = click.clickCategory;
                            Setting.Interactive.cases = click.clickCase;
                            Setting.Interactive.hash = click.clickHash;
                            Setting.Interactive.key = click.clickKey;
                            Setting.Interactive.value = click.clickValue;
                            Setting.Interactive.code = click.clickCode;
                            if (click.isNpc)
                            {
                                Setting.Interactive.npcCode = click.npcCode;
                                NPCHistory(Setting.Interactive.npcCode).Forget();

                            }

                            // 카테고리에 맞게 인터렉티브 효과 실행
                            switch ((InteractiveType)click.clickCategory)
                            {
                                case InteractiveType.Event:
                                    InteractiveEvent().Forget();
                                    break;
                                case InteractiveType.Animation:
                                    InteractiveAnimation().Forget();
                                    break;
                                case InteractiveType.Bora:
                                    InteractiveBora().Forget();
                                    break;
                                case InteractiveType.Riding:
                                    InteractiveRiding().Forget();
                                    break;
                                case InteractiveType.Warp:
                                    InteractiveWarp().Forget();
                                    break;
                                case InteractiveType.SeasonEvent:
                                    InteractiveSeasonEventDragon().Forget();
                                    hit.transform.gameObject.SetActive(false);
                                    hit.transform.parent.gameObject.SetActive(false);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // 라이딩 진행
        [PunRPC]
        public void PlayerRiding(string userCode, string ridingCode)
        {
            if (playerNetwork.Owner.NickName.Equals(userCode))
            {
                riding = ridingCode;
                RidingAsync(userCode, ridingCode).Forget();
            }
        }

        // 라이딩 취소
        [PunRPC]
        private void PlayerRidingClear(string userCode, string ridingCode)
        {
            if (playerNetwork.Owner.NickName.Equals(userCode))
            {
                riding = ridingCode;
                if (IsRiding)
                {
                    IsRiding = false;
                }
            }
        }

        #endregion



        #region Interactive

        // 인터렉티브 정지를 위해 기존 행동 시작
        private void InteractivePlay()
        {
            playerKey = Setting.User.userCodeNo.ToString();
            playerValue = "";

            Interactive.EventUpdate(false);
        }

        // 인터렉티브 실행을 위해 행동 정지
        private void InteractivePause()
        {
            playerKey = Setting.User.userCodeNo.ToString();
            playerValue = Setting.Interactive.code;

            Interactive.EventUpdate(true);
        }


        // ==================================================
        // Interactive Function
        // ================================================== 

        // 인터렉티브 씬 활성화
        private async UniTaskVoid InteractiveScene(string sceneName)
        {
            // Clear
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }


        // ==================================================
        // Event
        // ================================================== 

        // 인터렉티브 이벤트 이벤트 진행
        private async UniTaskVoid InteractiveEvent()
        {
            // Clear
            switch ((InteractiveEventType)Setting.Interactive.cases)
            {
                case InteractiveEventType.Custom:
                    if (Setting.Interactive.hash)
                    {
                        if (Setting.Interactive.key.ToLower().Equals("scene"))
                        {
                            InteractiveScene(Setting.Interactive.value).Forget();
                        }
                    }
                    break;
                default:
                    await UniTask.Yield();
                    break;
            }
        }


        // ==================================================
        // Animation
        // ==================================================        

        // 인터렉티브 애니메이션 실행
        private async UniTaskVoid InteractiveAnimation()
        {
            // 이미 인터렉티브 진행중이라면 진행 x
            if (IsInteractive)
            {
                return;
            }

            // Clear
            var data = Addressables.LoadAssetAsync<InteractiveAnimationData>(Setting.Interactive.code);
            await data;

            if (data.Status == AsyncOperationStatus.Succeeded)
            {
                // 현재 위치 지정
                var resetPosition = player.position;
                var resetRotation = player.rotation;

                // 애니메이션 시작 위치로 이동
                if (data.Result.IsStart)
                {
                    PlayerReset(data.Result.startPosition, Quaternion.Euler(data.Result.startRotation));
                }

                // 애니메이션 실행
                PlayerAnimation(((InteractiveAnimationType)Setting.Interactive.cases).ToString());

                // 애니메이션 실행중
                InteractivePause();
                IsMove = false;
                while (!IsMove)
                {
                    await UniTask.Yield();
                }
                InteractivePlay();

                // 애니메이션 멈추기 위치로 이동
                if (data.Result.IsFinish)
                {
                    // 지정된 위치가 있을 경우 실행
                    PlayerReset(data.Result.finishPosition, Quaternion.Euler(data.Result.finishRotation));
                }
                else
                {
                    // 지정된 위치가 없을경우 처음 시작시 지정된 위치로 이동
                    PlayerReset(resetPosition, resetRotation);
                }

            }
            else
            {
                PlayerAnimation(Setting.Interactive.cases);
                return;
            }

            if (data.IsValid())
            {
                Addressables.Release(data);
            }
        }

        // ==================================================
        // Bora
        // ==================================================  

        // 보라 실행
        private async UniTaskVoid InteractiveBora()
        {
            // Clear
            switch ((InteractiveBoraType)Setting.Interactive.cases)
            {
                case InteractiveBoraType.Bora360:
                    var data = Addressables.LoadAssetAsync<InteractiveBoraData>(Setting.Interactive.code);
                    await data;

                    if (data.Status == AsyncOperationStatus.Succeeded)
                    {
                        // 영상 데이터 입력
                        Setting.Video.url = data.Result.boraUrl;
                        Setting.Video.option = data.Result.boraOption;

                        if (data.Result.boraReset)
                        {
                            // 360도 카메라 옵션 위치로 지정
                            Setting.Video.cameraPosition = data.Result.boraPosition;
                            Setting.Video.cameraRotation = Quaternion.Euler(data.Result.boraRotation);
                            Setting.Video.cameraScale = data.Result.boraScale;
                        }
                        else
                        {
                            // 360도 카메라 초기 위치 강제지정
                            Setting.Video.cameraPosition = Vector3.zero;
                            Setting.Video.cameraRotation = Quaternion.identity;
                            Setting.Video.cameraScale = Vector3.one;
                        }

                        if (data.IsValid())
                        {
                            Addressables.Release(data);
                        }

                        InteractiveScene("102_Video360").Forget();
                    }

                    break;
                case InteractiveBoraType.BoraXR:
                    InteractiveScene("302_BoraXR").Forget();
                    break;
            }
        }

        // ==================================================
        // Riding
        // ==================================================  

        // 라이딩 실행  
        private async UniTaskVoid InteractiveRiding()
        {
            // Clear
            switch ((InteractiveRidingType)Setting.Interactive.cases)
            {
                case InteractiveRidingType.Horse:
                    RidingEvnet("horse");
                    break;
                default:
                    await UniTask.Yield();
                    break;
            }

            // 라이딩 실행 위치로
            var data = Addressables.LoadAssetAsync<InteractiveRidingData>(Setting.Interactive.code);
            await data;

            if (data.Status == AsyncOperationStatus.Succeeded)
            {
                if (data.Result.IsRidingReset)
                {
                    playerController.enabled = false;
                    player.transform.position = data.Result.ridingPosition;
                    player.transform.rotation = Quaternion.Euler(data.Result.ridingRotation);
                    playerController.enabled = true;
                }
            }
        }

        // 라이딩 이벤트 실행
        private void RidingEvnet(string riding)
        {
            Player.Riding.IsUse = true;
            Player.Riding.address = riding;
            Player.RidingUpdate();

            Keyboard.RidingKeyboard();
        }


        // ==================================================
        // Warp
        // ==================================================    

        // 캐릭터 텔레포트 기능 실행
        private async UniTaskVoid InteractiveWarp()
        {
            // Clear
            switch ((InteractiveWarpType)Setting.Interactive.cases)
            {
                case InteractiveWarpType.Teleport:
                    InteractiveScene("501_WarpTeleport").Forget();
                    break;
                default:
                    await UniTask.Yield();
                    break;
            }
        }
        /// ===============================================
        /// NPC 상호작용 체크
        /// ===============================================
        private async UniTask NPCHistory(int objNo)
        {
            //Debug.Log("NPC History Start");
            WWWForm form = new WWWForm();
            form.AddField("channelNo", Setting.Network.channel);
            form.AddField("objectNo", objNo);
            form.AddField("worldNo", Setting.World.code);

            string url = $"https://{Setting.Server.api}/api/ingame/object/history";
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    Debug.Log("Error: " + e);
                }

                //Debug.Log("NPC History End: \n" + request.downloadHandler.text);
            }
        }

        #endregion



        #region Riding

        // 라이딩 진행
        private async UniTaskVoid RidingAsync(string userCode, string ridingCode)
        {
            if (IsRiding)
            {
                return;
            }
            var data = Addressables.LoadAssetAsync<RidingData>(ridingCode);
            await data;

            if (data.Status == AsyncOperationStatus.Succeeded)
            {
                // 라이딩 아바타 생성 및 위치 초기화
                riding = ridingCode;
                var ridingAvatar = Instantiate(data.Result.riding, player);
                ridingAvatar.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                // 라이딩 아바타 애니메이션 가져오기
                playerAnimator = ridingAvatar.GetComponent<Animator>();
                if (playerAnimator != null)
                {
                    playerAvatar.gameObject.SetActive(false);
                }

                // 라이딩 아바타 의상 변경
                var script = playerAnimator.GetComponent<PlayerAvatar>();
                if (script != null)
                {
                    script.PlayerMeti(userCode).Forget();
                }

                // 라이딩 닉네임 지정
                var nickname = player.GetComponent<PlayerNickname>();
                if (nickname != null)
                {
                    nickname.NicknameLocation(1.6f);
                }

                // 라이딩 스피드 재지정
                if (playerController != null)
                {
                    walkSpeed = data.Result.ridingWalkSpeed;
                    runSpeed = data.Result.ridingRunSpeed;
                }

                // 설정된 라이딩 오디오 소스가져오기
                if (playerAudioSource != null)
                {
                    idleClip = data.Result.ridingIdleClip;
                    walkClip = data.Result.ridingWalkClip;
                    runClip = data.Result.ridingRunClip;

                    playerAudioSource.clip = idleClip;
                    playerAudioSource.Play();
                }

                // 상태정보 따른 아바타 상태 갱신
                if (playerAnimator != null)
                {
                    if (playerState == 2)
                    {
                        playerSpeed = runSpeed;
                        playerAnimator.Play(runHash);
                        script.AnimationSynchUpdate(runHash);
                    }
                    else if (playerState == 1)
                    {
                        playerSpeed = walkSpeed;
                        playerAnimator.Play(walkHash);
                        script.AnimationSynchUpdate(walkHash);
                    }
                    else
                    {
                        playerSpeed = 0;
                        playerAnimator.Play(idleHash);
                        script.AnimationSynchUpdate(idleHash);
                    }
                }

                // 라이딩 기능 설정
                IsInteractive = true;
                IsRiding = true;
                while (IsRiding)
                {
                    await UniTask.Yield();
                }
                IsRiding = false;
                IsInteractive = false;

                // 라이딩 애니메이터 해제 기존 캐릭터 애니메이터 설정
                playerAnimator = playerAvatar.GetComponent<Animator>();
                if (playerAnimator != null)
                {
                    playerAvatar.gameObject.SetActive(true);
                }

                // 닉네임 위치 되돌리기 => 캐릭터 크기에 맞춰 재지정
                if (nickname != null)
                {
                    nickname.NicknameLocation(1.2f);
                }

                // 기존 플레이어 정보로 재지정
                if (playerController != null)
                {
                    OnPlayerPropertyUpdate();
                }

                // 오디오 소스 해제
                if (playerAudioSource != null)
                {
                    playerAudioSource.clip = null;
                    playerAudioSource.Pause();
                }

                DestroyImmediate(ridingAvatar);
            }

            if (data.IsValid())
            {
                Addressables.Release(data);
            }
        }

        #endregion

        private async UniTask InteractiveSeasonEventDragon()
        {
            //Debug.Log("NPC History Start");
            WWWForm form = new WWWForm();
            form.AddField("dragonNo", Setting.Interactive.value);
            form.AddField("worldNo", Setting.World.code);

            string url = $"https://{Setting.Server.api}/api/quest/3d-dragon";
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);

                try
                {
                    await request.SendWebRequest();
                }
                catch (UnityWebRequestException e)
                {
                    Debug.Log("Error: " + e);
                }
                var hash = new PopupHash()
                {
                    title = "용 한마리 잡기 성공!",
                    message = "[미션] 창에서 확인해주세요",
                    confirm = "확인"
                };
                Popup.Message(PopupType.General, hash);
                //Debug.Log("NPC History End: \n" + request.downloadHandler.text);
            }
        }


        #region Job

        // InputSystem으로 들어온 값을 통해 비동기 계산 진행
        private void PlayerCalculate()
        {
            InputCalculate job = new InputCalculate()
            {
                input = playerInputVector,
                rotation = playerCamera.rotation,
                direction = playerTarget.up,

                result = new NativeArray<Vector3>(1, Allocator.TempJob)
            };

            JobHandle handle = job.Schedule();
            handle.Complete();

            playerMoveVector = job.result[0];

            job.result.Dispose();
        }

        #endregion



        #region Network

        // 포톤으로 네트워크값 전송 및 받기
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(player.position);
                stream.SendNext(player.rotation);
                stream.SendNext(playerState);
                stream.SendNext(playerAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash);
                stream.SendNext(playerKey);
                stream.SendNext(playerValue);
            }
            else
            {
                playerPosition = (Vector3)stream.ReceiveNext();
                playerRotation = (Quaternion)stream.ReceiveNext();
                playerCalculateState = (int)stream.ReceiveNext();
                playerTargetHash = (int)stream.ReceiveNext();
                playerKey = (string)stream.ReceiveNext();
                playerValue = (string)stream.ReceiveNext();
                if (string.IsNullOrEmpty(playerValue))
                {
                    if (Interactive.containerUser.ContainsKey(playerKey))
                    {
                        Interactive.containerUser.Remove(playerKey);
                    }
                }
                else
                {
                    if (!Interactive.containerUser.ContainsKey(playerKey))
                    {
                        Interactive.containerUser.Add(playerKey, playerValue);
                    }
                }
            }
        }

        #endregion

    }

    // InputSystem 이동 값을 => 카메라가 바라보는 방향으로 계산
    [BurstCompile]
    public struct InputCalculate : IJob
    {
        // Value
        public Vector2 input;
        public Quaternion rotation;
        public Vector3 direction;

        // Result    
        public NativeArray<Vector3> result;

        public void Execute()
        {
            if (input != Vector2.zero)
            {
                Vector3 clampVector = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);
                Vector3 cameraDir = Vector3.ProjectOnPlane(rotation * Vector3.forward, direction).normalized;
                if (cameraDir.sqrMagnitude == 0f)
                {
                    cameraDir = Vector3.ProjectOnPlane(rotation * Vector3.up, direction).normalized;
                }
                Quaternion cameraRot = Quaternion.LookRotation(cameraDir, direction);

                Vector3 dirVector = cameraRot * clampVector;
                result[0] = dirVector.normalized;
            }
            else
            {
                result[0] = Vector3.zero;
            }
        }
    }

}

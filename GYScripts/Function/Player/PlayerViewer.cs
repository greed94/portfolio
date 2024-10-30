using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Metalive
{

    [RequireComponent(typeof(PhotonView))]
    public class PlayerViewer : MonoBehaviour
    {

        #region Variable

        // =============================================
        // [ Viewer ]
        // =============================================   
        [Header("[ Viewer ]")]
        [SerializeField]
        private Transform viewerTarget;
        private Transform viewer { get; set; }
        private ViewerController viewerController { get; set; }

        private Vector2 touchInput = Vector2.zero;
        private Vector2 touchRange = new Vector2(680f, 680f);

        private Vector3 mouseInput = Vector3.zero;
        private float mouseScrollInput = 0;
        private float scrollSpeed = 0.1f;

        private const string InputX = "Mouse X";
        private const string InputY = "Mouse Y";
        private const string InputScroll = "Mouse ScrollWheel";

        #endregion



        #region Option

        public bool IsUse { get; set; }

        #endregion



        #region Patch

        public void Patch()
        {
            var mine = gameObject.GetComponent<PhotonView>().IsMine;
            if (mine)
            {
                // 포톤을 통해 자신인지 상대인지 판별 => 나자신
                viewer = Camera.main.transform;
                if (viewerController == null)
                {
                    // 코드 변경필요 => 컨트롤러 합치기 => null체크 불필요
                    viewerController = viewer.GetComponent<ViewerController>();

                    // 셋팅이 월드, 갤러리 구분해서 보여지는 위치 변경 여부
                    bool enable = (SettingWorldType)Setting.World.type == SettingWorldType.World ? false : true;

                    // 뷰어의 위치를 재지정
                    viewerController.SetFollowTransform(viewerTarget, enable);
                }

                IsUse = viewer != null && viewerController != null ? true : false;
            }
            else
            {
                // 포톤을 통해 자신인지 상대인지 판별 => 상대
                // 스크립트 제거
                DestroyImmediate(this);
            }
        }

        #endregion



        #region Lifecycle

        private void OnEnable()
        {
            // InputSystem 이해 필요
            EnhancedTouchSupport.Enable();
        }

        private void Start()
        {
            Patch();
        }

        private void Update()
        {
            if (IsUse)
            {
#if UNITY_EDITOR || UNITY_EDITOR_OSX || UNITY_STANDALONE
                // PC, Editor에서 사용
                HandleCameraInputEditor();
#else
                // Mobile에서 사용
                HandleCameraInputMobile();
#endif
            }
        }

        private void LateUpdate()
        {
            if (IsUse)
            {
                HandleCameraUpdate();
            }
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        #endregion



        #region Method

        private void HandleCameraUpdate()
        {
            // 현재 카메라 위치를 재지정 => 미리지정된 캐릭터 위치를 따라가도록 설정되어 있기때문에 업데이트만 필요
            viewerController.UpdateWithInput(Time.deltaTime, 0, Vector3.zero);
        }

        // Editor에서 카메라 회전
        private void HandleCameraInputEditor()
        {
            // 마우스 버튼을 사용했을때 진행
            if (Input.GetMouseButton(0))
            {
                // 이동 조이스틱을 움직이지 않고 UI를 클릭하고 있지않을때 조건을 실행
                if (!Gamepad.current.leftStick.IsActuated() && !EventSystem.current.IsPointerOverGameObject())
                {
                    // 마우스 값 가져오기
                    mouseInput = new Vector3(Input.GetAxisRaw(InputX), Input.GetAxisRaw(InputY), 0f);

                    // 마우스 스크롤 휠 값 가져오기
                    mouseScrollInput = -Input.GetAxis(InputScroll);

                    // 카메라 적용
                    viewerController.UpdateWithInput(Time.deltaTime, mouseScrollInput, mouseInput);
                }

            }
        }

        private void HandleCameraInputMobile()
        {
            // 현재 터치 카운터를 가져온다
            var count = Touch.activeFingers.Count;
            if (count == 0)
                return;

            Touch touch = Touch.activeTouches[0];
            if (count == 1)
            {
                // 이동중일때는 하지않는다
                if (Gamepad.current.leftStick.IsActuated())
                    return;

                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // 손가락 하나일때 처리
                    touchInput = touch.delta * 0.1f;
                    mouseInput = new Vector3(touchInput.x, touchInput.y, 0f);
                    mouseScrollInput = -Input.GetAxis(InputScroll);
                    viewerController.UpdateWithInput(Time.deltaTime, mouseScrollInput, mouseInput);
                }
            }
            else
            {
                // 범위에 터치 손가락 터치 체크
                // 움직이는 영역을 제외하고는 전부 오른손가락이라고 하는게 좋지 않을까?
                if (touch.startScreenPosition.x <= touchRange.x && touch.startScreenPosition.y <= touchRange.y)
                {
                    touch = Touch.activeTouches[1];
                }
                else
                {
                    touch = Touch.activeTouches[0];
                }

                if (!EventSystem.current.IsPointerOverGameObject() && !Gamepad.current.leftStick.IsActuated())
                {
                    // 시작값, 마지막값 차이를 지정
                    var startDistance = Vector2.Distance(Touch.activeTouches[0].startScreenPosition, Touch.activeTouches[1].startScreenPosition);
                    var lastDistance = Vector2.Distance(Touch.activeTouches[0].screenPosition, Touch.activeTouches[1].screenPosition);

                    // 스크롤 값 체크
                    var distance = (lastDistance - startDistance);
                    if (distance < 0)
                    {
                        mouseScrollInput = scrollSpeed;
                    }
                    else
                    {
                        mouseScrollInput = -scrollSpeed;
                    }
                }

                if (mouseScrollInput == 0)
                {
                    // 카메라 업데이트
                    touchInput = touch.delta * 0.1f;
                    mouseInput = new Vector3(touchInput.x, touchInput.y, 0f);
                    viewerController.UpdateWithInput(Time.deltaTime, mouseScrollInput, mouseInput);
                }
                else
                {
                    // 카메라 일반 업데이트
                    viewerController.UpdateWithInput(Time.deltaTime, mouseScrollInput, Vector3.zero);
                }
            }
        }

        #endregion

    }
}


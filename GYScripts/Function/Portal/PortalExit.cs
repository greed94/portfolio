using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Metalive
{
    public class PortalExit : MonoBehaviour, IPopupCallback
    {

        #region Variable

        [SerializeField]
        private Button exitButton;

        #endregion



        #region Lifecycle

        private void OnEnable()
        {
            Popup.AddCallback(this);
        }

        private void Start()
        {
            if (exitButton)
            {
                exitButton.onClick.AddListener(() => Exit());
            }
            else
            {
                Metalive.Message("Portal", "Debug", "PortalExit component error");
            }
        }

        private void OnDisable()
        {
            Popup.RemoveCallback(this);
        }

        private void OnDestroy()
        {
            if(exitButton)
            {
                Destroy(exitButton);
            }
        }

        #endregion



        #region Callback

        public void OnPopupUpdate(PopupRedirect redirect)
        {
            if(redirect.code.Equals("portalexit-out"))
            {
                if(redirect.type == PopupRedirectType.Yes)
                {
                    Metalive.Message("Portal", "Exit", "");
                }
            }
        }

        #endregion



        #region Method

        // 월드 종료
        // 갤러리, 일반 가상월드 구분
        private void Exit()
        {
            if((SettingWorldType)Setting.World.type == SettingWorldType.Gallery)
            {
                var hash = new PopupHash
                {
                    title = "갤러리 나가기",
                    message = "갤러리 접속을 종료 하시겠습니까?",
                    confirm = "",
                    yes = "확인",
                    no = "취소",
                };

                Popup.Message(PopupType.Choice, PopupCallbackType.Callback, "portalexit-out", hash);
            }                       
            else
            {
                var hash = new PopupHash
                {
                    title = "가상월드 나가기",
                    message = "가상월드 접속을 종료 하시겠습니까?",
                    confirm = "",
                    yes = "확인",
                    no = "취소",
                };

                Popup.Message(PopupType.Choice, PopupCallbackType.Callback, "portalexit-out", hash);
            }
        }

        #endregion

    }
}
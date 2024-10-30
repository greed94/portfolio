// ==================================================
//
// 포탈 버전을 Dev, QA 표시하기 위해 만들어진 코드
// => 해당 스크립트 사용시 기본 1.0.0 표기
// => Unity Editor Inspector에서 버전 수정해서 바로 표기가능
//
// ==================================================

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Metalive
{
    public class PortalVersion : MonoBehaviour
    {

        #region Variable

        [SerializeField]
        private TMP_Text version;
        [SerializeField]
        private string versionValue = "1.0.0";

        #endregion



        #region Lifecycle

        private void Start()
        {
            Patch();
        }

        #endregion



        #region Patch

        private void Patch()
        {
            if(version)
            {
                if ((SettingServerType)Setting.Server.type == SettingServerType.DEV || (SettingServerType)Setting.Server.type == SettingServerType.QA)
                {
                    version.text = $"v{versionValue}";
                    version.gameObject.SetActive(true);
                }
                else
                {
                    version.gameObject.SetActive(false);
                }
            }
        }

        #endregion

    }

}
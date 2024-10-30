using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metalive
{
    public class HubManager : MonoBehaviour
    {

        #region Lifecycle

        private void Start()
        {
            HubEvent();
        }

        #endregion



        #region Callback

        // ==================================================
        // 
        // 네이티브에서 메타라이브 정보를 얻는다
        //
        // ==================================================

#if (UNITY_ANDROID || UNITY_IOS)

        // == [ Server ] =================================
        private void ServerCallback(string type)
        {            
            Setting.Server.type = int.Parse(type);
            Debug.Log("Server : " + type);
        }


        // == [ User ] ===================================
        private void UserAccessTokenCallback(string accessToken)
        {
            Setting.User.accessToken = accessToken;
            Debug.Log("UserAccountToken " + accessToken);
        }


        // [ choice ]
        private void UserRefreshTokenCallback(string refreshToken)
        {
            Setting.User.refreshToken = refreshToken;
            Debug.Log("UserRefreshToken " + refreshToken);
        }

        private void UserCodeCallback(string userCodeNo)
        {
            Setting.User.userCodeNo = int.Parse(userCodeNo);
            Debug.Log("UsercodeNo : " + userCodeNo);
        }

        private void UserNicknameCallback(string nickname)
        {
            Setting.User.nickname = nickname;
            Debug.Log("User Nickname : " + nickname);
        }


        // == [ World ] ==================================
        private void WorldCallback(string type)
        {
            Setting.World.type = int.Parse(type);
            Debug.Log("World type : " + type);
        }

        private void WorldCodeCallback(string code)
        {
            Setting.World.code = int.Parse(code);
            Debug.Log("World code : " + code);
        }

        private void WorldIdentificationCallback(string identification)
        {
            Setting.World.identification = identification;
            Debug.Log("World identification : " + identification);
        }

        private void WorldVersionCallback(string version)
        {
            Setting.World.version = version;
            Debug.Log("World version : " + version);
        }


        // == [ Network ] ==================================
        private void NetworkCallback(string type)
        {
            Setting.Network.type = int.Parse(type);
            Debug.Log("World network : " + type);
        }

        private void NetworkInviteCallback(string invite)
        {
            Setting.Network.invite = int.Parse(invite);
            Debug.Log("World invite : " + invite);
        }

        private void NetworkChannelCallback(string channel)
        {
            Setting.Network.channel = int.Parse(channel);
            Debug.Log("World Channel : " + channel);
        }

        private void NetworkCountCallback(string count)
        {
            Setting.Network.count = int.Parse(count);
            Debug.Log("World Count : " + count);
        }


        // == [ Option ] ==================================
        private void OptionLocalizationCallback(string count)
        {
            Setting.Option.localization = int.Parse(count);
            Debug.Log("World Count : " + count);
        }


        // [ Progress ] ===================================
        private void AndroidProgressCallback(string code)
        {
            if (code.Equals("0"))
            {
                // 실패
                return;
            }
            else
            {
                // 성공
                HubScene();
            }
        }

        private void iOSCallbackProgress(string code)
        {
            if (code.Equals("0"))
            {
                // 실패
                return;
            }
            else
            {
                // 성공
                HubScene();
            }
        }

#endif

        #endregion



        #region Method

        // ==================================================
        // 
        // 월드 정보를 받고 포탈로 넘어간다
        //
        // ==================================================
        private void HubScene()
        {
            HubSceneAsync().Forget();
        }

        private async UniTask HubSceneAsync()
        {
            var portal = "01_Portal";
            var portalScene = SceneManager.LoadSceneAsync(portal);
            await portalScene;

            if (portalScene.isDone)
            {
                while (!Portal.IsUse)
                {
                    await UniTask.Yield();
                }
                Metalive.Message("Portal", "Enter", "");
            }
            else
            {
                HubError("0000");
                return;
            }
        }

        // ==================================================
        // Error
        // ==================================================
        private void HubError(string code)
        {

#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Error", $"Code : {code}", "CONFIRM", "CANCEL");
#else
            Debug.LogError("Code : " + code);
#endif

        }


        // ==================================================
        // Event
        // ==================================================
        private void HubEvent()
        {
            if (Portal.status == PortalStatusType.None)
            {
                var data = Resources.Load<SettingData>("Metalive/MetaliveAssetSettings");
                if (data.portalExport)
                {
                    // == [ Server ] ==================================================
                    if ((SettingServerType)data.portalServer == SettingServerType.None)
                    {
                        HubError("1001");
                        return;
                    }
                    else
                    {
                        Setting.Server.type = data.portalServer;
                    }


                    // == [ User ] ==================================================
                    if (string.IsNullOrEmpty(data.portalAccessToken))
                    {
                        HubError("1011");
                        return;
                    }
                    else
                    {
                        Setting.User.accessToken = data.portalAccessToken;
                    }

                    if (string.IsNullOrEmpty(data.portalRefreshToken))
                    {
                        HubError("1012");
                        return;
                    }
                    else
                    {
                        Setting.User.refreshToken = data.portalRefreshToken;
                    }

                    if (data.portalUserCodeNumber == 0)
                    {
                        HubError("1013");
                        return;
                    }
                    else
                    {
                        Setting.User.userCodeNo = data.portalUserCodeNumber;
                    }

                    if (string.IsNullOrEmpty(data.portalNickname))
                    {
                        HubError("1014");
                        return;
                    }
                    else
                    {
                        Setting.User.nickname = data.portalNickname;
                    }


                    // == [ World ] ==================================================
                    if ((SettingWorldType)data.portalWorld == SettingWorldType.None)
                    {
                        HubError("1021");
                        return;
                    }
                    else
                    {
                        Setting.World.type = data.portalWorld;
                    }

                    if (data.portalCode == 0)
                    {
                        HubError("1022");
                        return;
                    }
                    else
                    {
                        Setting.World.code = data.portalCode;
                    }

                    if (string.IsNullOrEmpty(data.portalIdentification))
                    {
                        HubError("1023");
                        return;
                    }
                    else
                    {
                        Setting.World.identification = data.portalIdentification;
                    }

                    if (string.IsNullOrEmpty(data.portalVersion))
                    {
                        HubError("1024");
                        return;
                    }
                    else
                    {
                        Setting.World.version = data.portalVersion;
                    }


                    // == [ Network ] ==================================================
                    if ((SettingNetworkType)data.portalNetwork == SettingNetworkType.None)
                    {
                        HubError("1031");
                        return;
                    }
                    else
                    {
                        Setting.Network.type = data.portalNetwork;
                    }

                    if ((SettingInviteType)data.portalInvite == SettingInviteType.None)
                    {
                        HubError("1032");
                        return;
                    }
                    else
                    {
                        Setting.Network.invite = data.portalInvite;
                    }

                    if (data.portalChannel == 0)
                    {
                        HubError("1033");
                        return;
                    }
                    else
                    {
                        Setting.Network.channel = data.portalChannel;
                    }

                    if (data.portalCount == 0)
                    {
                        HubError("1034");
                        return;
                    }
                    else
                    {
                        Setting.Network.count = data.portalCount;
                    }


                    // == [ Option ] ==================================================
                    if ((SettingLocalizationType)data.portalLocalization == SettingLocalizationType.None)
                    {
                        HubError("1041");
                        return;
                    }
                    else
                    {
                        Setting.Option.localization = data.portalLocalization;
                    }

                    HubScene();
                }
                else
                {

#if UNITY_ANDROID
                    HubAndroidConnect();
#elif UNITY_IOS
                    HubiOSConnect();
#elif UNITY_STANDALONE_WIN
                    HubScene();
#endif

                }

                Resources.UnloadAsset(data);
            }
            else
            {

#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
#elif UNITY_ANDROID
                HubAndroidDisConnect();
#elif UNITY_IOS
                HubiOSDisConnect();
#else
                Application.Quit();
#endif

            }
        }


        // ==================================================
        // Portal Connect(Mobile)
        // ==================================================

        // == [ Android ] ====================
        private void HubAndroidConnect()
        {

#if UNITY_ANDROID
            AndroidJavaObject plugin = new AndroidJavaObject("com.awesomepia.metalive.unity.Portal");
            plugin.Call("worldStart");
#endif

        }

        private void HubAndroidDisConnect()
        {

            Portal.status = PortalStatusType.None;

#if UNITY_ANDROID
            AndroidJavaObject plugin = new AndroidJavaObject("com.awesomepia.metalive.unity.Portal");
            plugin.Call("worldEnd");
#endif            

        }

#if UNITY_IOS
        [DllImport("__Internal")]
        public static extern void sendMessageToMobileApp(string message);
#endif

        // == [ iOS ] ====================
        private void HubiOSConnect()
        {

#if UNITY_IOS
            sendMessageToMobileApp("worldStart");   
#endif

        }

        private void HubiOSDisConnect()
        {

            Portal.status = PortalStatusType.None;

#if UNITY_IOS

            sendMessageToMobileApp("worldEnd");   

#endif
        }

        #endregion

    }

}
// ==================================================
// 
// Init = 연결
// Fina = 해제
//
// ==================================================

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine.Networking;

namespace Metalive
{
    /*
     * Addressable add download
     */

    [RequireComponent(typeof(PortalController))]
    public class PortalManager : MonoBehaviour, IPopupCallback
    {

        #region Variable

        // Controller
        // => 삭제가능
        // => 일정에 맞지 않는 하드코딩 개발로 인한 controller 시스템 도입
        private PortalController controller;

        // Addressable
        private List<AsyncOperationHandle<SceneInstance>> scenes;
        private AsyncOperationHandle<MetaliveLightingData> lighting;


        // ↓ 수정 필요
        // Player script => 전역으로 처리해 정보를 가지고있는게 더 편한가능성이 높다!
        public static GameObject player;
        public static GameObject npcPlayer;

        //PDK TRTC 주석
        //[HideInInspector]
        //public RTCManager rtcManager;
        //[HideInInspector]
        //public TRTCVideoRender videoRender;

        //PDK 더미 Bot 생성 코드(테스트중)
        //private AsyncOperationHandle<WanderingNPCData> npcPatrolData;
        //private List<WanderingNPCTable> npcPatrolList;

        #endregion



        #region Lifecycle

        private void OnEnable()
        {
            Metalive.Hub += OnMessage;
            Popup.AddCallback(this);
        }

        private void Start()
        {
            controller = GetComponent<PortalController>();
            if (controller)
            {
                Portal.IsUse = true;
            }
        }

        private void OnDisable()
        {
            Metalive.Hub -= OnMessage;
            Popup.RemoveCallback(this);
        }

        // ==================================================
        //
        // * 매개 변수
        // [label] = Portal
        // [key] = Enter, Exit, Error, Debug
        // [value] = None
        // 
        // * 추가 개발 필요
        // - 월드에서 월드로 이동 기능필요
        //
        // ==================================================

        private void OnMessage(string label, string key, string value)
        {
            if (label.Equals("Portal"))
            {
                switch (key)
                {
                    case "Enter":
                        PortalEnter(value);
                        break;
                    case "Exit":
                        PortalExit(value);
                        break;
                    case "Error":
                        PortalError(value);
                        break;
                    case "Debug":
                        PortalDebug(value);
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            Portal.IsUse = false;

            if (scenes.Count > 0)
            {
                for (int i = 0; i < scenes.Count; i++)
                {
                    if (scenes[i].IsValid())
                    {
                        Addressables.Release(scenes[i]);
                    }
                }
            }

            if (lighting.IsValid())
            {
                Addressables.Release(lighting);
            }

            if (player)
            {
                DestroyImmediate(player);
            }

            //Addressables.Release(npcPatrolData);
        }

        #endregion



        #region Method

        // ==================================================
        // Enter or Exit
        // ==================================================        
        private void PortalEnter(string code)
        {
            Portal.status = PortalStatusType.Enter;
            Enter();
        }

        private void PortalExit(string code)
        {
            Portal.status = PortalStatusType.Exit;
            Exit();
        }

        // ==================================================
        // Progress
        // ==================================================
        private void Enter()
        {
            EnterAsync().Forget();
        }

        private async UniTaskVoid EnterAsync()
        {
            controller.PortalOpen();
            controller.ProgressOpen();
            controller.ProgressInit();

            // 카탈로그 불러오기
            var catalog = await CatalogInit();
            if (!catalog)
            {
                return;
            }

            controller.ProgressBreak();
            controller.ProgressInit();
            controller.ProgressEnviorment();

            // 월드 씬 가져오기
            var IsWorld = await WorldSceneInit();
            if (!IsWorld)
            {
                return;
            }

            // 플레이어 씬 가져오기
            var IsPlayer = await PlayerSceneInit();
            if (IsPlayer)
            {
                PlayerInit().Forget();
            }
            else
            {
                return;
            }

            //PDK TRTC 추가
            //var IsRTC = await RTCInit();
            //if (!IsRTC)
            //{
            //    return;
            //}

            // 네트워크 연결
            var IsNetwork = await NetworkInit();
            if (!IsNetwork)
            {
                return;
            }

            // UI씬 가져오기
            var IsUI = await UISceneInit();
            if (!IsUI)
            {
                return;
            }

            //복많이 받아용 이벤트용 코드
            if (Setting.World.code == 4055 || Setting.World.code == 4060 || Setting.World.code == 199)
            {
                var IsEvent = await SeasonEventSceneInit();
                if (!IsEvent)
                {
                    return;
                }
            }

            controller.ProgressBreak();
            controller.ProgressInit();

            // 아바타 대머리 안보이기 위한 2초 => 늘려도 됩니다.
            await UniTask.Delay(2000);

            Portal.status = PortalStatusType.Enter;
            controller.ProgressClose();
            controller.PortalClose();
        }

        public void SystemInit()
        {
            DefaultPool defaultPool = PhotonNetwork.PrefabPool as DefaultPool;
            defaultPool.ResourceCache.Clear();
            Destroy(NetworkManager.Instance.gameObject);
            //PDK Photon Chat -> TCP Chat Test
            //Destroy(NetworkChat.Instance.gameObject);

            Chat.SystemInit();
            Keyboard.SystemInit();
            Pedometer.SystemInit();
            CreatePlayerManager.SystemInit();
            Player.SystemInit();
            Portal.SystemInit();
            Setting.SystemInit();
            GC.Collect();
        }

        private async void Exit()
        {
            try
            {
                controller.PortalOpen();
                NetworkFina();
                WorldSceneFina();
                UISceneFina();
                PlayerSceneFina();
                PlayerFina();
                SystemInit();
                SeasonEventFina();

                controller.PortalClose();
            }
            catch
            {
                Metalive.Message("Portal", "Error", "9999");
            }
            finally
            {
                var location = "00_Hub";
#if UNITY_STANDALONE_WIN
                location = "000_PCScene";
#endif
                var scene = SceneManager.LoadSceneAsync(location);
                await scene;

                if (scene.isDone)
                {
                    Portal.status = PortalStatusType.Exit;
                    await Resources.UnloadUnusedAssets();
                }
            }
        }


        // ==================================================
        //
        // 카탈로그 로드 및 다운로드
        // => 개별 정보를 통해 가상월드를 다운로드한다.
        // 
        // 주소 정보는 5가지로 구성
        // [1] 기본 주소
        // [2] 파일 주소 => ex) com.awesomepia.metalive
        // [3] 월드 코드 => ex) 185
        // [4] 월드 버전 => ex) v1.0.0
        // [5] 플랫폼 => ex) Android, iOS
        //
        // 카탈로그는 중복으로 가져서는 안된다.
        // 카탈로그를 사용해서 사용자가 각 월드에 몇번씩 입장했는지도 알수있다.
        //
        // ==================================================
        private async UniTask<bool> CatalogInit()
        {
            try
            {

                // 카탈로그 가져오기
                if (Portal.catalogs == null)
                {
                    Portal.catalogs = new Dictionary<string, string>();
                }

                var identification = $"{Setting.World.identification}";
                var code = $"{Setting.World.code}";
                if (!Portal.catalogs.ContainsKey(code))
                {
                    // 해당 월드에 처음 접속한다면

                    var url = $"https://metalive-asset-resouse.s3.ap-northeast-2.amazonaws.com/admin/asset-resouse/WORLD/{identification}/{code}/v{Setting.World.version}/{Setting.Server.platform}/catalog_metalive.json";
                    var catalogHandle = Addressables.LoadContentCatalogAsync(url.Trim(), true);
                    await catalogHandle;

                    if (catalogHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Addressables.AddResourceLocator(catalogHandle.Result);
                        Portal.catalogs.Add(code, "1");
                    }
                    else
                    {
                        string timeStamp = TimeStamp().ToString();
                        CatalogError($"ci1, {timeStamp}");
                        ErrorDataSent($"{timeStamp}_{catalogHandle.OperationException.Message}");
                        return false;
                    }
                }
                else
                {
                    // 해당 월드에 첫번째 접속이 아니라면

                    Portal.catalogs[code] = (int.Parse(Portal.catalogs[code]) + 1).ToString();
                }

                // 카탈로그속 월드 정보를 통해 씬 다운로드
                var address = $"{Setting.World.code}_0_0";
                var data = Addressables.LoadAssetAsync<MetaliveExportData>(address);
                await data;

                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    // 사이즈 체크
                    var totalSize = (long)0;
                    var count = data.Result.scenes.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var location = data.Result.scenes[i];
                        var size = Addressables.GetDownloadSizeAsync(location);
                        await size;
                        totalSize += size.Result;

                        if (size.IsValid())
                        {
                            Addressables.Release(size);
                        }
                    }

                    // 전체 사이즈를 체크해서 다운로드 진행
                    if (totalSize > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var location = data.Result.scenes[i];
                            var download = Addressables.DownloadDependenciesAsync(location);
                            while (!download.IsDone)
                            {
                                float percentage = download.GetDownloadStatus().Percent;
                                controller.ProgressDownload(percentage);

                                if (Portal.status == PortalStatusType.Cancel)
                                {
                                    CatalogCancel();
                                    return false;
                                }

                                await UniTask.Yield();
                            }

                            if (download.Status == AsyncOperationStatus.Succeeded)
                            {
                                if (download.IsValid())
                                {
                                    Addressables.Release(download);
                                }
                            }
                            else
                            {
                                // Download fail → check download system
                                string timeStamp = TimeStamp().ToString();
                                CatalogError($"ci2, {timeStamp}");
                                ErrorDataSent($"{timeStamp}_{download.OperationException.Message}");
                                return false;
                            }
                        }
                    }

                    Addressables.Release(data);
                }
                else
                {
                    string timeStamp = TimeStamp().ToString();
                    CatalogException($"ce1, {timeStamp}");
                    ErrorDataSent($"{timeStamp}_{data.OperationException.Message}");

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {

                string timeStamp = TimeStamp().ToString();
                CatalogException($"ce2, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }

            // 필수적으로 마지막에 로드한 Addressable은 릴리즈한다
        }

        // 다운로드 오류
        private void CatalogError(string errorCode)
        {
            Portal.status = PortalStatusType.Error;

            var hash = new PopupHash
            {
                title = "다운로드 오류",
                message = $"서비스 접속이 원활하지 않습니다.\n잠시 후, 다시 시도해 주세요. \n\n<size=35>Code: {errorCode}</size>",
                confirm = "확인",
                yes = "",
                no = "",
            };

            Popup.Message(PopupType.General, PopupCallbackType.Callback, "Error-out", hash);
            return;
        }

        // 다운로드 기타오류
        private void CatalogCatch(string errorCode)
        {
            var hash = new PopupHash
            {
                title = "",
                message = $"일시적인 오류가 발생하였습니다. \n\n<size=35>Code: {errorCode}</size>",
                confirm = "",
                yes = "",
                no = "",
            };

            Popup.Message(PopupType.Floating, PopupStatusType.Caution, hash);
            return;
        }

        // 다운로드 예외 오류
        private void CatalogException(string errorCode)
        {
            Portal.status = PortalStatusType.Error;

            var hash = new PopupHash
            {
                title = "다운로드 오류",
                message = $"네트워크 연결 상태를 확인하신 후,\n다시 시도해 주세요. \n\n<size=35>Code: {errorCode}</size>",
                confirm = "확인",
                yes = "",
                no = "",
            };

            Popup.Message(PopupType.General, PopupCallbackType.Callback, "Error-out", hash);
            return;
        }

        // 다운로드 취소
        private void CatalogCancel()
        {
            Portal.status = PortalStatusType.Cancel;

            var hash = new PopupHash
            {
                title = "다운로드 취소",
                message = "다운로드를 취소하였습니다.",
                confirm = "확인",
                yes = "",
                no = "",
            };

            Popup.Message(PopupType.General, PopupCallbackType.Callback, "Error-out", hash);
            return;
        }

        private int TimeStamp()
        {
            var now = DateTime.Now.ToLocalTime();
            var span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            return (int)span.TotalSeconds;
        }

        private async void ErrorDataSent(string errorData)
        {
            await ErrorDataSentAsync(errorData);
        }

        private async UniTask ErrorDataSentAsync(string errorData)
        {
            string url;
            WWWForm form = new WWWForm();
            form.AddField("errorData", errorData);
            form.AddField("errorMsg", "error_Unity");
#if UNITY_ANDROID
            form.AddField("os", 1);
#elif UNITY_IOS
            form.AddField("os", 2);
#else
            form.AddField("os", 3);
#endif
            form.AddField("userCodeNo", Setting.User.userCodeNo);

            switch (Setting.Server.type)
            {
                case 1: //Release
                    url = "https://admin.meti.world/tracker";
                    break;
                case 3: //QA
                    url = "https://qaadmin.meti.world/tracker";
                    break;
                default: // case 2: Dev
                    url = "https://devadmin.meti.world/tracker";
                    break;
            }
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                await request.SendWebRequest();
            }
        }

        // ==================================================
        // 
        // 네트워크 연결진행
        // => 멀티플레이를 선택해야한 네트워크 연결진행
        // => 채팅은 멀티플레이가 연결 후에 진행
        //
        // ==================================================
        private async UniTask<bool> NetworkInit()
        {
            try
            {
                if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                {
                    NetworkManager.Instance.ConnectToServer();
                    var cts = new CancellationTokenSource();
                    cts.CancelAfterSlim(TimeSpan.FromSeconds(30));
                    await UniTask.WaitUntil(() => NetworkManager.Instance.IsNetworkConnect, PlayerLoopTiming.Update, cts.Token);

                    //PDK Photon Chat -> TCP Chat Test
                    //if (NetworkManager.Instance.IsNetworkConnect)
                    //{
                    //    NetworkChat.Instance.ConnectToServer();
                    //}
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"ni1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        private bool NetworkFina()
        {
            try
            {
                if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                {
                    NetworkManager.Instance.DisconnectToServer();
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogCatch($"ne1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        // ==================================================
        //
        // 월드 연결 진행
        // => MetaliveExportData에 있는 모든 월드를 연결
        // => 월드 연결 완료시 WorldSetting 진행
        // 
        // 수정필요
        // => Quest부분을 월드 입장시 전 월드에 대해 퀘스트진행으로 변경하여야 함
        //
        // ==================================================
        private async UniTask<bool> WorldSceneInit()
        {
            try
            {
                var address = $"{Setting.World.code}_0_0";
                var data = Addressables.LoadAssetAsync<MetaliveExportData>(address);
                await data;


                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    if (scenes == null)
                    {
                        scenes = new List<AsyncOperationHandle<SceneInstance>>();
                    }

                    var count = data.Result.scenes.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var scene = Addressables.LoadSceneAsync(data.Result.scenes[i], LoadSceneMode.Additive);
                        await scene;
                        scenes.Add(scene);
                        //scene.Completed += SceneLoadedForRTC; //PDK RTC 주석
                    }

                    if (data.IsValid())
                    {
                        Addressables.Release(data);
                    }

                    // 필수 수정 부분
                    if (Setting.World.code == 4055)
                    {
                        Quest.Send("32", Setting.World.code.ToString());
                    }

                    //PDK TRTC 주석
                    //if (Setting.World.code == 4053)
                    //{
                    //    Setting.TRTC.mode = 2;
                    //}
                    //else
                    //{
                    //    Setting.TRTC.mode = 0;
                    //}

                    return true;
                }
                else
                {
                    string timeStamp = TimeStamp().ToString();
                    CatalogError($"wi1, {timeStamp}");
                    ErrorDataSent($"{timeStamp}_{data.OperationException.Message}");
                    return false;
                }
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"wi2, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");

                return false;
            }
            finally
            {
                WorldSetting().Forget();
            }
        }

        // ==================================================
        //
        // 월드 셋팅 진행
        // => 현재 Lighting에 관한 데이터만 적용
        // => 원래 계획은 모든 월드설정에 대해 적용할 예정        
        //
        // ==================================================
        private async UniTaskVoid WorldSetting()
        {
            try
            {
                var address = $"{Setting.World.code}_0_1";
                var data = Addressables.LoadAssetAsync<MetaliveLightingData>(address);
                await data;

                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    if (data.Result.IsSkybox)
                    {
                        RenderSettings.skybox = data.Result.skybox;
                    }
                    else
                    {
                        RenderSettings.skybox = null;
                    }

                    if (data.Result.IsFog)
                    {
                        RenderSettings.fog = true;
                        RenderSettings.fogColor = data.Result.fogColor;
                        RenderSettings.fogMode = data.Result.fogMode;
                        RenderSettings.fogDensity = data.Result.fogDensity;
                    }
                    else
                    {
                        RenderSettings.fog = false;
                    }
                }
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"ws1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return;
            }
        }

        private bool WorldSceneFina()
        {
            try
            {
                if (scenes.Count > 0)
                {
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        if (scenes[i].IsValid())
                        {
                            Addressables.Release(scenes[i]);
                        }
                    }
                }

                if (lighting.IsValid())
                {
                    Addressables.Release(lighting);
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"we1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");

                return true;
            }
        }

        // [ RTC Init ] RTC주석
        //private async UniTask<bool> RTCInit()
        //{
        //    //if (MetaliveManager.Instance.game.mode == PortalMode.VirtualLand)
        //    try
        //    {
        //        AsyncOperation async = SceneManager.LoadSceneAsync("30_RTC", LoadSceneMode.Additive);

        //        if (Setting.TRTC.mode == 2)
        //        {
        //            async.completed += RTCSceneLoaded;
        //        }

        //        await async;

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        //if (progressCancel)
        //        //{
        //        //    var async = SceneManager.UnloadSceneAsync("30_RTC");
        //        //    await async;
        //        //}
        //    }
        //}
        ////동건 RTC 코드 시작
        //private void SceneLoadedForRTC(AsyncOperationHandle<SceneInstance> handle)
        //{
        //    if (handle.Status == AsyncOperationStatus.Succeeded)
        //    {
        //        Scene targetScene = handle.Result.Scene;
        //        GameObject targetObject = targetScene.GetRootGameObjects().FirstOrDefault(go => go.name == "World");

        //        if (targetObject != null)
        //        {
        //            Debug.Log("Find RTC Object");
        //            GameObject renderObj = targetObject.transform.Find("Object/LiveStreaming").gameObject;
        //            renderObj.AddComponent<TRTCVideoRender>();
        //            videoRender = renderObj.GetComponent<TRTCVideoRender>();

        //        }
        //        else
        //        {
        //            Debug.Log("No RTC Object");
        //        }
        //    }
        //}

        //private void RTCSceneLoaded(AsyncOperation handle)
        //{
        //    Scene targetScene = SceneManager.GetSceneByName("30_RTC");
        //    GameObject targetObj = targetScene.GetRootGameObjects().FirstOrDefault(go => go.name == "RTCManager");
        //    if (targetObj != null)
        //    {
        //        Debug.Log("Find RTC Manager");
        //        rtcManager = targetObj.GetComponent<RTCManager>();
        //        rtcManager.videoRender = videoRender;
        //    }
        //    else
        //    {
        //        Debug.Log("No RTC Manager");
        //    }
        //}
        //동건 RTC 코드 끝

        // ==================================================
        // 
        // UI씬 전체로드
        //
        // ==================================================
        private async UniTask<bool> UISceneInit()
        {
            try
            {
                var location = "02_UI";
                var scene = SceneManager.LoadSceneAsync(location, LoadSceneMode.Additive);
                await scene;

                if (scene.isDone)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"ui1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");

                return false;
            }
        }

        private bool UISceneFina()
        {
            try
            {
                var location = "02_UI";
                if (SceneManager.GetSceneByName(location) != null)
                {
                    SceneManager.UnloadSceneAsync(location);
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"uie1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");

                return false;
            }
        }


        // ==================================================
        // 
        // Player 씬 전체로드
        // => 해당 씬 삭제 가능
        // => 해당 씬 안에 들어있는 플레이어 카메라를 캐릭터 생성시 함께 생성하도록 조치하는게 가장좋음
        // => 해당 방식을 적용안한 이유는 메타라이브 캐릭터에 대한 정의를 하지를 않음...
        //
        // ==================================================
        private async UniTask<bool> PlayerSceneInit()
        {
            try
            {
                var location = "03_Player";
                var scene = SceneManager.LoadSceneAsync(location, LoadSceneMode.Additive);
                await scene;

                if (scene.isDone)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"pi2, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        private bool PlayerSceneFina()
        {
            try
            {
                var location = "03_Player";
                if (SceneManager.GetSceneByName(location) != null)
                {
                    SceneManager.UnloadSceneAsync(location);
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogCatch($"pe1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        // ==================================================
        // 
        // Player 캐릭터 로드
        // => MetalivePlayerData에 기록되어있는 플레이어 위치를 적용
        // => 멀티플레이어 캐릭터 생성시 포톤 이용
        // => 싱글플레이어 캐릭터 생성시 리소스 폴더 직접 접근
        //
        // ==================================================
        private async UniTaskVoid PlayerInit()
        {
            try
            {
                var address = $"{Setting.World.code}_0_3";
                var data = Addressables.LoadAssetAsync<MetalivePlayerData>(address);
                await data;

                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    CreatePlayer(data.Result.playerPosition, Quaternion.Euler(data.Result.playerRotation)).Forget();
                }
                else
                {
                    CreatePlayer(Vector3.zero, Quaternion.identity).Forget();
                }

                if (data.IsValid())
                {
                    Addressables.Release(data);
                }
            }
            catch
            {
                CreatePlayer(Vector3.zero, Quaternion.identity).Forget();
            }

        }

        private async UniTaskVoid CreatePlayer(Vector3 position, Quaternion rotation)
        {
            //Player가 없는경우
            if (!player)
            {
                //멀티인경우 Photon으로 생성
                if ((SettingNetworkType)Setting.Network.type == SettingNetworkType.MultiPlay)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfterSlim(TimeSpan.FromSeconds(60));
                    try
                    {
                        await UniTask.WaitUntil(() => NetworkManager.Instance.IsNetworkConnect == true, PlayerLoopTiming.Update, cts.Token);

                        //Debug.Log("My Player 생성");
                        player = NetworkManager.Instance.PlayerInstantiate(position, rotation);
                        if (player)
                        {
                            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName("03_Player"));
                        }
                        //PDK 더미 Bot 생성 코드(테스트중)
                        //if (NetworkManager.Instance.createdRoomCheck)
                        //{
                        //Debug.Log("Created Room, NPC Instantiate");
                        //CreateWanderingNPC(position, rotation);
                        //}
                    }
                    catch (Exception e)
                    {
                        string timeStamp = TimeStamp().ToString();
                        CatalogError($"cpi1, {timeStamp}");
                        ErrorDataSent($"{timeStamp}_{e}");
                    }
                }
                //싱글인경우 Resources로 생성
                else
                {
                    var obj = Resources.Load<GameObject>("Player");
                    player = Instantiate(obj, position, rotation);
                    if (player)
                    {
                        SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName("03_Player"));
                    }
                }
            }
        }

        //PDK 더미 Bot 생성 코드(테스트중)
        //private async void CreateWanderingNPC(Vector3 position, Quaternion rotation)
        //{
        //    Debug.Log("Child Count: " + GameObject.Find("PatrolCollection").transform.GetChild(0));
        //    npcPatrolData = Addressables.LoadAssetAsync<WanderingNPCData>("4056-101-0");
        //    await npcPatrolData;

        //    if (npcPatrolData.Status == AsyncOperationStatus.Succeeded)
        //    {
        //        Debug.Log("NPC Patrol Data Load Success");
        //        WanderingNPCData result = npcPatrolData.Result;
        //        Debug.Log(result.npcTable[0].waitTime);
        //        npcPatrolList = result.npcTable;

        //        Debug.Log("ListCount: " + npcPatrolList.Count);
        //        Debug.Log("Position: " + npcPatrolList[0].position);
        //        Debug.Log("Rotation: " + npcPatrolList[0].rotation);
        //        Debug.Log("Scale: " + npcPatrolList[0].scale);
        //        Debug.Log("WaitTime: " + npcPatrolList[0].waitTime);
        //    }

        //    //npcPlayer = NetworkManager.Instance.NPCInstantiate(position, rotation);
        //}

        private bool PlayerFina()
        {
            try
            {
                if (player)
                {
                    DestroyImmediate(player);
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"pfe1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        /// <summary>
        /// Season Event Scene Init
        /// </summary>
        /// <returns></returns>
        private async UniTask<bool> SeasonEventSceneInit()
        {
            try
            {
                var location = "15_SeasonEvent";
                var scene = SceneManager.LoadSceneAsync(location, LoadSceneMode.Additive);
                await scene;

                if (scene.isDone)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"ei1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");
                return false;
            }
        }

        private bool SeasonEventFina()
        {
            try
            {
                var location = "15_SeasonEvent";
                if (SceneManager.GetSceneByName(location) != null)
                {
                    SceneManager.UnloadSceneAsync(location);
                }

                return true;
            }
            catch (Exception e)
            {
                string timeStamp = TimeStamp().ToString();
                CatalogError($"see1, {timeStamp}");
                ErrorDataSent($"{timeStamp}_{e}");

                return false;
            }
        }

        // ==================================================
        // Portal Error
        // ==================================================
        private void PortalError(string code)
        {
            Portal.status = PortalStatusType.Error;
        }


        // ==================================================
        // Portal Cancel
        // ==================================================
        public void PortalCancel()
        {
            Portal.status = PortalStatusType.Cancel;
        }


        // ==================================================
        // Portal Debug 
        // ==================================================
        private void PortalDebug(string debug)
        {

#if UNITY_EDITOR
            Debug.Log(debug);
#endif

        }

        // ==================================================
        // Portal popup Callback
        // ==================================================
        public void OnPopupUpdate(PopupRedirect redirect)
        {
            if (redirect.code == "Error-out")
            {
                Debug.Log("Exit");
                PortalExit("");
            }
        }

        #endregion

    }

}


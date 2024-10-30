using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metalive
{
    public class Portal
    {

        /// <summary>
        /// Portal의 사용 여부 체크
        /// </summary>
        public static bool IsUse { get; set; }

        /// <summary>
        /// Portal의 현재 상태 체크
        /// </summary>
        public static PortalStatusType status { get; set; }

        /// <summary>
        /// Portal 카탈로그 DB
        /// </summary>
        public static Dictionary<string, string> catalogs { get; set; }



        // ==================================================
        // [ Portal ]
        // ==================================================

        /// <summary>
        /// 액티비티 씬 활성화된 씬 이름
        /// </summary>
        public static string activateScene { get; set; }


        /// <summary>
        /// 액티비티 씬 사용 가능여부
        /// </summary>
        /// <returns></returns>
        public static bool IsAvailable() => IsAvailable(activateScene);

        public static bool IsAvailable(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid();
        }


        /// <summary>
        /// 액티비트 씬 오픈
        /// </summary>        
        public static void Open(string sceneName) => OpenAsync(sceneName).Forget();

        private static async UniTask OpenAsync(string sceneName)
        {
            if (IsAvailable(sceneName))
            {
                return;
            }
            else
            {
                if (IsAvailable())
                {
                    await CloseAsync(activateScene);
                }
            }

            var scene = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await scene;

            if (scene.isDone)
            {
                activateScene = sceneName;
            }
        }


        /// <summary>
        /// 액티비티 씬 닫기
        /// </summary>
        /// <param name="sceneName"></param>
        public static void Close(string sceneName) => CloseAsync(sceneName).Forget();

        public static async UniTask CloseAsync(string sceneName)
        {
            var scene = SceneManager.UnloadSceneAsync(sceneName);
            await scene;

            if (!scene.isDone)
            {
                activateScene = sceneName;
            }
        }

        public static void SystemInit()
        {
            IsUse = false;
            // PortalStatusType.Exit 좀 더 생각 필요
            // status = PortalStatusType.None;
            catalogs = null;
            activateScene = null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Metalive
{
    public class Video360Manager : MonoBehaviour
    {
        [SerializeField]
        private MediaPlayer mediaPlayer;

        void Awake()
        {
            // 비디오 셋팅옵션 값에 따라 재생바 나오도록 지정
            string interactiveValue = Setting.Interactive.value;

            if (interactiveValue == "101_Video")
            {
                Setting.Video.option = true;
            }
            else if (interactiveValue == "102_Video360")
            {
                Setting.Video.option = false;
            }
        }

        async void Start()
        {
            // Interactive Click에서 바로 비디오 씬으로 이동하는 분기 추가(2023-06-26 최재호)
            string interactiveValue = Setting.Interactive.value;
            bool isBora = Setting.Interactive.category == (int)InteractiveType.Bora;
            if (!isBora && (interactiveValue == "101_Video" || interactiveValue == "102_Video360"))
            {
                AsyncOperationHandle<InteractiveEventData> data = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);
                await data;

                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    InteractiveEventData eventData = data.Result;
                    string url = null;
                    if (Setting.World.code == 9002)
                    {
                        url = eventData.eventTable.Find(x => x.key == Setting.Server.platform + "url").value;
                    }
                    else
                    {
                        url = eventData.eventTable.Find(x => x.key == "url").value;
                    }

                    Setting.Video.url = url;
                }
                Addressables.Release(data);
            }
            mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, Setting.Video.url, false);
            mediaPlayer.Play();
        }

        private void OnDestroy()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.ForceDispose();
            }
        }

        //private void Update()
        //{
        //    Debug.Log($"Video Option : {Setting.Video.option}");
        //}
    }
}
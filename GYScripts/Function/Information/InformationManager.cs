using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Metalive
{

    [RequireComponent(typeof(InformationController))]
    public class InformationManager : MonoBehaviour
    {

        #region Variable

        private InformationController controller;
        private AsyncOperationHandle<MetaliveInformationData> data;

        #endregion



        #region Lifecycle

        private void Start()
        {
            if (controller == null)
            {
                controller = GetComponent<InformationController>();
                if (controller)
                {
                    Patch();
                }
            }
        }

        private void OnDestroy()
        {
            if (controller)
            {
                controller = null;
            }

            if (data.IsValid())
            {
                Addressables.Release(data);
            }
        }

        #endregion



        #region Method

        // 월드 정보 가져오기
        private void Patch() => PatchAsync().Forget();

        private async UniTask PatchAsync()
        {
            try
            {
                //Debug.Log("Server Type: " + Setting.Server.type);
                if (Setting.Server.type == 1)//Release
                {
                    controller.testTmp.gameObject.SetActive(false);
                }
                else
                {
                    controller.testTmp.gameObject.SetActive(true);
                    controller.testTmp.SetText($"Lobby: {NetworkManager.Instance.lobbyName}, Room: {NetworkManager.Instance.roomName}");
                }


                if (controller.informationScroll)
                {
                    controller.informationScroll.enabled = false;
                    controller.informationScroll.content.gameObject.SetActive(false);
                }

                // 월드정보
                // worldcode_0_2
                var address = $"{Setting.World.code}_0_2";
                data = Addressables.LoadAssetAsync<MetaliveInformationData>(address);
                await data;

                if (data.Status == AsyncOperationStatus.Succeeded)
                {
                    // Task => Success
                    if (!data.Result.IsInformation)
                    {
                        // [1]
                        // 월드 정보가 없을때 정보가 없다는 표시
                        controller.informationScroll.enabled = false;
                        controller.informationScroll.content.gameObject.SetActive(true);

                        controller.informationItem.gameObject.SetActive(false);
                        controller.informationHelp.gameObject.SetActive(true);
                        return;
                    }

                    // 아이템 가져오기
                    var item = controller.informationItem.GetComponent<InformationItem>();
                    if (item == null)
                    {
                        // [2]
                        // 아이템이 없으면 월드 정보 표시 안함
                        controller.informationScroll.enabled = false;
                        controller.informationScroll.content.gameObject.SetActive(true);

                        controller.informationItem.gameObject.SetActive(false);
                        controller.informationHelp.gameObject.SetActive(true);
                        return;
                    }

                    // ==================================================
                    //
                    // [1], [2]
                    // 코드 통합 진행 필요
                    //
                    // ==================================================

                    // 이미지 설정
                    if (item.informationImage)
                    {
                        if (data.Result.informationImage)
                        {
                            item.informationImage.color = Color.white;
                            item.informationImage.sprite = data.Result.informationImage;
                        }
                        else
                        {
                            item.informationImage.color = new Color(217 / 255f, 217 / 255f, 217 / 255f, 255 / 255f);
                        }
                    }

                    // 현재 채널 설정
                    if (item.informationChannel)
                    {
                        item.informationChannel.text = $"채널 {Setting.Network.channel}";
                    }

                    // 가상월드 이름 설정
                    if (item.informationTitle)
                    {
                        item.informationTitle.text = data.Result.informationTitle;
                    }

                    // 가상월드 영어 이름 설정
                    if (item.informationSubTitle)
                    {
                        item.informationSubTitle.text = data.Result.informationSubTitle;
                    }

                    // 가상월드 설명 설정
                    if (item.informationDescription)
                    {
                        item.informationDescription.text = data.Result.informationDescription;
                        item.informationDescription.rectTransform.sizeDelta = new Vector2(1280f, item.informationDescription.preferredHeight);
                    }

                    // 스크롤 사이즈 재조정
                    var size = new Vector2(1280f, 960f + item.informationDescription.preferredHeight);
                    controller.informationScroll.content.sizeDelta = size;

                    controller.informationScroll.enabled = true;
                    controller.informationScroll.content.gameObject.SetActive(true);
                }
                else
                {
                    Addressables.Release(data);

                    controller.informationScroll.enabled = false;
                    controller.informationScroll.content.gameObject.SetActive(true);
                    return;
                }

                // 스크롤사용
                if (controller.informationScroll)
                {
                    controller.informationScroll.enabled = true;
                    controller.informationScroll.content.gameObject.SetActive(true);
                }

                controller.informationItem.gameObject.SetActive(true);
                controller.informationHelp.gameObject.SetActive(false);
            }
            catch
            {
                if (controller.informationScroll)
                {
                    controller.informationScroll.enabled = false;
                    controller.informationScroll.content.gameObject.SetActive(true);
                }

                controller.informationItem.gameObject.SetActive(false);
                controller.informationHelp.gameObject.SetActive(true);
            }
        }

        #endregion

    }

}
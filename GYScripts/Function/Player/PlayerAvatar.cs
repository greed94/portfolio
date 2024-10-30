// ==================================================
//
// * 담당자 김근영
//
// ==================================================

using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Metalive
{
    public class AvatarDressTable
    {
        public int itemNo { get; set; }
        public string itemName { get; set; }
        public string itemMesh { get; set; }
        public string itemMaterial { get; set; }
        public List<string> morphList { get; set; }
    }

    public class AvatarDressRedirect
    {
        public string result { get; set; }
        public AvatarDressData data { get; set; }
        public string message { get; set; }
    }

    public class AvatarDressData
    {
        public string userCodeNo { get; set; }
        public string sex { get; set; }
        public AvatarDressTable outfitNo { get; set; }
        public AvatarDressTable topNo { get; set; }
        public AvatarDressTable bottomsNo { get; set; }
        public AvatarDressTable hairNo { get; set; }
        public AvatarDressTable shoesNo { get; set; }
        public AvatarDressTable socksNo { get; set; }
        public AvatarDressTable underwareNo { get; set; }
        public AvatarDressTable capNo { get; set; }
        public AvatarDressTable hairaccNo { get; set; }
        public AvatarDressTable maskNo { get; set; }
        public AvatarDressTable glassNo { get; set; }
        public AvatarDressTable ringNo { get; set; }
        public AvatarDressTable brceletNo { get; set; }
        public AvatarDressTable necklaceNo { get; set; }
        public AvatarDressTable earringNo { get; set; }
        public AvatarDressTable bagNo { get; set; }
        public AvatarDressTable wingNo { get; set; }
        public AvatarDressTable nailNo { get; set; }
        public AvatarDressTable gloveNo { get; set; }
        public AvatarDressTable watchNo { get; set; }
        public AvatarDressTable stickNo { get; set; }
        public AvatarDressTable etcNo { get; set; }
        public AvatarDressTable bodyNo { get; set; }
        public AvatarDressTable headNo { get; set; }
        public AvatarDressTable eyelashNo { get; set; }
        public AvatarDressTable eyebrowNo { get; set; }
        public AvatarDressTable eyeballNo { get; set; }
        public AvatarDressTable toothNo { get; set; }
        public AvatarDressTable beardNo { get; set; }
        public AvatarDressTable equipmentNo { get; set; }
        public AvatarDressTable bandNo { get; set; }
    }

    public class PlayerAvatar : MonoBehaviour
    {
        public AvatarDressData playerAvatarData { get; set; }

        // public void PlayerMeti(string code)
        public async UniTask PlayerMeti(string code)
        {
            // PlayerMetiAsync(code).Forget();
            await PlayerMetiAsync(code);
        }

        public async UniTask NPCMeti(string npcNo)
        {
            await NPCMetiAsync(npcNo);
        }

        private SkinnedMeshRenderer bodyRenderer;

        public async UniTask DownloadAndInstantiateItem(string itemName, bool instantiate)
        {
            if (instantiate)
            {
                var location = itemName;
                var size = Addressables.GetDownloadSizeAsync(location);
                await size;

                if (size.Result > 0)
                {
                    var download = Addressables.DownloadDependenciesAsync(location);
                    await download;

                    if (download.IsValid())
                    {
                        Addressables.Release(download);
                    }
                }

                if (size.IsValid())
                {
                    Addressables.Release(size);
                }

                var data = Addressables.LoadAssetAsync<GameObject>(location);
                await data;
                var item = Instantiate(data.Result, transform);
                item.name = itemName;
                AnimationSynchronizer(item);

                if (itemName == "avatar_female" || itemName == "avatar_male")
                {
                    bodyRenderer = item.transform.Find($"1_Avatar/{itemName}_body_002").GetComponent<SkinnedMeshRenderer>();
                }
            }
            else
            {
                GameObject item = transform.Find(itemName).gameObject;

                DestroyImmediate(item, false);
            }
        }

        public Animator otherAnimator; // 동기화할 다른 오브젝트의 Animator
        private Animator thisAnimator; // 현재 GameObject의 Animator

        private void AnimationSynchronizer(GameObject item)
        {
            if (otherAnimator == null)
            {
                otherAnimator = GetComponent<Animator>();
            }

            // 현재 GameObject의 Animator 컴포넌트를 가져옵니다.
            thisAnimator = item.GetComponent<Animator>();

            // // 두 Animator가 모두 null이 아닌지 확인합니다.
            // if (otherAnimator != null && thisAnimator != null)
            // {
            //     // otherAnimator에서 thisAnimator로 RuntimeAnimatorController를 동기화합니다.
            thisAnimator.runtimeAnimatorController = otherAnimator.runtimeAnimatorController;

            //     // 현재 재생 중인 애니메이션의 재생 시간을 가져옵니다.
            //     float playbackTime = otherAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            //     // 현재 재생 중인 클립의 이름을 가져옵니다.
            //     AnimatorClipInfo[] clipInfo = otherAnimator.GetCurrentAnimatorClipInfo(0);
            //     if (clipInfo.Length > 0)
            //     {
            //         string clipName = clipInfo[0].clip.name;
            //         Debug.Log(clipName);

            //         // 동일한 재생 시간으로 현재 애니메이션을 재생합니다.
            //         // thisAnimator.Play(clipName, -1, playbackTime);
            //         for (int i = 0; i < transform.childCount; i++)
            //         {
            //             transform.GetChild(i).GetComponent<Animator>().Play(clipName, -1, playbackTime);
            //         }
            //     }
            //     else
            //     {
            //         Debug.LogWarning("GameObject에 클립 정보가 없습니다.");
            //     }
            // }
            // else
            // {
            //     Debug.LogError("'GameObject' 또는 현재 GameObject에 Animator가 없습니다.");
            // }
        }

        public void AnimationSynchUpdate(RuntimeAnimatorController animatorController, int hash)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var animator = transform.GetChild(i).GetComponent<Animator>();
                animator.runtimeAnimatorController = animatorController;
                animator.Play(hash);
            }
        }

        public void AnimationSynchUpdate(int hash)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                try
                {
                    transform.GetChild(i).GetComponent<Animator>().Play(hash);
                }
                catch
                {
                }
            }
        }

        private async UniTask NPCMetiAsync(string npcNo)
        {
            if (Setting.World.code == 4066)
            {
                npcNo = "4";
            }
            string url = $"https://{Setting.Server.api}/api/ingame/npc?npcNo={npcNo}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return;
                }
                else
                {
                    playerAvatarData = JsonConvert.DeserializeObject<AvatarDressRedirect>(request.downloadHandler.text).data;
                    MetiSex(playerAvatarData.sex);
                    MetiDress();
                }
            }
        }


        private async UniTask PlayerMetiAsync(string code)
        {
            string url = $"https://{Setting.Server.api}/api/user/others-dress?userCodeNo={code}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", Setting.User.accessToken);
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return;
                }
                else
                {
                    playerAvatarData = JsonConvert.DeserializeObject<AvatarDressRedirect>(request.downloadHandler.text).data;
                    Debug.Log(request.downloadHandler.text);
                    MetiSex(playerAvatarData.sex);
                    MetiDress();
                }
            }
        }

        public void MetiSex(string sex)
        {
            if (sex.Equals("W"))
            {
                DownloadAndInstantiateItem("avatar_female", true).Forget();
            }
            else
            {
                DownloadAndInstantiateItem("avatar_male", true).Forget();
            }
        }

        public void MetiDress()
        {
            if (playerAvatarData.outfitNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.outfitNo.itemName, true).Forget();
            }

            if (playerAvatarData.topNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.topNo.itemName, true).Forget();
            }

            if (playerAvatarData.bottomsNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.bottomsNo.itemName, true).Forget();
            }

            if (playerAvatarData.hairNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.hairNo.itemName, true).Forget();
            }

            if (playerAvatarData.shoesNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.shoesNo.itemName, true).Forget();
            }

            if (playerAvatarData.socksNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.socksNo.itemName, true).Forget();
            }

            if (playerAvatarData.capNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.capNo.itemName, true).Forget();
            }

            if (playerAvatarData.hairaccNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.hairaccNo.itemName, true).Forget();
            }

            if (playerAvatarData.glassNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.glassNo.itemName, true).Forget();
            }

            if (playerAvatarData.necklaceNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.necklaceNo.itemName, true).Forget();
            }

            if (playerAvatarData.earringNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.earringNo.itemName, true).Forget();
            }

            if (playerAvatarData.bagNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.bagNo.itemName, true).Forget();
            }

            if (playerAvatarData.gloveNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.gloveNo.itemName, true).Forget();
            }

            if (playerAvatarData.watchNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.watchNo.itemName, true).Forget();
            }

            if (playerAvatarData.stickNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.stickNo.itemName, true).Forget();
            }

            if (playerAvatarData.equipmentNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.equipmentNo.itemName, true).Forget();
            }

            if (playerAvatarData.bandNo != null)
            {
                DownloadAndInstantiateItem(playerAvatarData.bandNo.itemName, true).Forget();
            }
        }
    }
}
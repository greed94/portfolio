using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Metalive
{
    //201_NPC.Scene
    //작가컴퍼니 NPC 상호작용
    public class NpcManager : MonoBehaviour
    {

        #region Variable

        [SerializeField]
        private NpcController npcController;
        private AsyncOperationHandle<InteractiveEventData> npcData;
        private AsyncOperationHandle<GameObject> npcAvatar;

        private GameObject npc { get; set; }
        private RenderTexture npcRenderTexture { get; set; }
        private List<NpcItem> npcItems { get; set; }

        #endregion



        #region Lifecycle

        private void Start()
        {
            if (npcController)
            {
                Patch().Forget();
            }
        }

        private void OnDestroy()
        {
            if (npcData.IsValid())
            {
                Addressables.Release(npcData);
            }

            if (npcAvatar.IsValid())
            {
                Addressables.Release(npcAvatar);

                if (npcRenderTexture)
                {
                    npcRenderTexture.Release();
                }

                if (npc)
                {
                    DestroyImmediate(npc);
                }

                if (npcController)
                {
                    if (npcController.npcCamera.targetTexture != null)
                    {
                        npcController.npcCamera.targetTexture.Release();
                    }
                }
            }
        }

        #endregion



        #region Method

        private async UniTaskVoid Patch()
        {
            npcData = Addressables.LoadAssetAsync<InteractiveEventData>(Setting.Interactive.code);
            await npcData;

            if (npcData.Status == AsyncOperationStatus.Succeeded)
            {
                AvatarPatch();
                TTSPatch();
                BackgroundPatch();
                NicknamePatch();
                ChatPatch();
            }
        }

        private void AvatarPatch()
        {
            npcAvatar = Addressables.LoadAssetAsync<GameObject>("meti");
            npcAvatar.WaitForCompletion();

            if (npcAvatar.Status == AsyncOperationStatus.Succeeded)
            {
                npc = Instantiate(npcAvatar.Result);
                foreach (Transform child in npc.transform.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Npc");
                }

                npc.transform.localPosition = new Vector3(1000f, -1.05f, 1000.8f);
                npc.transform.localRotation = Quaternion.Euler(0, 180f, 0);
                npc.GetComponentInChildren<Animator>().Play("Hello");


                if (npcController.npcCamera && npcController.npcViewer)
                {
                    npcRenderTexture = new RenderTexture(Screen.height, Screen.height, 16);
                    npcController.npcViewer.texture = npcRenderTexture;
                    npcController.npcViewer.color = Color.white;

                    npcController.npcCamera.targetTexture = npcRenderTexture;
                    npcController.npcCamera.Render();
                }
            }

            // => Avatar Setup
        }

        private void TTSPatch()
        {
            var tts = npcData.Result.eventAudioTable.Find(x => x.key == "tts");
            if (tts != null)
            {
                npcController.npcTTS.clip = tts.value;
                npcController.npcTTS.Play();
            }
        }

        private void BackgroundPatch()
        {
            var background = npcData.Result.eventSpriteTable.Find(x => x.key == "background");
            if (background != null)
            {
                npcController.npcBackground.sprite = background.value;
                npcController.npcBackground.color = Color.white;
            }
            else
            {
                npcController.npcBackground.color = new Color(128f / 255f, 128f / 255f, 128f / 255f, 255f / 255f);
            }
        }

        private void NicknamePatch()
        {
            var nickname = npcData.Result.eventTable.Find(x => x.key == "nickname");
            if (nickname != null)
            {
                npcController.npcNickname.text = nickname.value;
            }
        }

        private void ChatPatch()
        {
            var chatCount = npcData.Result.eventSubTable.Count;
            if (chatCount > 0)
            {
                if (npcController.npcChat)
                {
                    if (npcController.npcChat.content.gameObject.activeSelf)
                    {
                        npcController.npcChat.content.gameObject.SetActive(false);
                    }

                    var width = npcController.npcChat.content.sizeDelta.x;
                    var height = 200f;

                    var item = npcController.npcChat.content.GetChild(0).GetComponent<RectTransform>();
                    item.anchoredPosition = new Vector3(96f, 0, 0);
                    item.sizeDelta = new Vector2(width - 260f, height);
                    if (!item.gameObject.activeSelf)
                    {
                        item.gameObject.SetActive(true);
                    }

                    if (npcItems == null)
                    {
                        npcItems = new List<NpcItem>();
                    }

                    for (int i = 0; i < chatCount; i++)
                    {
                        RectTransform prefab = Instantiate(item, npcController.npcChat.content);
                        npcItems.Add(prefab.GetComponent<NpcItem>());
                        npcItems[i].chat.text = npcData.Result.eventSubTable[i].value;

                        if (i != 0)
                        {
                            var location = -(npcItems[i - 1].item.anchoredPosition.y) + npcItems[i - 1].item.rect.height + 24;
                            npcItems[i].item.anchoredPosition = new Vector3(96, -location, 0);
                        }

                        var itemSize = npcItems[i].chat.GetPreferredValues(npcItems[i].chat.text, npcItems[i].chat.rectTransform.rect.width, 0f).y;
                        npcItems[i].item.sizeDelta = new Vector2(width - 260f, itemSize + 112);
                    }

                    if (item.gameObject.activeSelf)
                    {
                        item.gameObject.SetActive(false); ;
                    }

                    var scrollSize = (-(npcItems[npcItems.Count - 1].item.anchoredPosition.y) + npcItems[npcItems.Count - 1].item.sizeDelta.y) + 120;
                    npcController.npcChat.content.sizeDelta = new Vector2(width, scrollSize);

                    if (!npcController.npcChat.content.gameObject.activeSelf)
                    {
                        npcController.npcChat.content.gameObject.SetActive(true);
                    }
                }
            }
        }

        #endregion

    }
}

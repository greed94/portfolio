using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using UnityEngine;
using UnityEngine.AddressableAssets;

//라마마나 ChatGPT씬에서 우측에 내 캐릭터 표시하는 용도
public class MetiManager : MonoBehaviour
{
    public List<MetiTable> metiTables = new List<MetiTable>();
    public RuntimeAnimatorController animatorNPC;
    private PlayerAvatar playerAvatar;

    private void Start()
    {
        SetupAvatars();
    }

    private void OnDestroy()
    {
        if (playerAvatar != null)
        {
            Destroy(playerAvatar);
        }
    }

    private void SetupAvatars() => SetupAvatarsAsync().Forget();

    private async UniTask SetupAvatarsAsync()
    {
        for (int i = 0; i < metiTables.Count; i++)
        {
            playerAvatar = Instantiate(CreatePlayerManager.CreateMeti, transform).GetComponent<PlayerAvatar>();
            playerAvatar.transform.localPosition = metiTables[i].playerPos;
            playerAvatar.transform.localRotation = Quaternion.Euler(metiTables[i].playerRot);

            switch (metiTables[i].avatar)
            {
                case AvatarType.Player:
                    await playerAvatar.PlayerMeti(Setting.User.userCodeNo.ToString());
                    break;
                case AvatarType.NPC:
                    await playerAvatar.NPCMeti(metiTables[i].npcNo);
                    playerAvatar.GetComponent<Animator>().runtimeAnimatorController = animatorNPC;
                    break;
                case AvatarType.Meti:
                    playerAvatar.MetiSex(metiTables[i].sex.ToString());
                    for (int j = 0; j < metiTables[i].dressList.Count; j++)
                    {
                        await playerAvatar.DownloadAndInstantiateItem(metiTables[i].dressList[j], true);
                    }
                    playerAvatar.AnimationSynchUpdate(animatorNPC, Animator.StringToHash("Hello"));
                    break;
            }

            foreach (var items in playerAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                items.gameObject.layer = metiTables[i].layer;
            }
        }
    }
}

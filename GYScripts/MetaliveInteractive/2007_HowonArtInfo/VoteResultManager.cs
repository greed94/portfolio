using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VoteResultManager : MonoBehaviour
{
    public class Data
    {
        public int howonVoteNo { get; set; }
        public int emojiNo { get; set; }
        public int commentNo { get; set; }
        public int no { get; set; }
        public string nickName { get; set; }
    }

    public class Root
    {
        public string result { get; set; }
        public List<Data> data { get; set; }
        public string message { get; set; }
    }


    public HowonArtInfo howonArtInfo;
    private int lastNo;
    public List<Sprite> emojiSpriteList;
    public List<string> commentList;
    public VoteResultItem voteResultItemPrefab;
    public RectTransform content;
    public List<Color> colorsList;
    public Scrollbar scrollbar;

    public void VoteResultButton()
    {
        Init();
        if (!scrollbar.IsActive())
        {
            VoteResult();
        }
    }

    private void VoteResult() => VoteResultAsync().Forget();

    private async UniTask VoteResultAsync()
    {
        var url = $"https://{Setting.Server.api}/api/howon/vote?artNo={howonArtInfo.artNo}&lastNo={lastNo}&range={12}";

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Setting.User.accessToken);
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var json = JsonConvert.DeserializeObject<Root>(request.downloadHandler.text);

                if (json != null && json.data != null)
                {
                    var data = json.data;
                    Debug.Log(request.downloadHandler.text);
                    Debug.Log(lastNo);
                    foreach (var item in data)
                    {
                        VoteResultItem voteResultItem = Instantiate(voteResultItemPrefab, content);
                        voteResultItem.gameObject.SetActive(true);
                        voteResultItem.name = "vote";
                        voteResultItem.nickNameTMP.text = item.nickName;
                        voteResultItem.commentTMP.text = commentList[item.commentNo - 1];
                        voteResultItem.emojiImage.sprite = emojiSpriteList[item.emojiNo - 1];
                        voteResultItem.BackgroundEmojiImage.color = colorsList[item.emojiNo - 1];
                        voteResultItem.BackgroundImage.color = colorsList[item.emojiNo - 1];

                        switch (lastNo % 3)
                        {
                            case 0:
                                voteResultItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(123, lastNo / 3 * -300);
                                break;

                            case 1:
                                voteResultItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(891, lastNo / 3 * -300);
                                break;

                            case 2:
                                voteResultItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(1659, lastNo / 3 * -300);
                                break;
                        }
                        lastNo++;
                    }

                    content.sizeDelta = new Vector2(0, 300 * ((lastNo + 2) / 3));
                }
            }
        }
    }

    public void OnValueChanged(float value)
    {
        if (value <= 0)
        {
            VoteResult();
        }
    }

    private void Init()
    {
        content.anchoredPosition = Vector2.zero;
    }
}

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VoteManager : MonoBehaviour
{
    public HowonArtInfo howonArtInfo;
    private int commentNo;
    private int emojiNo;
    public List<Toggle> emojiToggleList;
    public List<Toggle> commentToggleList;
    private Color backgroundDefaultColor;
    private Color backgroundSelectedColor;
    private Color textDefaultColor;
    private Color textSelectedColor;

    private void Start()
    {
        for (int i = 0; i < emojiToggleList.Count; i++)
        {
            int index = i;
            Toggle toggle = emojiToggleList[i];
            Image image = toggle.GetComponent<Image>();
            emojiToggleList[i].onValueChanged.AddListener((isOn) => EmojiToggle(toggle, image, index));
        }

        for (int i = 0; i < commentToggleList.Count; i++)
        {
            int index = i;
            Toggle toggle = commentToggleList[i];
            Image image = toggle.GetComponent<Image>();
            TMP_Text text = toggle.GetComponentInChildren<TMP_Text>();
            commentToggleList[i].onValueChanged.AddListener((isOn) => CommentToggle(toggle, image, text, index));
        }

        backgroundDefaultColor = new Color(0.9333333f, 0.9333333f, 0.9333333f, 1);
        backgroundSelectedColor = new Color(0.9176471f, 0.9176471f, 1, 1);
        textDefaultColor = new Color(0.5921569f, 0.6156863f, 0.9490196f, 1);
        textSelectedColor = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1);
    }

    public void CommentToggle(Toggle toggle, Image image, TMP_Text text, int no)
    {
        if (toggle.isOn)
        {
            image.color = backgroundSelectedColor;
            text.color = textDefaultColor;
            commentNo = no + 1;
        }
        else
        {
            image.color = backgroundDefaultColor;
            text.color = textSelectedColor;
            if (commentNo == no + 1)
            {
                commentNo = 0;
            }
        }
    }

    public void EmojiToggle(Toggle toggle, Image image, int no)
    {
        if (toggle.isOn)
        {
            image.color = backgroundSelectedColor;
            emojiNo = no + 1;
        }
        else
        {
            image.color = backgroundDefaultColor;
            if (emojiNo == no + 1)
            {
                emojiNo = 0;
            }
        }
    }

    public void Vote() => VoteAsync().Forget();

    private async UniTask VoteAsync()
    {
        if (commentNo == 0 || emojiNo == 0)
        {
            var hash = new PopupHash()
            {
                title = "아이콘과 문구를\n각각 선택해 주세요.",
                message = " ",
                confirm = "확인"
            };
            Popup.Message(PopupType.General, hash);
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("artNo", howonArtInfo.artNo);
        form.AddField("commentNo", commentNo);
        form.AddField("emojiNo", emojiNo);

        string url = $"https://{Setting.Server.api}/api/howon/vote";
        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.SetRequestHeader("Authorization", Setting.User.accessToken);
            await request.SendWebRequest();
            var hash = new PopupHash()
            {
                title = "투표가 완료되었습니다.",
                message = " ",
                confirm = "확인"
            };
            Popup.Message(PopupType.General, hash);
            Init();
        }
    }

    public void Init()
    {
        if (commentNo != 0)
        {
            commentToggleList[commentNo - 1].isOn = false;
        }

        if (emojiNo != 0)
        {
            emojiToggleList[emojiNo - 1].isOn = false;
        }
    }
}

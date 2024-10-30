using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowonTile : MonoBehaviour
{
    public TextMeshProUGUI messageTmp; //표시할 TextMeshPro

    public string message; //표시할 문자
    public string nickName; //유저 닉네임
    public int userNo; //내껀지 확인할 유저코드번호
    public Image tileImage; //색상 넣을 이미지
    public int messageNo; //수정할때 쓸 메시지 ID

    public HowonTileManager tileManager;

    public RectTransform rectTransform;

    public void SetMessage()
    {
        messageTmp.SetText(message);
    }

    public void HowonTileClick()
    {
        if (!string.IsNullOrEmpty(messageTmp.text))
        {
            tileManager.MessageClick(messageTmp.text, userNo, messageNo, nickName, tileImage.color);
        }
    }
}

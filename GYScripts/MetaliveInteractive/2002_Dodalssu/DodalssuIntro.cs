using System.Collections;
using System.Collections.Generic;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class DodalssuIntro : MonoBehaviour
{
    public ScrollRect scrollRect;
    public DodalssuMain dodalssuMain;
    public TextMeshProUGUI nameTMP;
    public Image introImg;
    public TextMeshProUGUI titleTMP;
    public TextMeshProUGUI subTitleTMP;
    public TextMeshProUGUI explanationTMP;

    public void IntroInit()
    {
        SetInfo();
        LayoutRebuilder.ForceRebuildLayoutImmediate(explanationTMP.rectTransform);
        RectTransform content = scrollRect.content;
        content.sizeDelta = new Vector2(0, 150f);
        content.anchoredPosition = Vector2.zero;

        for (int i = 0; i < content.childCount; i++)
        {
            content.sizeDelta += new Vector2(0, content.GetChild(i).GetComponent<RectTransform>().sizeDelta.y);
        }

        scrollRect.enabled = true;
    }

    public void SetInfo()
    {
        var table = dodalssuMain.result.eventTable;
        nameTMP.text = nameTMP.text.Replace("location", table.Find(x => x.key == "locationKr").value);
        introImg.sprite = dodalssuMain.result.eventSpriteTable.Find(x => x.key == "intro").value;
        titleTMP.text = table.Find(x => x.key == "locationKr").value;
        subTitleTMP.text = table.Find(x => x.key == "locationEn").value;
        explanationTMP.text = table.Find(x => x.key == "explanation").value;
    }
}
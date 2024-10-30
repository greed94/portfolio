using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowonDocentExhibitionIntro : MonoBehaviour
{
    public HowonDocentMain howonDocentMain;
    public TMP_Text title;
    public TMP_Text subTitle;
    public Image exhibitionIntroImg;
    private string closePage;
    public Canvas groupIntroCanvas;
    public RectTransform exhibitionRect;

    public void ExhibitionIntroSetInfo()
    {
        title.text = howonDocentMain.table.Find(x => x.key == "exhibitionIntroTitle").value;
        subTitle.text = howonDocentMain.table.Find(x => x.key == "exhibitionIntroSubTitle").value;
        exhibitionIntroImg.sprite = howonDocentMain.result.eventSpriteTable.Find(x => x.key == "exhibitionIntro").value;
        closePage = "Exhibition";
        exhibitionRect.anchoredPosition = Vector2.zero;
    }

    public void GroupIntroSetInfo(string no)
    {
        title.text = howonDocentMain.table.Find(x => x.key == "groupIntroTitle").value;
        subTitle.text = howonDocentMain.table.Find(x => x.key == "groupIntro" + no).value;
        exhibitionIntroImg.sprite = howonDocentMain.result.eventSpriteTable.Find(x => x.key == "groupIntroDetail" + no).value;
        closePage = "Group";
        exhibitionRect.anchoredPosition = Vector2.zero;
    }

    public void Close()
    {
        switch (closePage)
        {
            case "Exhibition":
                howonDocentMain.mainCanvas.enabled = true;
                GetComponentInParent<Canvas>().enabled = false;
                break;

            case "Group":
                groupIntroCanvas.enabled = true;
                GetComponentInParent<Canvas>().enabled = false;
                break;
        }
    }
}

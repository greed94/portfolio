
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class HowonDocentGroupIntro : MonoBehaviour
{
    public RectTransform groupIntroPrefab;
    public RectTransform groupIntroRecttransform;
    public HowonDocentMain howonDocentMain;
    public HowonDocentExhibitionIntro howonDocentExhibitionIntro;
    // public DocentExhibitionDetailInfo docentExhibitionDetailInfo;

    private void OnEnable()
    {

    }

    public void SetInfoButton(string key)
    {
        test = 80f;
        groupIntroRecttransform.anchoredPosition = new Vector2(test, 0);
        int tet = 1;

        string keyNo;

        while (true)
        {
            keyNo = key + tet.ToString();
            if (howonDocentMain.table.Exists(x => x.key == keyNo))
            {
                Test(keyNo, tet.ToString());
                tet++;
            }
            else
            {
                break;
            }
        }

        groupIntroRecttransform.sizeDelta = new Vector2(test, groupIntroRecttransform.sizeDelta.y);
    }

    private float test;

    private void Test(string key, string no)
    {
        RectTransform prefab = Instantiate(groupIntroPrefab, groupIntroRecttransform);
        prefab.gameObject.SetActive(true);
        IntroPrefab introPrefabTest = prefab.GetComponent<IntroPrefab>();
        prefab.anchoredPosition = new Vector2(test, 0);
        test += 680f;

        introPrefabTest.Image.sprite = howonDocentMain.result.eventSpriteTable.Find(x => x.key == key).value;
        introPrefabTest.Title.text = howonDocentMain.table.Find(x => x.key == key).value;
        introPrefabTest.button.onClick.AddListener(() => howonDocentExhibitionIntro.GroupIntroSetInfo(no));
    }

    public void CloseButton()
    {
        GetComponentInParent<Canvas>().enabled = false;
        howonDocentMain.mainCanvas.enabled = true;
        int prefabCount = groupIntroRecttransform.childCount;
        for (int i = 1; i < prefabCount; i++)
        {
            DestroyImmediate(groupIntroRecttransform.GetChild(1).gameObject);
        }
    }
}


using Cysharp.Threading.Tasks;
using Metalive;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocentIntroManager : MonoBehaviour
{
    public RectTransform introPrefab;
    public RectTransform welcomeIntroPrefab;
    public RectTransform introRecttransform;
    public DocentMainManager docentMainManager;
    public DocentExhibitionDetailInfo docentExhibitionDetailInfo;
    public DocentWriterInfo docentWriterInfo;
    public TMP_Text title;

    private void OnEnable()
    {

    }

    public void SetInfoButton(string key)
    {
        welcomeIntroPrefab.gameObject.SetActive(false);

        if (key.Contains("exhibition"))
        {
            title.text = "기획 전시 소개";
        }
        else if (key.Contains("writer"))
        {
            title.text = "작가 소개";
        }

        int tet = 1;
        test = 0;

        string keyNo;

        while (true)
        {

            keyNo = key + tet.ToString();
            if (docentMainManager.table.Exists(x => x.key == keyNo))
            {
                Test(keyNo);
                tet++;
            }
            else
            {
                break;
            }
        }

        introRecttransform.sizeDelta = new Vector2(test, introRecttransform.sizeDelta.y);
    }

    private float test;

    private void Test(string key)
    {
        RectTransform prefab;

        if (Setting.World.code == 4066 && key.Contains("exhibition"))
        {
            prefab = welcomeIntroPrefab;
            prefab.gameObject.SetActive(true);
        }
        else
        {
            prefab = Instantiate(introPrefab, introRecttransform);
            prefab.gameObject.SetActive(true);
            prefab.anchoredPosition = new Vector2(test, 0);
            test += 890f;
        }

        IntroPrefab introPrefabTest = prefab.GetComponent<IntroPrefab>();

        // introPrefabTest.Image.sprite = keyKr;
        // introPrefabTest.Title.text = artTable.Find(x => x.key == key).value;
        // introPrefabTest.SubTitle.onClick.RemoveAllListeners();
        introPrefabTest.Image.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key).value;
        introPrefabTest.Title.text = docentMainManager.table.Find(x => x.key == key).value;
        introPrefabTest.button.onClick.RemoveAllListeners();
        if (key.Contains("exhibition"))
        {
            if (Setting.World.code == 4057)
            {
                introPrefabTest.button.onClick.AddListener(() => SetExhibitionDetailInfo(key));
            }
            else if (Setting.World.code == 4066)
            {
                introPrefabTest.button.onClick.AddListener(() => WelcomeHomeSetExhibitionDetailInfo(key));
            }
        }
        else if (key.Contains("writer"))
        {
            if (Setting.World.code == 4057)
            {
                introPrefabTest.button.onClick.AddListener(() => SetWriterDetailInfo(key));
            }
            else if (Setting.World.code == 4066)
            {
                introPrefabTest.button.onClick.AddListener(() => WelcomeHomeSetWriterDetailInfo(key));
            }
        }
    }

    private void SetExhibitionDetailInfo(string key)
    {
        GetComponentInParent<Canvas>().enabled = false;
        docentExhibitionDetailInfo.GetComponentInParent<Canvas>().enabled = true;
        docentExhibitionDetailInfo.ArtImg.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key).value;
        docentExhibitionDetailInfo.TitleTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailTitle").value;
        docentExhibitionDetailInfo.PeriodTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailPeriod").value;
        docentExhibitionDetailInfo.LocationTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailLocation").value;
        docentExhibitionDetailInfo.WriterTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailWriter").value;
        if (docentMainManager.table.Exists(x => x.key == key + "DetailWork"))
        {
            docentExhibitionDetailInfo.WorkValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailWork").value;
            docentExhibitionDetailInfo.WorkKeyTMP.gameObject.SetActive(true);
            docentExhibitionDetailInfo.WorkValueTMP.gameObject.SetActive(true);
        }
        else
        {
            docentExhibitionDetailInfo.WorkKeyTMP.gameObject.SetActive(false);
            docentExhibitionDetailInfo.WorkValueTMP.gameObject.SetActive(false);
        }
        docentExhibitionDetailInfo.ExplanationTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailExplanation").value;
        docentExhibitionDetailInfo.audioSource.clip = docentMainManager.result.eventAudioTable.Find(x => x.key == key).value;
        docentExhibitionDetailInfo.Play();
        ExhibitionDetailInit();
    }

    private void SetWriterDetailInfo(string key)
    {
        GetComponentInParent<Canvas>().enabled = false;
        docentWriterInfo.GetComponentInParent<Canvas>().enabled = true;
        docentWriterInfo.artImg.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key).value;
        docentWriterInfo.nameValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailName").value;
        docentWriterInfo.birthdayValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailBirthday").value;
        docentWriterInfo.introValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailIntro").value;
        docentWriterInfo.educationValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailEducation").value;
        docentWriterInfo.soloExhibitionValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailSoloExhibition").value;
        docentWriterInfo.groupExhibitionValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailGroupExhibition").value;
        docentWriterInfo.audioSource.clip = docentMainManager.result.eventAudioTable.Find(x => x.key == key).value;
        docentWriterInfo.Play();
        WriterDetailInit();
    }

    private void WelcomeHomeSetExhibitionDetailInfo(string key)
    {
        GetComponentInParent<Canvas>().enabled = false;
        docentExhibitionDetailInfo.GetComponentInParent<Canvas>().enabled = true;
        // docentExhibitionDetailInfo.ArtImg.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key).value;
        docentExhibitionDetailInfo.ArtImg.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key + "Detail").value;
        docentExhibitionDetailInfo.TitleTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailTitle").value;
        docentExhibitionDetailInfo.PeriodTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailPeriod").value;
        docentExhibitionDetailInfo.LocationTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailLocation").value;
        docentExhibitionDetailInfo.WriterTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailWriter").value;
        if (docentMainManager.table.Exists(x => x.key == key + "DetailWork"))
        {
            docentExhibitionDetailInfo.WorkValueTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailWork").value;
            docentExhibitionDetailInfo.WorkKeyTMP.gameObject.SetActive(true);
            docentExhibitionDetailInfo.WorkValueTMP.gameObject.SetActive(true);
        }
        else
        {
            docentExhibitionDetailInfo.WorkKeyTMP.gameObject.SetActive(false);
            docentExhibitionDetailInfo.WorkValueTMP.gameObject.SetActive(false);
        }
        docentExhibitionDetailInfo.ExplanationTMP.text = docentMainManager.table.Find(x => x.key == key + "DetailExplanation").value;
        docentExhibitionDetailInfo.audioSource.clip = docentMainManager.result.eventAudioTable.Find(x => x.key == key).value;
        docentExhibitionDetailInfo.Play();
        ExhibitionDetailInit();
    }

    private void WelcomeHomeSetWriterDetailInfo(string key)
    {
        GetComponentInParent<Canvas>().enabled = false;
        docentWriterInfo.GetComponentInParent<Canvas>().enabled = true;
        Debug.Log(key);
        docentWriterInfo.artImg.sprite = docentMainManager.result.eventSpriteTable.Find(x => x.key == key).value;
        docentWriterInfo.nameValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailName").value;
        docentWriterInfo.birthdayValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailBirthday").value;
        docentWriterInfo.introValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailIntro").value;
        // docentWriterInfo.educationValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailEducation").value;
        // docentWriterInfo.soloExhibitionValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailSoloExhibition").value;
        // docentWriterInfo.groupExhibitionValueTMP.text = docentMainManager.subTable.Find(x => x.key == key + "DetailGroupExhibition").value;
        docentWriterInfo.audioSource.clip = docentMainManager.result.eventAudioTable.Find(x => x.key == key).value;
        docentWriterInfo.Play();
        WriterDetailInit();
    }

    public void CloseButton()
    {
        GetComponentInParent<Canvas>().enabled = false;
        docentMainManager.mainCanvas.enabled = true;
        int prefabCount = introRecttransform.childCount;
        for (int i = 1; i < prefabCount; i++)
        {
            DestroyImmediate(introRecttransform.GetChild(1).gameObject);
        }
    }

    private void ExhibitionDetailInit()
    {
        docentExhibitionDetailInfo.scrollRect.enabled = false;
        RectTransform content = docentExhibitionDetailInfo.scrollRect.content;
        float trY = 40;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform prefab = content.GetChild(i).GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(prefab);

            if (prefab.gameObject.activeSelf)
            {
                if (prefab.name.Contains("Key") || prefab.name.Contains("ExplanationValue"))
                {
                    trY -= 40f;
                }

                prefab.anchoredPosition = new Vector2(prefab.anchoredPosition.x, trY);

                if (prefab.name.Contains("Value") || prefab.name.Contains("ExplanationKey"))
                {
                    trY -= prefab.sizeDelta.y;
                }
            }
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, -trY);
        content.anchoredPosition = Vector2.zero;
        docentExhibitionDetailInfo.scrollRect.enabled = true;
    }

    private void WriterDetailInit()
    {
        docentWriterInfo.scrollRect.enabled = false;
        RectTransform content = docentWriterInfo.scrollRect.content;
        float trY = 16;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform prefab = content.GetChild(i).GetComponent<RectTransform>();
            if (Setting.World.code == 4066 && prefab.name.Contains("Education"))
            {
                break;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(prefab);

            if (prefab.name.Contains("Key"))
            {
                trY -= 80f;
            }
            else
            {
                trY -= 16f;
            }
            prefab.anchoredPosition = new Vector2(prefab.anchoredPosition.x, trY);
            trY -= prefab.sizeDelta.y;
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, -trY);
        content.anchoredPosition = Vector2.zero;

        docentWriterInfo.scrollRect.enabled = true;
    }
}

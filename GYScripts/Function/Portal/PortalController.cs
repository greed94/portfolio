using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortalController : MonoBehaviour
{

    #region Variable

    public Canvas portal;
    public Canvas portalProgress;
    public TMP_Text progressStatus;
    public TMP_Text progressPercentage;
    public Image progressGauge;

    #endregion



    #region Check

    public bool IsProgress { get; set; }

    #endregion



    #region Lifecycle

    /// <summary>
    /// 포탈씬에 있는 정보 초기화
    /// </summary>
    private void OnDestroy()
    {
        if(progressStatus)
        {
            progressStatus = null;
        }

        if(progressPercentage)
        {
            progressPercentage = null;
        }

        if(progressGauge)
        {
            progressGauge = null;
        }
    }

    #endregion



    #region Method

    // ==================================================
    // [ Portal ]
    // ==================================================

    /// <summary>
    /// 포탈 씬 열기
    /// </summary>
    public void PortalOpen()
    {
        if(!portal.enabled)
        {
            portal.enabled = true;
        }
    }

    /// <summary>
    /// 포탈 씬 닫기
    /// </summary>
    public void PortalClose()
    {
        if (portal.enabled)
        {
            portal.enabled = false;
        }
    }


    // ==================================================
    // [ Progress ]
    // ==================================================
    
    /// <summary>
    /// 로딩 창 열기
    /// </summary>
    public void ProgressOpen()
    {
        if(!portalProgress.enabled)
        {
            portalProgress.enabled = true;
        }
    }

    /// <summary>
    /// 로딩 창 닫기
    /// </summary>
    public void ProgressClose()
    {
        if (portalProgress.enabled)
        {
            portalProgress.enabled = false;
        }
    }

    /// <summary>
    /// 로딩 창 초기화
    /// </summary>
    public void ProgressInit()
    {
        IsProgress = false;
        progressStatus.text = "";
        progressPercentage.text = "";        
        progressGauge.fillAmount = 0;
    }

    /// <summary>
    /// 로딩 창 멈추기
    /// </summary>
    public void ProgressBreak()
    {
        IsProgress = false;
    }

    /// <summary>
    /// 로딩 창 다운로드 적용 => 다운로드 값을 알수있을때
    /// </summary>
    public void ProgressDownload(float percente)
    {
        float timer = 1 / percente;
        float amount = Mathf.Lerp(progressGauge.fillAmount, percente, timer);
        progressStatus.text = $"리소스 다운로드 중...({(progressGauge.fillAmount * 100).ToString("F1")}%)";
        progressPercentage.text = $"{(progressGauge.fillAmount * 100).ToString("F1")}%";
        progressGauge.fillAmount = amount;
    }

    /// <summary>
    /// 로딩 창 다운로드 적용 => 환경에 맞게 다운로드를 적용해야할때
    /// </summary>
    public async void ProgressEnviorment()
    {
        IsProgress = true;
        while (IsProgress)
        {
            float amount = Mathf.Lerp(progressGauge.fillAmount, 1, Time.deltaTime);
            progressStatus.text = "가상월드 입장 중...";
            progressPercentage.text = $"{(progressGauge.fillAmount * 100).ToString("F1")}%";
            progressGauge.fillAmount = amount;
            await UniTask.Yield();
        }

        progressStatus.text = "";
        progressPercentage.text = "100%";
        progressGauge.fillAmount = 1f;
    }

    /// <summary>
    /// 현재 다운로드 퍼센테이지를 바로 적용해야할때
    /// </summary>
    /// <param name="percentage"></param>
    public void ProgressPercentage(float percentage)
    {
        progressGauge.fillAmount = percentage;
    }

    
    

    #endregion

}

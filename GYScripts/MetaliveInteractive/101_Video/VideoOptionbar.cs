/*
작성자: 최재호(cjh0798@gmail.com)
기능: 비디오 재생바 OnOff 제어를 위한 스크립트
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class VideoOptionbar : MonoBehaviour
{
    [SerializeField] private MediaPlayerUI mediaPlayerUI;

    private void OnEnable()
    {
        mediaPlayerUI.AutoOffOptionBar();
    }

    private void OnDisable()
    {
        mediaPlayerUI.OffOptionBar();
    }
}

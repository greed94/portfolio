using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AdvertisementPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private int videoClipsOrder;
    public List<string> videoUrlList;
    private int playerLayerNo;
    public AudioSource bgm;

    void Start()
    {
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.loopPointReached += OnVideoCompletion;

        VideoStart(videoUrlList[0]);
        playerLayerNo = LayerMask.NameToLayer("Player");
    }

    private void OnVideoCompletion(VideoPlayer source)
    {
        videoClipsOrder++;

        if (videoClipsOrder == videoUrlList.Count)
        {
            videoClipsOrder = 0;
        }

        VideoStart(videoUrlList[videoClipsOrder]);
    }

    private void VideoStart(string url)
    {
        videoPlayer.url = url;
        videoPlayer.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerNo)
        {
            videoPlayer.SetDirectAudioMute(0, false);
            bgm.volume = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayerNo)
        {
            videoPlayer.SetDirectAudioMute(0, true);
            bgm.volume = 0.4f;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

public class SimpledAdvertisementPlayer : MonoBehaviour
{
    public MediaPlayer mediaPlayer;
    public string videoUrl;

    void Start()
    {
        VideoStart(videoUrl);
    }

    private void VideoStart(string url)
    {
        mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, url, false);
        mediaPlayer.Play();
    }
}

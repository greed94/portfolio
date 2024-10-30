using System.Collections;
using System.Collections.Generic;
using Metalive;
using UnityEngine;

public class AdvertisementSoundCheck : MonoBehaviour
{
    private int playerLayerNo;
    private GameObject advertisement;
    public AudioSource bgm;

    private void Start()
    {
        playerLayerNo = LayerMask.NameToLayer("Player");

        // Adevertisement (YoutubePlayerLivestream)
        advertisement = transform.GetChild(0).gameObject;
    }
    // 캐릭터 섹션 입장시 소리 꺼짐 켜짐
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayerNo)
        {
            // videoPlayer.SetDirectAudioMute(0, false);
            advertisement.SetActive(true);
            bgm.volume = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayerNo)
        {
            advertisement.SetActive(false);
            // videoPlayer.SetDirectAudioMute(0, true);
            bgm.volume = 0.4f;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metalive
{
    public class AudioManager : MonoBehaviour
    {

        #region Lifecycle

        // ==================================================
        //
        // [ bgm ]
        // - bgm 정보 없으면
        // => 실행
        // - bgm 정보 있으면
        // => 기록된 정보로 실행
        //
        // ==================================================
        private void Start()
        {            
            var bgm = PlayerPrefs.GetString("Setting-BGM");
            if (string.IsNullOrEmpty(bgm))
            {
                Sound.IsSound = true;
                Sound.Play();
            }
            else
            {
                bool hasBGM = bool.Parse(bgm);
                if (hasBGM)
                {
                    Sound.IsSound = true;
                    Sound.Play();
                }
                else
                {
                    Sound.IsSound = false;
                    Sound.Mute();
                }
            }
        }

        #endregion


    }

}
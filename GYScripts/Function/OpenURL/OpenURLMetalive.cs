using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metalive
{
    public class OpenURLMetalive
    {

        public static void OpenURL(string url)
        {
#if(UNITY_EDITOR)
            Application.OpenURL(url);
#elif UNITY_ANDROID
            OpenURLAndroid(url);
#else
            Application.OpenURL(url);
#endif
        }

        private static void OpenURLAndroid(string url)
        {
            AndroidJavaObject plugin = new AndroidJavaObject("com.awesomepia.metalive.unity.Portal");
            plugin.Call("worldLink", url);
        }

    }
}

/*

  

 
 
*/
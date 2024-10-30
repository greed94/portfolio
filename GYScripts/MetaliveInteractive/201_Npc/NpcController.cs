using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Metalive
{
    public class NpcController : MonoBehaviour
    {
        // ==================================================
        // [ UI ]
        // ==================================================
        [Header("UI")]
        public Image npcBackground;
        public Camera npcCamera;
        public RawImage npcViewer;
   
        // ==================================================
        // [ Chat ]
        // ==================================================
        [Header("Chat")]
        public TMP_Text npcNickname;
        public ScrollRect npcChat;
        public AudioSource npcTTS;
                        
    }
}
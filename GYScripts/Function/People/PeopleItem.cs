using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Metalive
{
    public class PeopleItem : MonoBehaviour
    {

        #region Variable

        // UI
        public string peopleCode;
        public RectTransform peopleItem;
        public Image peopleImage;
        public TMP_Text peopleName;        
        public TMP_Text peopleID;
        public RectTransform peopleMy;

        // Follow
        public bool IsFollow = false;        
        public RectTransform peopleFollow;
        public Button peopleFollowButton;
        public Image peopleFollowShadow;
        public Image peopleFollowBackground;
        public TMP_Text peopleFollowText;

        #endregion

    }

}
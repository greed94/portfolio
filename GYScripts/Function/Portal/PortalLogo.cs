using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Metalive
{
    public class PortalLogo : MonoBehaviour
    {

        #region Variable

        [SerializeField]
        private TMP_Text logo;

        #endregion



        #region Variable

        void Start()
        {
            if(logo)
            {
                switch(Setting.World.identification)
                {
                    case "com.awesomepia.metalive":
                        if(Setting.World.code == 4053)
                        {
                            logo.text = "여행을 떠날 때 꼭 거치는 필수 코스";
                        }
                        else
                        {
                            logo.text = "제주를 즐겨요, 라이브하게";
                        }                        
                        break;
                    case "com.awesomepia.daegu":
                        logo.text = "대구를 즐겨요, 파워풀하게";
                        break;
                    default:
                        logo.text = "여행을 떠날 때 꼭 거치는 필수 코스";
                        break;
                }
            }
        }

        #endregion

    }

}
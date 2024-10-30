using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Metalive
{
    public class PeopleController : MonoBehaviour
    {

        #region Variable

        public ScrollRect peopleScroll;
        public RectTransform peopleItem;
        public readonly Vector2 peopleItemSize = new Vector2(1280f, 220f);

        #endregion

    }

}
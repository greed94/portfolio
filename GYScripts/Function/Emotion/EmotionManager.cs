using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Metalive
{
    [RequireComponent(typeof(EmotionController))]
    public class EmotionManager : MonoBehaviour
    {

        #region Variable

        private EmotionController controller;

        #endregion



        #region Check

        private bool IsUse = false;

        #endregion



        #region Lifecycle

        private void Start()
        {
            controller = gameObject.GetComponent<EmotionController>();
        }

        #endregion



        #region Method

        // Emotion view 활성화 / 비활성화
        public void EmotionAnimation()
        {
            if (IsUse)
            {
                IsUse = false;

                controller.emotion.DOSizeDelta(new Vector2(0f, 168f), 0.1f)
                    .OnComplete(() =>
                    {
                        controller.emotionCanvas.enabled = false;
                    });
            }
            else
            {
                IsUse = true;

                controller.emotion.DOSizeDelta(new Vector2(636f, 291.98f), 0.1f)
                    .OnStart(() =>
                    {
                        controller.emotionCanvas.enabled = true;
                    });
            }
        }

        // 이모션 이벤트 실행
        public void EmotionEvent(string emotion)
        {
            Player.Emotion.animation = emotion;
            Player.EmotionUpdate();
        }

        #endregion

    }
}

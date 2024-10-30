using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Metalive
{
    public class Player
    {

        public class Reset
        {
            /// <summary>
            /// player recode position
            /// </summary>
            public static Vector3 positon { get; set; }

            /// <summary>
            /// player recode rotation
            /// </summary>
            public static Quaternion rotation { get; set; }

            /// <summary>
            /// player recode scale
            /// </summary>
            public static Vector3 scale { get; set; }
        }

        public class Emotion
        {
            /// <summary>
            /// play animation
            /// </summary>
            public static string animation { get; set; }
        }

        public class Jesture
        {
            /// <summary>
            /// play animation
            /// </summary>
            public static string animation { get; set; }
        }

        public class Riding
        {
            /// <summary>
            /// 
            /// </summary>
            public static bool IsUse = false;

            /// <summary>
            /// 
            /// </summary>
            public static string address { get; set; }
        }

        public class Property
        {
            /// <summary>
            /// player walk speed
            /// </summary>
            public static float walkSpeed { get; set; }

            /// <summary>
            /// player run speed
            /// </summary>
            public static float runSpeed { get; set; }

            /// <summary>
            /// player jump height
            /// </summary>
            public static float jumpHeight { get; set; }

            /// <summary>
            /// player gravity value
            /// </summary>
            public static float gravityValue { get; set; }

        }

        public class Keyboard
        {
            public class Zero
            {
                /// <summary>
                /// keyboard zero code
                /// </summary>
                public static string code { get; set; }

                /// <summary>
                /// keyboard hash code
                /// </summary>
                public static int hash { get; set; }
            }

            public class Frist
            {
                /// <summary>
                /// keyboard zero code
                /// </summary>
                public static string code { get; set; }

                /// <summary>
                /// keyboard hash code
                /// </summary>
                public static int hash { get; set; }
            }
        }

        #region Container

        /// <summary>
        /// player callback container
        /// </summary>
        public static List<IPlayerCallback> container;

        /// <summary>
        /// player callback add
        /// </summary>
        /// <param name="target"></param>
        public static void AddCallback(object target)
        {
            var callback = target as IPlayerCallback;

            if (callback != null)
            {
                if (container == null)
                {
                    container = new List<IPlayerCallback>();
                }

                container.Add(callback);
            }
        }

        /// <summary>
        /// player callback remove
        /// </summary>
        /// <param name="target"></param>
        public static void RemoveCallback(object target)
        {
            var callback = target as IPlayerCallback;

            if (callback != null)
            {
                if (container == null)
                {
                    return;
                }

                container.Remove(callback);
            }
        }

        /// <summary>
        /// player transform reset
        /// </summary>
        public static void ResetUpdate()
        {
            if (container.Count == 0)
                return;

            foreach (IPlayerCallback callback in container)
            {
                callback.OnPlayerResetUpdate();
            }
        }

        /// <summary>
        /// player emotion play
        /// </summary>
        public static void EmotionUpdate()
        {
            if (container.Count == 0)
                return;

            foreach (IPlayerCallback callback in container)
            {
                callback.OnPlayerEmotionUpdate();
            }
        }

        /// <summary>
        /// player riding play
        /// </summary>
        public static void RidingUpdate()
        {
            if (container.Count == 0)
                return;

            foreach (IPlayerCallback callback in container)
            {
                callback.OnPlayerRidingUpdate();
            }
        }

        /// <summary>
        /// player property reset
        /// </summary>
        public static void PropertyUpdate()
        {
            if (container.Count == 0)
                return;

            foreach (IPlayerCallback callback in container)
            {
                callback.OnPlayerPropertyUpdate();
            }
        }

        #endregion

        public static void SystemInit()
        {
            Reset.positon = Vector3.zero;
            Reset.rotation = Quaternion.Euler(Vector3.zero);
            Reset.scale = Vector3.zero;
            Emotion.animation = null;
            Jesture.animation = null;
            Riding.IsUse = false;
            Riding.address = null;
            Property.walkSpeed = 0;
            Property.runSpeed = 0;
            Property.jumpHeight = 0;
            Property.gravityValue = 0;
            Keyboard.Zero.code = null;
            Keyboard.Zero.hash = 0;
            Keyboard.Frist.code = null;
            Keyboard.Frist.hash = 0;
            container.Clear();
        }

    }

}
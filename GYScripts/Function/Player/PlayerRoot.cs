namespace Metalive
{
    // Player callback interface
    public interface IPlayerCallback
    {
        /// <summary>
        /// 
        /// </summary>
        public void OnPlayerResetUpdate();

        /// <summary>
        /// 
        /// </summary>
        public void OnPlayerEmotionUpdate();

        /// <summary>
        /// 
        /// </summary>
        public void OnPlayerRidingUpdate();

        /// <summary>
        /// 
        /// </summary>        
        public void OnPlayerPropertyUpdate();   
    }
}
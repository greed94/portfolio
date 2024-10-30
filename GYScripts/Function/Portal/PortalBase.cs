namespace Metalive
{
    /// <summary>
    /// 포탈 씬 상태 여부
    /// </summary>
    public enum PortalStatusType
    {
        None = 0,        
        Enter = 1,
        Exit = 2,
        Connect = 3,
        Disconnect = 4,        
        Cancel = 5,
        Error = 6,
    }

    /// <summary>
    /// 포탈 씬 상태?
    /// </summary>
    public enum PortalSceneType
    {
        None = 0,
        Open = 1,
        Close = 2,
    }

}
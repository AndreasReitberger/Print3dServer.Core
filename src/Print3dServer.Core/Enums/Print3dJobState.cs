namespace AndreasReitberger.API.Print3dServer.Core.Enums
{
    public enum Print3dJobState
    {
        Completed,
        Shutdown,
        InProgress,
        Cancelled,
        Paused,
        Operational,
        Connecting,
        Printing,
        Closed,
        Offline,
        Unknown,
        Error = 99,
    }
}

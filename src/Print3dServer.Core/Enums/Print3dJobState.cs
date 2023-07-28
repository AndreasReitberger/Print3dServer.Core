namespace AndreasReitberger.API.Print3dServer.Core.Enums
{
    public enum Print3dJobState
    {
        Completed,
        Shutdown,
        InProgress,
        Cancelled,

        Error = 99,
    }
}

namespace AndreasReitberger.API.Print3dServer.Core.Enums
{
    public enum Printer3dToolHeadState
    {
        Idle,
        Heating,
        Ready,
        Cooling,

        Error = 99,
    }
}

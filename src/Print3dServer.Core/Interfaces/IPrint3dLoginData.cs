namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dLoginData : IPrint3dBase
    {
        #region Properties
        public string? Username { get; set; }
        public long? Permissions { get; set; }
        #endregion
    }
}

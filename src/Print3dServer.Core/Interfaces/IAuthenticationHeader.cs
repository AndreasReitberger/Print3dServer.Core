namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IAuthenticationHeader
    {
        #region Properties
        public string Token { get; set; }
        public int Order { get; set; }
        public string? Format { get; set; }
        #endregion

    }
}

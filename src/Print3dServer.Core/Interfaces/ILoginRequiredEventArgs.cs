namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface ILoginRequiredEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dLoginData? LoginData { get; set; }
        public bool LoginSucceeded { get; set; }
        #endregion
    }
}

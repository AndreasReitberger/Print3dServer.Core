namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IWebCamConfigChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IWebCamConfig? NewConfig { get; set; }
        #endregion
    }
}

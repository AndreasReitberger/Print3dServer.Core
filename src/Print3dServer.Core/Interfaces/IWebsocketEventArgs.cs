namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IWebsocketEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public string? MessageReceived { get; set; }
        public byte[]? Data { get; set; }
        #endregion
    }
}

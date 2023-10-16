namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IListeningChangedEventArgs : ISessionChangedEventArgs
    {
        #region Properties
        public bool IsListening { get; set; }
        public bool IsListeningToWebSocket { get; set; }
        #endregion
    }
}

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface ISessionChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public string? Session { get; set; }
        #endregion
    }
}

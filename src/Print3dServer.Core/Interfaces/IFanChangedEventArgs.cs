namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IFanChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public string? Name { get; set; }
        public IPrint3dFan? Fan { get; set; }
        #endregion
    }
}

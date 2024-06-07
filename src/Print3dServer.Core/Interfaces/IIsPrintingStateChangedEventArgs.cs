namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IIsPrintingStateChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public bool IsPrinting { get; set; }
        public bool IsPaused { get; set; }
        #endregion
    }
}

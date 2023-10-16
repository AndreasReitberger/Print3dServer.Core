namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IActivePrinterChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrinter3d? NewPrinter { get; set; }
        public IPrinter3d? OldPrinter { get; set; }
        #endregion
    }
}

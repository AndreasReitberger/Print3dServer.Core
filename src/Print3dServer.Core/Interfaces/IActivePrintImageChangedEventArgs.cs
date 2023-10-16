namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IActivePrintImageChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public byte[]? NewImage { get; set; }
        public byte[]? PreviousImage { get; set; }
        #endregion
    }
}

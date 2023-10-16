namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface ITemperatureDataEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dTemperatureInfo? TemperatureInfo { get; set; }

        #endregion
    }
}

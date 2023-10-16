namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dTemperatureInfo : IPrint3dBase
    {
        #region Properties  
        public double? TemperatureOffset { get; set; }
        public double? TemperatureSet { get; set; }
        public double? TemperatureTarget { get; set; }

        #endregion

        #region Methods

        public Task SetTemperatureAsync(double temperature);
        public Task MoveAsync(double x, double y, double z);

        #endregion
    }
}

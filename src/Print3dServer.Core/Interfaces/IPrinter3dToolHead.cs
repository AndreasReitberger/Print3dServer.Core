using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrinter3dToolHead : IPrint3dBase
    {
        #region Properties
        public string Name { get; set; }    
        public double? Temperature { get; set; }
        public double? TemperatureTarget { get; set; }
        public Printer3dToolHeadState State { get; set; }

        #endregion

        #region Methods

        public Task SetTemperatureAsync(double temperature);
        public Task MoveAsync(double x, double y, double z);

        #endregion
    }
}

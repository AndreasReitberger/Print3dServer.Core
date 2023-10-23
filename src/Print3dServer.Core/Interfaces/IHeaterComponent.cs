using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IHeaterComponent
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }
        public double? TempRead { get; set; }
        public double? TempSet { get; set; }
        public long Error { get; set; }

        public Printer3dHeaterType Type { get; set; }
        #endregion  

        #region Methods

        public Task<bool> SetTemperatureAsync(IPrint3dServerClient client, string command, object? data);

        #endregion
    }
}

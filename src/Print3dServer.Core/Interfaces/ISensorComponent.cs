using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface ISensorComponent
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public bool Triggered { get; set; }
        public Printer3dSensorType Type { get; set; }
        #endregion
    }
}

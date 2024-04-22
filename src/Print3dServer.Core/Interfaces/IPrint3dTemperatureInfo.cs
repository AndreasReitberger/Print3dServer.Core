namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dTemperatureInfo : IPrint3dBase
    {
        #region Properties  
        public double? TemperatureOffset { get; set; }
        public double? TemperatureSet { get; set; }
        public double? TemperatureTarget { get; set; }

        #endregion
    }
}

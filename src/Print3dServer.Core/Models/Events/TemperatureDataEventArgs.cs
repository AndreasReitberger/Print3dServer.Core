using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class TemperatureDataEventArgs : Print3dBaseEventArgs, ITemperatureDataEventArgs
    {
        #region Properties
        public IPrint3dTemperatureInfo? TemperatureInfo { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.TemperatureDataEventArgs);
        #endregion
    }
}

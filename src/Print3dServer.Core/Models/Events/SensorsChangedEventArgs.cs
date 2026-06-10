using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class SensorsChangedEventArgs : Print3dBaseEventArgs, ISensorsChangedEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, ISensorComponent> Sensors { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.SensorsChangedEventArgs);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class HeatersChangedEventArgs : Print3dBaseEventArgs, IHeatersChangedEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> Heaters { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.HeatersChangedEventArgs);
        #endregion
    }
}

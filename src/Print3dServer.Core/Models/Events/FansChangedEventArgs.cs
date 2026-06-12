using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class FansChangedEventArgs : Print3dBaseEventArgs, IFansChangedEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, IPrint3dFan> Fans { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.FansChangedEventArgs);
        #endregion
    }
}

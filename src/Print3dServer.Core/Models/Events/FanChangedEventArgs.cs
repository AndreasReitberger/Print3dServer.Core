using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class FanChangedEventArgs : Print3dBaseEventArgs, IFanChangedEventArgs
    {
        #region Properties
        public string? Name { get; set; }
        public IPrint3dFan? Fan { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.FanChangedEventArgs);
        #endregion
    }
}

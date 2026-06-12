using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class ActivePrintImageChangedEventArgs : Print3dBaseEventArgs, IActivePrintImageChangedEventArgs
    {
        #region Properties
        public byte[]? NewImage { get; set; }
        public byte[]? PreviousImage { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.ActivePrintImageChangedEventArgs);
        #endregion
    }
}

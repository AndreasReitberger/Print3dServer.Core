using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class ActivePrinterChangedEventArgs : Print3dBaseEventArgs, IActivePrinterChangedEventArgs
    {
        #region Properties
        public IPrinter3d? NewPrinter { get; set; }
        public IPrinter3d? OldPrinter { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.ActivePrinterChangedEventArgs);
        #endregion
    }
}

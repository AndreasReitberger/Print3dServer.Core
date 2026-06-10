using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class WebCamConfigChangedEventArgs : Print3dBaseEventArgs, IWebCamConfigChangedEventArgs
    {
        #region Properties
        public IWebCamConfig? NewConfig { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.WebCamConfigChangedEventArgs);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class WebCamConfigsChangedEventArgs : Print3dBaseEventArgs, IWebCamConfigsChangedEventArgs
    {
        #region Properties
        public List<IWebCamConfig> NewConfigs { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.WebCamConfigsChangedEventArgs);
        #endregion
    }
}

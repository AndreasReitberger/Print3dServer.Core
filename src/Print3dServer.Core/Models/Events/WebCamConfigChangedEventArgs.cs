using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class WebCamConfigChangedEventArgs : Print3dBaseEventArgs, IWebCamConfigChangedEventArgs
    {
        #region Properties
        public IWebCamConfig? NewConfig { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

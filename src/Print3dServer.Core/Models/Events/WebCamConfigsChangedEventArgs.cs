using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class WebCamConfigsChangedEventArgs : Print3dBaseEventArgs, IWebCamConfigsChangedEventArgs
    {
        #region Properties
        public List<IWebCamConfig> NewConfigs { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

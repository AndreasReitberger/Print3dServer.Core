using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class WebsocketEventArgs : Print3dBaseEventArgs, IWebsocketEventArgs
    {
        #region Properties
        public string? MessageReceived { get; set; }
        public byte[]? Data { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

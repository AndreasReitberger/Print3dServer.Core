using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class RestEventArgs : EventArgs, IRestEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Status { get; set; }
        public Uri? Uri { get; set; }
        public Exception? Exception { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

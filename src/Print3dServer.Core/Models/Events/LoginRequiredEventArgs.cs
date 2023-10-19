using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class LoginRequiredEventArgs : Print3dBaseEventArgs, ILoginRequiredEventArgs
    {
        #region Properties
        public IPrint3dLoginData? LoginData { get; set; }
        public bool LoginSucceeded { get; set; } = false;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

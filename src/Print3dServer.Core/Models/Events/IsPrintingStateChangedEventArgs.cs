using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class IsPrintingStateChangedEventArgs : Print3dBaseEventArgs, IIsPrintingStateChangedEventArgs
    { 
        #region Properties
        public bool IsPrinting { get; set; }
        public bool IsPaused { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class GcodesChangedEventArgs : Print3dBaseEventArgs, IGcodesChangedEventArgs
    {
        #region Properties
        public List<IGcode> NewModels { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

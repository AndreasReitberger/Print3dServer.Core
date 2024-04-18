using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class SensorsChangedEventArgs : Print3dBaseEventArgs, ISensorsChangedEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, ISensorComponent> Sensors { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

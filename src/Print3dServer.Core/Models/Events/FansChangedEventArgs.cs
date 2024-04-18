using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class FansChangedEventArgs : Print3dBaseEventArgs, IFansChangedEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, IPrint3dFan> Fans { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

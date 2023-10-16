using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class IgnoredJsonResultsChangedEventArgs : Print3dBaseEventArgs, IIgnoredJsonResultsChangedEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, string> NewIgnoredJsonResults { get; set; } = new();
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

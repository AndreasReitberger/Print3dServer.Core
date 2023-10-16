using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IIgnoredJsonResultsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, string> NewIgnoredJsonResults { get; set; }

        #endregion
    }
}

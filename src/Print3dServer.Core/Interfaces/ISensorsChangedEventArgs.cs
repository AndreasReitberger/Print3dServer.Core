using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface ISensorsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, ISensorComponent> Sensors { get; set; }
        #endregion
    }
}

using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IFansChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ConcurrentDictionary<string, IPrint3dFan> Fans { get; set; }
        #endregion
    }
}

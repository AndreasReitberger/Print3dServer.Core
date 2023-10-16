using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IJobFinishedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dJob? Job { get; set; }

        #endregion
    }
}

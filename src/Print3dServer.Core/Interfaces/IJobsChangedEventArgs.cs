using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IJobsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dJob? NewJob { get; set; }
        public IPrint3dJob? OldJob { get; set; }

        #endregion
    }
}

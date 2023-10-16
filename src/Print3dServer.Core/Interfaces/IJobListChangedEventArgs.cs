using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IJobListChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ObservableCollection<IPrint3dJob> NewJobList { get; set; }
        #endregion
    }
}

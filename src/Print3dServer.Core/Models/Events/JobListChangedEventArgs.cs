using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobListChangedEventArgs : Print3dBaseEventArgs, IJobListChangedEventArgs
    {
        #region Properties
        public ObservableCollection<IPrint3dJob> NewJobList { get; set; } = new();
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

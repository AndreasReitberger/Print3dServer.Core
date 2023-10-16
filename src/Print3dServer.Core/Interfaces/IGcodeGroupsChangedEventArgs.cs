using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeGroupsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ObservableCollection<IGcodeGroup> NewModelGroups { get; set; }
        #endregion
    }
}

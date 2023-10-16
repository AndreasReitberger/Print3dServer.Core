using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodesChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public ObservableCollection<IGcode> NewModels { get; set; }
        #endregion
    }
}

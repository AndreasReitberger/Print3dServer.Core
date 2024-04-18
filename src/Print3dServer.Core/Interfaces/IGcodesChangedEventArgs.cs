using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodesChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public List<IGcode> NewModels { get; set; }
        #endregion
    }
}

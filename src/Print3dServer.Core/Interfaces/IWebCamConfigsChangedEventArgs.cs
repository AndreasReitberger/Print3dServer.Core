using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IWebCamConfigsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public List<IWebCamConfig> NewConfigs { get; set; }
        #endregion
    }
}

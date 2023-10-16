using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class GcodesChangedEventArgs : Print3dBaseEventArgs, IGcodesChangedEventArgs
    {
        #region Properties
        public ObservableCollection<IGcode> NewModels { get; set; } = new();
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

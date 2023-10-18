using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class PrintersChangedEventArgs : Print3dBaseEventArgs, IPrintersChangedEventArgs
    {
        #region Properties
        public ObservableCollection<IPrinter3d> NewPrinters { get; set; } = new();
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class HeaterChangedEventArgs : Print3dBaseEventArgs, IHeaterChangedEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public int? Index { get; set; }
        public IHeaterComponent? Heater { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

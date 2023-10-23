using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class ToolheadsChangedEventArgs : Print3dBaseEventArgs, IToolheadsChangedEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public ConcurrentDictionary<int, IToolhead> Toolheads { get; set; } = new();
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

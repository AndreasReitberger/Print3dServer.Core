using AndreasReitberger.API.Print3dServer.Core.Enums;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IHeatersChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> Heaters { get; set; }
        #endregion
    }
}

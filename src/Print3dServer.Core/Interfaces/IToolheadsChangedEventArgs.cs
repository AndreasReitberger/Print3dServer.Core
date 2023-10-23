using AndreasReitberger.API.Print3dServer.Core.Enums;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IToolheadsChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public ConcurrentDictionary<int, IToolhead> Toolheads { get; set; }
        #endregion
    }
}

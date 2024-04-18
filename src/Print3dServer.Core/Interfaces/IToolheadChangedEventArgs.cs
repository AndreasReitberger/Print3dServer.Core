using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IToolheadChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public Printer3dHeaterType Type { get; set; }
        public int? Index { get; set; }
        public IToolhead? Toolhead { get; set; }
        #endregion
    }
}

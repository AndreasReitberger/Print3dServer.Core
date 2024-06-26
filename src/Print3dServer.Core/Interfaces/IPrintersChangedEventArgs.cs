﻿using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrintersChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public List<IPrinter3d> NewPrinters { get; set; }
        #endregion
    }
}

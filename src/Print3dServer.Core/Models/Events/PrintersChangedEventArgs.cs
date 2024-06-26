﻿using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class PrintersChangedEventArgs : Print3dBaseEventArgs, IPrintersChangedEventArgs
    {
        #region Properties
        public List<IPrinter3d> NewPrinters { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

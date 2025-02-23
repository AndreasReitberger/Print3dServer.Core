﻿using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class FanChangedEventArgs : Print3dBaseEventArgs, IFanChangedEventArgs
    {
        #region Properties
        public string? Name { get; set; }
        public IPrint3dFan? Fan { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class GcodeGroupsChangedEventArgs : Print3dBaseEventArgs, IGcodeGroupsChangedEventArgs
    {
        #region Properties
        public List<IGcodeGroup> NewModelGroups { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}

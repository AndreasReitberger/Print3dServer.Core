using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class GcodeGroupsChangedEventArgs : Print3dBaseEventArgs, IGcodeGroupsChangedEventArgs
    {
        #region Properties
        public List<IGcodeGroup> NewModelGroups { get; set; } = [];
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.GcodeGroupsChangedEventArgs);
        #endregion
    }
}

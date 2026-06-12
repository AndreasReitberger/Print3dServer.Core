using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobsChangedEventArgs : Print3dBaseEventArgs, IJobsChangedEventArgs
    {
        #region Properties
        public IPrint3dJob? NewJob { get; set; }
        public IPrint3dJob? OldJob { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.JobsChangedEventArgs);
        #endregion
    }
}

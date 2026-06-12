using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobStatusChangedEventArgs : Print3dBaseEventArgs, IJobStatusChangedEventArgs
    {
        #region Properties
        public IPrint3dJobStatus? NewJobStatus { get; set; }
        public IPrint3dJobStatus? PreviousJobStatus { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.JobStatusChangedEventArgs);
        #endregion
    }
}

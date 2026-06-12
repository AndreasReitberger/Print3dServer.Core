using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobStatusFinishedEventArgs : Print3dBaseEventArgs, IJobStatusFinishedEventArgs
    {
        #region Properties
        public IPrint3dJobStatus? JobStatus { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.JobStatusFinishedEventArgs);
        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.SourceGeneration;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobFinishedEventArgs : Print3dBaseEventArgs, IJobFinishedEventArgs
    {
        #region Properties
        public IPrint3dJob? Job { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonSerializer.Serialize(this!, Print3dCoreSourceGenerationContext.Default.JobFinishedEventArgs);
        #endregion
    }
}

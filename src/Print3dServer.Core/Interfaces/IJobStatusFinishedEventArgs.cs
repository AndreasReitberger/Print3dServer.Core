namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IJobStatusFinishedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dJobStatus? JobStatus { get; set; }

        #endregion
    }
}

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IJobStatusChangedEventArgs : IPrint3dBaseEventArgs
    {
        #region Properties
        public IPrint3dJobStatus? NewJobStatus { get; set; }
        public IPrint3dJobStatus? PreviousJobStatus { get; set; }

        #endregion
    }
}

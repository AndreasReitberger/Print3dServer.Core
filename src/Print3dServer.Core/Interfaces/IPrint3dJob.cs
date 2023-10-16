namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dJob : IPrint3dBase
    {
        #region Properties
        public string JobId { get; set; }
        public double? TimeAdded { get; set; }
        public double? TimeInQueue { get; set; }
        public string FileName { get; set; }
        #endregion

        #region Methods

        public Task<bool> StartJobAsync();
        public Task<bool> PauseJobAsync();
        public Task<bool> StopJobAsync();
        public Task<bool> RemoveFromQueueAsync();

        #endregion
    }
}

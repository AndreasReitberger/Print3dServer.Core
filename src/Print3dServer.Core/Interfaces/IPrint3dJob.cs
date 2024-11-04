namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dJob : IPrint3dBase
    {
        #region Properties
        public string JobId { get; set; }
        public double? TimeAdded { get; set; }
        public DateTime? TimeAddedGeneralized { get; set; }
        public double? TimeInQueue { get; set; }
        public DateTime? TimeInQueueGeneralized { get; set; }
        public double? PrintTime { get; set; }
        public TimeSpan? PrintTimeGeneralized { get; set; }
        public string FileName { get; set; }
        #endregion

        #region Methods

        public Task<bool> StartJobAsync(IPrint3dServerClient client, string command, object? data);
        public Task<bool> PauseJobAsync(IPrint3dServerClient client, string command, object? data);
        public Task<bool> StopJobAsync(IPrint3dServerClient client, string command, object? data);
        public Task<bool> RemoveFromQueueAsync(IPrint3dServerClient client, string command, object? data);

        #endregion
    }
}

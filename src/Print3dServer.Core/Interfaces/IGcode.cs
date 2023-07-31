namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcode : IPrint3dBase
    {
        #region Properties

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public double Modified { get;set; }
        public long Size { get; set; }
        public string Permissions { get; set; }
        public string Group { get; set; }
        public byte[] Image { get; set; }
        public IGcodeMeta Meta { get; set; }

        #endregion

        #region Methods
        public Task PrintAsync();
        public Task ViewAsync();
        public Task MoveToAsync(string targetPath, bool copy = false);
        public Task MoveToQueueAsync(bool printIfReady = false);
        #endregion
    }
}

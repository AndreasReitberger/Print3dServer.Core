using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcode : IPrint3dBase
    {
        #region Properties
        public long Identifier { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string PrinterName { get; set; }
        public GcodeImageType ImageType { get; set; }
        public double Modified { get;set; }
        public double Volume { get;set; }
        public double Filament { get;set; }
        public double PrintTime { get;set; }
        public long Size { get; set; }
        public string Permissions { get; set; }
        public string Group { get; set; }
        public byte[] Thumbnail { get; set; }
        public byte[] Image { get; set; }
        public IGcodeMeta Meta { get; set; }

        #endregion

        #region Methods
        public Task PrintAsync(IPrint3dServerClient client);
        //public Task ViewAsync();
        public Task MoveToAsync(IPrint3dServerClient client, string targetPath, bool copy = false);
        public Task MoveToQueueAsync(IPrint3dServerClient client, bool printIfReady = false);
        #endregion
    }
}

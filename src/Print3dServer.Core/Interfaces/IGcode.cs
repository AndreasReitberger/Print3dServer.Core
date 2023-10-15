using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcode
    {
        #region Properties

        public Guid Id { get; set; }
        public long Identifier { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string PrinterName { get; set; }
        public GcodeImageType ImageType { get; set; }
        public double Modified { get;set; }
        public double Volume { get;set; }
        public double Filament { get;set; }
        public long Size { get; set; }
        public string Permissions { get; set; }
        public byte[] Thumbnail { get; set; }
        public byte[] Image { get; set; }
        public IGcodeMeta Meta { get; set; }

        #endregion
    }
}

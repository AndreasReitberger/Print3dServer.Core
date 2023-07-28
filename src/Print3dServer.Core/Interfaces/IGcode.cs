namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcode
    {
        #region Properties

        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public double Modified { get;set; }
        public long Size { get; set; }
        public string Permissions { get; set; }
        public byte[] Image { get; set; }
        public IGcodeMeta Meta { get; set; }

        #endregion
    }
}

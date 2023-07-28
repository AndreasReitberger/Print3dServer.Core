namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeImage
    {
        #region Properties
        public Guid Id { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
        public bool IsPathRelative { get; set; }
        #endregion
    }
}

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeImage : IPrint3dBase
    {
        #region Properties
        public long Width { get; set; }
        public long Height { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
        public bool IsPathRelative { get; set; }
        #endregion
    }
}

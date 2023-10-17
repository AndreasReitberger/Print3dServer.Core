namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IWebCamConfig
    {
        #region Properties
        public string Alias { get; set; }
        public Uri? WebCamUrlDynamic { get; set; }
        public Uri? WebCamUrlStatic { get; set; }
        public long Position { get; set; }
        public long Orientation { get; set; }
        #endregion
    }
}

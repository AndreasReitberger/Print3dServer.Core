namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeGroup
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }     
        public string DirectoryName { get; set; }
        public string Path { get; set; }
        public string Root { get; set; }
        public double? Modified { get; set; }
        public DateTime? ModifiedGeneralized { get; set; }
        public long Size { get; set; }
        public string Permissions { get; set; }
        #endregion
    }
}

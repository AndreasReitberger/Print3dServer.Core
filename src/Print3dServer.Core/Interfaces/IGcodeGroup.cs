namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeGroup
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }

        #endregion
    }
}

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dBase : IDisposable, ICloneable
    {
        #region Properties
        public Guid Id { get; set; }

        #endregion

        #region Methods

        public Task<bool> DeleteAsync();
        #endregion
    }
}

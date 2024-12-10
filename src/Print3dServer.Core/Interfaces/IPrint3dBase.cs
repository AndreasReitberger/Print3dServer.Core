namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    [Obsolete("Use `IRestApiClient` instead")]
    public interface IPrint3dBase : IDisposable, ICloneable
    {
        #region Properties
        public Guid Id { get; set; }

        #endregion
    }
}

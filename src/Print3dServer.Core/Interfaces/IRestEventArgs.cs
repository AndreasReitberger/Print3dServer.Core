namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IRestEventArgs 
    {
        #region Properties
        public string? Message { get; set; }
        public string? Status { get; set; }
        public Uri? Uri { get; set; }
        public Exception? Exception { get; set; }
        #endregion
    }
}

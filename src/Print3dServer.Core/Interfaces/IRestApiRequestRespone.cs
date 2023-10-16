namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IRestApiRequestRespone
    {
        #region Properties
        public string? Result { get; set; } 
        public bool IsOnline { get; set; }
        public bool Succeeded { get; set; }
        public bool HasAuthenticationError { get; set; }

        public IRestEventArgs? EventArgs { get; set; }
        #endregion
    }
}

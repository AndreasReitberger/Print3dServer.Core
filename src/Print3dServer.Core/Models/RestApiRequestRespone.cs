using AndreasReitberger.API.Print3dServer.Core.Interfaces;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class RestApiRequestRespone : ObservableObject, IRestApiRequestRespone
    {
        #region Properties
        [ObservableProperty]
        string? result;

        [ObservableProperty]
        bool isOnline = false;

        [ObservableProperty]
        bool succeeded = false;

        [ObservableProperty]
        bool hasAuthenticationError = false;

        [ObservableProperty]
        IRestEventArgs? eventArgs;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        
        #endregion
    }
}

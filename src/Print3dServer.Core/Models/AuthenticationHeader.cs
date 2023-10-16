using AndreasReitberger.API.Print3dServer.Core.Interfaces;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class AuthenticationHeader : ObservableObject, IAuthenticationHeader
    {
        #region Properties
        [ObservableProperty]
        string? token;

        [ObservableProperty]
        int order;

        [ObservableProperty]
        string? format;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        
        #endregion
    }
}

using System.Security;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

        #region Properties
        [ObservableProperty]
        bool isLoggedIn = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        string username;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        SecureString password;
        #endregion

    }
}

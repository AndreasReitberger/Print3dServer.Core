using System.Security;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

        #region Properties
        [ObservableProperty]
        bool isLoggedIn = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string username;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        SecureString password;
        #endregion

    }
}

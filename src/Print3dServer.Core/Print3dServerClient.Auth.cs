using System.Text.Json.Serialization;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

        #region Properties
        [ObservableProperty]
        public partial bool IsLoggedIn { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
        public partial string? Password { get; set; }
        #endregion

    }
}

using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

        #region Properties
        [ObservableProperty]
        public partial bool IsLoggedIn { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial string Username { get; set; } = string.Empty;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial string? Password { get; set; }
        #endregion

    }
}

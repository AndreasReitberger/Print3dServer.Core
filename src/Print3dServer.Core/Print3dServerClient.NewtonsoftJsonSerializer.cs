/*
using AndreasReitberger.API.Print3dServer.Core.JSON.Newtonsoft;
using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

#if DEBUG 
        #region Debug
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        JsonSerializerSettings newtonsoftJsonSerializerSettings = DefaultNewtonsoftJsonSerializerSettings;

        public new static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
        {
            // Detect if the json respone has more or less properties than the target class
            //MissingMemberHandling = MissingMemberHandling.Error,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                // Map the converters
                new AbstractConverter<AuthenticationHeader, IAuthenticationHeader>(),
            }
        };
        #endregion
#else
        #region Release
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        JsonSerializerSettings newtonsoftJsonSerializerSettings = DefaultNewtonsoftJsonSerializerSettings;

        public new static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
        {
            // Ignore if the json respone has more or less properties than the target class
            MissingMemberHandling = MissingMemberHandling.Ignore,          
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                // Map the converters
                new AbstractConverter<AuthenticationHeader, IAuthenticationHeader>(),
            }
        };
        #endregion
#endif
    }
}
*/

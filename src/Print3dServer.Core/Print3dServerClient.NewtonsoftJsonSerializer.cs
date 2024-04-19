using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.JSON.Newtonsoft;
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

        public static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
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

        public static JsonSerializerSettings DefaultNewtonsoftJsonSerializerSettings = new()
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
        #region Methods

#nullable enable
        public T? GetObjectFromJson<T>(string? json, JsonSerializerSettings? serializerSettings = null)
        {
            try
            {
                // Workaround
                // The HttpClient on net7-android seems to missing the char for the json respone
                // Seems to be only on a specific simulator, further investigation
#if DEBUG
                if ((json?.StartsWith("{") ?? false) && (!json?.EndsWith("}") ?? false))
                {
                    //json += $"}}"; 
                }
                else if ((json?.StartsWith("[") ?? false) && (!json?.EndsWith("]") ?? false))
                {
                    //json += $"]";
                }
#endif
                json ??= string.Empty;
                return JsonConvert.DeserializeObject<T?>(json, serializerSettings ?? NewtonsoftJsonSerializerSettings);
            }
            catch (JsonSerializationException jexc)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jexc,
                    OriginalString = json,
                    Message = jexc?.Message,
                    TargetType = nameof(T)
                });
                return default;
            }
        }
#nullable disable
        #endregion
    }
}

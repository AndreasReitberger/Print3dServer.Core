/*
using AndreasReitberger.API.Print3dServer.Core.JSON.System;
using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

#if DEBUG
        #region Debug
        [ObservableProperty]
        [property: Newtonsoft.Json.JsonIgnore, JsonIgnore, XmlIgnore]
        JsonSerializerOptions jsonSerializerSettings = DefaultJsonSerializerSettings;

        public new static JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            Converters =
            {                     
                // Map the converters
                new TypeMappingConverter<IAuthenticationHeader, AuthenticationHeader>(),
            }
        };
        #endregion
#else
        #region Release
        public new static JsonSerializerOptions DefaultJsonSerializerSettings = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Converters =
            {                     
                // Map the converters
                new TypeMappingConverter<IAuthenticationHeader, AuthenticationHeader>(),
            }
        };
        #endregion
#endif
    }
}
*/
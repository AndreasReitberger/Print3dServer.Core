using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.Core.Utilities;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        public class Print3dServerConnectionBuilder
        {
            #region Instance
            readonly Print3dServerClient _client = new();
            #endregion

            #region Methods

            public Print3dServerClient Build()
            {
                return _client;
            }

            public Print3dServerConnectionBuilder AsRepetierServer()
            {
                _client.Target = Print3dServerTarget.RepetierServer;
                _client.ApiKeyRegexPattern = RegexHelper.RepetierServerProApiKey;
                _client.Port = 3344;
                _client.WebSocketTarget = "/socket/";
                return this;
            }

            public Print3dServerConnectionBuilder AsOctoPrintServer()
            {
                _client.Target = Print3dServerTarget.OctoPrint;
                _client.ApiKeyRegexPattern = RegexHelper.OctoPrintApiKey;
                _client.Port = 8080;
                return this;
            }

            public Print3dServerConnectionBuilder AsMoonrakerServer()
            {
                _client.Target = Print3dServerTarget.Moonraker;
                _client.Port = 80;
                _client.WebSocketTarget = "/websocket/";
                return this;
            }

            public Print3dServerConnectionBuilder AsPrusaConnectServer()
            {
                _client.Target = Print3dServerTarget.PrusaConnect;
                return this;
            }

            public Print3dServerConnectionBuilder AsCustom(string apiKeyRegexPattern = "")
            {
                _client.Target = Print3dServerTarget.Custom;
                _client.ApiKeyRegexPattern = apiKeyRegexPattern;
                return this;
            }
            
            public Print3dServerConnectionBuilder As(Print3dServerTarget target)
            {
                _client.Target = target;
                return this;
            }

            public Print3dServerConnectionBuilder WithServerAddress(string serverAddress, int port, bool https = false)
            {
                _client.IsSecure = https;
                _client.ServerAddress = serverAddress;
                _client.Port = port;
                return this;
            }

            public Print3dServerConnectionBuilder WithApiKey(string apiKey, Print3dServerTarget target = Print3dServerTarget.Custom)
            {
                _client.ApiKey = apiKey;
                switch (target)
                {
                    case Print3dServerTarget.Moonraker:
                        _client.AuthHeaders.Add("X-Api-Key", new AuthenticationHeader() { Token = apiKey, Order = 0 });
                        //_client.AuthHeaders.Add("Authorization", new AuthenticationHeader() { Order = 1, Format = "Bearer {0}" });
                        break;
                    case Print3dServerTarget.RepetierServer:
                        _client.AuthHeaders.Add("apikey", new AuthenticationHeader() { Token = apiKey, Order = 0 });
                        break;
                    case Print3dServerTarget.OctoPrint:
                        break;
                    case Print3dServerTarget.PrusaConnect:
                        break;
                    case Print3dServerTarget.Custom:
                    default:
                        break;
                }
                return this;
            }

            #endregion
        }
    }
}

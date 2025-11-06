using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.Shared.Core.Utilities;

namespace Print3dServer.Core.Test
{
    public partial class MyOwnPrintServerClient
    {
        public class MyOwnPrintServerConnectionBuilder
        {
            #region Instance
            readonly MyOwnPrintServerClient _client = new();
            #endregion

            #region Methods

            public MyOwnPrintServerClient Build()
            {
                return _client;
            }

            public MyOwnPrintServerConnectionBuilder AsRepetierServer(string targetUri, string apiKey)
            {
                WithServerAddress(targetUri);
                WithApiKey(apiKey, Print3dServerTarget.RepetierServer);
                _client.Target = Print3dServerTarget.RepetierServer;
                _client.ApiKeyRegexPattern = RegexHelper.RepetierServerProApiKey;
                _client.WebCamTarget = "/printer/cammjpg/";
                _client.EnablePing = true;
                _client.PingInterval = 5;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder AsOctoPrintServer(string targetUri, string apiKey)
            {
                WithServerAddress(targetUri);
                WithApiKey(apiKey, Print3dServerTarget.OctoPrint);
                _client.Target = Print3dServerTarget.OctoPrint;
                _client.ApiKeyRegexPattern = RegexHelper.OctoPrintApiKey;
                //_client.Port = 80;
                _client.WebSocketTarget = "sockjs/websocket";
                _client.WebCamTarget = "/webcam/?action=stream";
                return this;
            }

            public MyOwnPrintServerConnectionBuilder AsMoonrakerServer(string targetUri, string apiKey)
            {
                WithServerAddress(targetUri);
                WithApiKey(apiKey, Print3dServerTarget.Moonraker);
                _client.Target = Print3dServerTarget.Moonraker;
                //_client.Port = 80;
                _client.WebSocketTarget = "websocket";
                _client.WebCamTarget = "/webcam/?action=stream";
                return this;
            }

            public MyOwnPrintServerConnectionBuilder AsPrusaConnectServer()
            {
                _client.Target = Print3dServerTarget.PrusaConnect;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder AsCustom(string apiKeyRegexPattern = "")
            {
                _client.Target = Print3dServerTarget.Custom;
                _client.ApiKeyRegexPattern = apiKeyRegexPattern;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder As(Print3dServerTarget target)
            {
                _client.Target = target;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder WithServerAddress(string serverAddress, string version = "")
            {
                _client.ApiTargetPath = serverAddress;
                _client.ApiVersion = version;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder WithApiKey(string apiKey, Print3dServerTarget target = Print3dServerTarget.Custom)
            {
                _client.ApiKey = apiKey;
                switch (target)
                {
                    case Print3dServerTarget.Moonraker:
                        _client.AuthHeaders.Add("X-Api-Key", new AuthenticationHeader() { Token = apiKey, Order = 0, Target = AuthenticationHeaderTarget.Header, Type = AuthenticationTypeTarget.Both });
                        break;
                    case Print3dServerTarget.RepetierServer:
                        _client.AuthHeaders.Add("apikey", new AuthenticationHeader() { Token = apiKey, Order = 0, Target = AuthenticationHeaderTarget.UrlSegment, Type = AuthenticationTypeTarget.Both });
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

            public MyOwnPrintServerConnectionBuilder WithName(string name)
            {
                _client.ServerName = name;
                return this;
            }

            public MyOwnPrintServerConnectionBuilder WithWebSocket(string websocketTarget, string pingCommand = "", int pingInterval = 5, bool enablePing = true)
            {
                _client.WebSocketTargetUri = websocketTarget;
                _client.PingCommand = pingCommand;
                _client.PingInterval = pingInterval;
                _client.EnablePing = enablePing;
                return this;
            }

            #endregion
        }
    }
}

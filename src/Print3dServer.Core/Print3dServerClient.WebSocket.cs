using AndreasReitberger.API.Print3dServer.Core.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Websocket.Client;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        WebsocketClient? webSocket;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long lastPingTimestamp;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        int pingInterval = 60;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long pingCounter = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long lastRefreshTimestamp;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        int refreshCounter = 0;

        [ObservableProperty]
        string pingCommand = string.Empty;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string webSocketTargetUri = string.Empty;

        [ObservableProperty]
        string webSocketTarget = "/socket/";
        partial void OnWebSocketTargetChanged(string value)
        {
            if(value?.StartsWith("/") is true)
            {
                WebSocketTarget = string.Join("", value.Skip(1));
            }
            if(value?.EndsWith("/") is true)
            {
                WebSocketTarget = string.Join("", value.Take(value.Length - 1));
            }
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty, Obsolete("Use IsListening instead")]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isListeningToWebsocket = false;
        partial void OnIsListeningToWebsocketChanged(bool value)
        {
            OnListeningChangedEvent(new ListeningChangedEventArgs()
            {
                SessionId = SessionId,
                IsListening = IsListening,
                IsListeningToWebSocket = value,
            });
        }
        #endregion

        #region Methods

        protected virtual string GetWebSocketTargetUri()
        {
            string webSocketTarget = $"{(IsSecure ? "wss" : "ws")}://{ServerAddress}:{Port}/{WebSocketTarget}";
            switch (Target)
            {
                case Enums.Print3dServerTarget.Moonraker:
                    string oneShotToken = "", apiToken = "";
                    if (AuthHeaders?.ContainsKey("oneshottoken") is true)
                    {
                        oneShotToken = AuthHeaders?["oneshottoken"].Token ?? "";
                    }
                    else if (AuthHeaders?.ContainsKey("session") is true)
                    {
                        oneShotToken = AuthHeaders?["session"].Token ?? "";
                    }
                    if (AuthHeaders?.ContainsKey("apikey") is true)
                    {
                        apiToken = AuthHeaders?["apikey"].Token ?? "";
                    }
                    if (string.IsNullOrEmpty(oneShotToken))
                    {
                        oneShotToken = SessionId;
                    }
                    if (string.IsNullOrEmpty(oneShotToken) && string.IsNullOrEmpty(apiToken))
                        break;

                    webSocketTarget += LoginRequired ?
                        $"?token={oneShotToken}" :
                        $"{(!string.IsNullOrEmpty(oneShotToken) ? $"?token={oneShotToken}" : $"?token={apiToken}")}";
                    break;
                case Enums.Print3dServerTarget.RepetierServer:
                    if (AuthHeaders?.ContainsKey("apikey") is true)
                    {
                        webSocketTarget += $"?apikey={AuthHeaders?["apikey"].Token}";
                    }
                    break;
                case Enums.Print3dServerTarget.OctoPrint:
                    if (AuthHeaders?.ContainsKey("apikey") is true)
                    {
                        webSocketTarget += $"?t={AuthHeaders?["apikey"].Token}";
                    }
                    break;
                case Enums.Print3dServerTarget.PrusaConnect:
                    break;
                case Enums.Print3dServerTarget.Custom:
                    break;
                default:
                    break;
            }
            return webSocketTarget;
        }

        /// <summary>
        /// Source: https://github.com/Z0rdak/RepetierSharp/blob/main/RepetierConnection.cs
        /// </summary>
        /// <returns></returns>
        protected virtual WebsocketClient GetWebSocketClient()
        {
            WebsocketClient client = new(new Uri(GetWebSocketTargetUri()))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(15)
            };
            client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
                    IsListening = true;
                    // Only query messages at this point when using a api-key or no auth
                    /*
                    if (Session.AuthType != AuthenticationType.Credentials)
                    {
                        this.QueryOpenMessages();
                    }
                    */
                }
                Task.Run(async () => await SendPingAsync());
            });
            client.DisconnectionHappened.Subscribe(info => WebSocket_Closed(info));
            client.MessageReceived.Subscribe(msg => WebSocket_MessageReceived(msg));
            return client;
        }

        public virtual Task SendPingAsync() => SendWebSocketCommandAsync(BuildPingCommand());
 
        public virtual Task SendWebSocketCommandAsync(string command) => Task.Run(() => WebSocket?.Send(command));

        public virtual string BuildPingCommand(object? data = null)
        {
            switch (Target)
            {
                case Enums.Print3dServerTarget.Moonraker:
                    // Example: {{\"jsonrpc\":\"2.0\",\"method\":\"server.info\",\"params\":{{}},\"id\":1}}
                    data = new
                    {
                        jsonrpc = "2.0",
                        method = "server.info",
                        @params = new { },
                        id = PingCounter,
                    };
                    break;
                case Enums.Print3dServerTarget.RepetierServer:
                    //Example: $"{{\"action\":\"ping\",\"data\":{{\"source\":\"{"App"}\"}},\"printer\":\"{GetActivePrinterSlug()}\",\"callback_id\":{PingCounter}}}"
                    data = new
                    {
                        action = "ping",
                        /*data = $"{{\"source\":\"{"App"}\"}}",*/                   
                        data = new
                        {
                            source = "App"
                        },                      
                        printer = GetActivePrinterSlug(),
                        callback_id = PingCounter
                    };
                    break;
                case Enums.Print3dServerTarget.OctoPrint:
                case Enums.Print3dServerTarget.PrusaConnect:
                case Enums.Print3dServerTarget.Custom:
                default:
                    data = new { };
                    break;
            }
            return JsonConvert.SerializeObject(data);
        }

        public virtual async Task UpdateWebSocketAsync(Func<Task>? refreshFunction = null, string[]? commandsOnConnect = null)
        {
            if (!string.IsNullOrEmpty(WebSocketTargetUri) && IsInitialized)
            {
                await StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunction: refreshFunction, commandsOnConnect: commandsOnConnect)
                    .ConfigureAwait(false);
            }
        }
        public virtual Task StartListeningAsync(bool stopActiveListening = false, string[]? commandsOnConnect = null)
            => StartListeningAsync(GetWebSocketTargetUri(), stopActiveListening, OnRefresh, commandsOnConnect: commandsOnConnect);

        public virtual async Task StartListeningAsync(string target, bool stopActiveListening = false, Func<Task>? refreshFunction = null, string[]? commandsOnConnect = null)
        {
            if (IsListening)// avoid multiple sessions
            {
                if (stopActiveListening)
                {
                    await StopListeningAsync();
                }
                else
                {
                    return; // StopListening();
                }
            }
            OnRefresh = refreshFunction;
            await ConnectWebSocketAsync(target, commandsOnConnect: commandsOnConnect).ConfigureAwait(false);
            IsListening = true;
        }
       
        public virtual async Task StopListeningAsync()
        {
            CancelCurrentRequests();
            if (IsListening)
            {
                await DisconnectWebSocketAsync().ConfigureAwait(false);
            }
            IsListening = false;
        }

        public virtual Task ConnectWebSocketAsync(string target, string commandOnConnect)
            => ConnectWebSocketAsync(target: target, commandsOnConnect: commandOnConnect is not null ? [commandOnConnect] : null);
        public virtual async Task ConnectWebSocketAsync(string target, string[]? commandsOnConnect = null)
        {
            try
            {
#if NET6_0_OR_GREATER
                bool targetValid = Uri.TryCreate(target, UriKind.Absolute, out var uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeWs || uriResult.Scheme == Uri.UriSchemeWss);
                if (!targetValid) return;
#else
                if (!string.IsNullOrEmpty(target) && Regex.IsMatch(target, @"/^(wss?:\/\/)([0-9]{1,3}(?:\.[0-9]{1,3}){3}|[a-zA-Z]+):([0-9]{1,5})$/"))
                {
                    return;
                }
#endif            
                await DisconnectWebSocketAsync();
                WebSocket = GetWebSocketClient();
                await WebSocket.StartOrFail()
                    .ContinueWith(t => SendPingAsync())
                    ;
                if (commandsOnConnect is not null && WebSocket is not null)
                {
                    // Send command
                    for (int i = 0; i < commandsOnConnect?.Length; i++)
                    {
                        WebSocket.Send(commandsOnConnect[i]);
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        public virtual async Task DisconnectWebSocketAsync()
        {
            try
            {
                if (WebSocket is not null)
                {
                    await Task.Delay(10);
                    WebSocket.Dispose();
                    WebSocket = null;
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        protected virtual void WebSocket_MessageReceived(ResponseMessage? msg)
        {
            try
            {
                if (msg?.MessageType != System.Net.WebSockets.WebSocketMessageType.Text || string.IsNullOrEmpty(msg?.Text))
                {
                    return;
                }
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (LastPingTimestamp + PingInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    PingCounter++;
                    LastPingTimestamp = timestamp;
                    Task.Run(SendPingAsync);
#if DEBUG
                    Debug.WriteLine($"WS-Ping sent from {Target}: {DateTime.Now}");
#endif
                }
                // Handle refreshing more often the pinging
                if (LastRefreshTimestamp + RefreshInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    LastRefreshTimestamp = timestamp;
                    Task.Run(async () =>
                    {
                        // Check online state on each 5. ping
                        if (RefreshCounter > 5)
                        {
                            RefreshCounter = 0;
                            await CheckOnlineAsync(commandBase: CheckOnlineTargetUri, authHeaders: AuthHeaders, timeout: 3500).ConfigureAwait(false);
                        }
                        else RefreshCounter++;
                        if (IsOnline)
                        {
                            if(OnRefresh is not null)
                            {
                                await OnRefresh.Invoke().ConfigureAwait(false);
#if DEBUG
                                Debug.WriteLine($"Data refreshed {Target}: {DateTime.Now} - On refresh done");
#endif
                            }
                        }
                        else if (IsListening)
                        {
                            await StopListeningAsync().ConfigureAwait(false); // StopListening();
                        }
                    });
                }
                if (string.IsNullOrEmpty(SessionId) && msg.Text.ToLower().Contains("session"))
                {
                    JObject? obj = JsonConvert.DeserializeObject<JObject>(msg.Text);
                    switch (Target)
                    {
                        case Enums.Print3dServerTarget.Moonraker:
                            break;
                        case Enums.Print3dServerTarget.RepetierServer:
                            var sessObj = obj?.SelectToken("session");
                            SessionId = sessObj?.Value<string>() ?? "";
                            break;
                        case Enums.Print3dServerTarget.OctoPrint:
                            break;
                        case Enums.Print3dServerTarget.PrusaConnect:
                            break;
                        case Enums.Print3dServerTarget.Custom:
                            break;
                        default:
                            break;
                    }
                    //Session.SessionId = message.SessionId;
                    //OnSessionEstablished?.Invoke(Session.SessionId);
                }/**/
                OnWebSocketMessageReceived(new WebsocketEventArgs()
                {
                    CallbackId = PingCounter,
                    Message = msg.Text,
                    SessionId = SessionId,
                });
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = msg?.Text,
                    Message = jecx.Message,
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        protected virtual void WebSocket_Closed(DisconnectionInfo? info)
        {
            try
            {
                IsListening = false;
                //StopPingTimer();
                OnWebSocketDisconnected(new Print3dBaseEventArgs()
                {
                    Message = 
                    $"WebSocket connection to {WebSocket} closed. Connection state while closing was '{(IsOnline ? "online" : "offline")}'" +
                    $"\n-- Connection closed: {info?.Type} | {info?.CloseStatus} | {info?.CloseStatusDescription}",
                    Printer = GetActivePrinterSlug(),
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        #endregion
    }
}

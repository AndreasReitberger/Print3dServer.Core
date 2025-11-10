using AndreasReitberger.API.REST.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;
using WebsocketEventArgs = AndreasReitberger.API.REST.Events.WebsocketEventArgs;


#if DEBUG
using System.Diagnostics;
#endif

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties
        
        /*
        [ObservableProperty, Obsolete("Set the `WebSocketTargetUri` instead to the full path")]
        public partial string WebSocketTarget { get; set; } = "/socket/";
        partial void OnWebSocketTargetChanged(string value)
        {
            if (value?.StartsWith("/") is true)
            {
                WebSocketTarget = string.Join("", value.Skip(1));
            }
            if (value?.EndsWith("/") is true)
            {
                WebSocketTarget = string.Join("", value.Take(value.Length - 1));
            }
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }
        */
        #endregion

        #region Methods

        protected virtual string GetWebSocketTargetUri()
        {
            //string webSocketTarget = $"{WebSocketTargetUri}/{WebSocketTarget}";
            string webSocketTarget = $"{WebSocketTargetUri}";
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

        public override string BuildPingCommand(object? data = null)
        {
            data = Target switch
            {
                // Example: {{\"jsonrpc\":\"2.0\",\"method\":\"server.info\",\"params\":{{}},\"id\":1}}
                Enums.Print3dServerTarget.Moonraker => new
                {
                    jsonrpc = "2.0",
                    method = "server.info",
                    @params = new { },
                    id = PingCounter,
                },
                //Example: $"{{\"action\":\"ping\",\"data\":{{\"source\":\"{"App"}\"}},\"printer\":\"{GetActivePrinterSlug()}\",\"callback_id\":{PingCounter}}}"
                Enums.Print3dServerTarget.RepetierServer => new
                {
                    action = "ping",
                    /*data = $"{{\"source\":\"{"App"}\"}}",*/
                    data = new
                    {
                        source = "App"
                    },
                    printer = GetActivePrinterSlug(),
                    callback_id = PingCounter
                },
                _ => new { },
            };
            return JsonConvert.SerializeObject(data);
        }
        
        protected override void WebSocket_MessageReceived(ResponseMessage? msg)
        {
            try
            {
                if (msg?.MessageType != System.Net.WebSockets.WebSocketMessageType.Text || string.IsNullOrEmpty(msg?.Text))
                {
                    return;
                }
                long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                /* Pinging happens in a separate task now
                if (LastPingTimestamp + PingInterval < DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    PingCounter++;
                    LastPingTimestamp = timestamp;
                    Task.Run(async() => SendPingAsync().ConfigureAwait(false));
#if DEBUG
                    Debug.WriteLine($"WS-Ping sent from {Target}: {DateTime.Now}");
#endif
                }
                */
                // Handle refreshing more often the pinging
                if (LastRefreshTimestamp + RefreshInterval < timestamp)
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
                            if (OnRefresh is not null)
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
                if (string.IsNullOrEmpty(SessionId) && msg.Text.Contains("session", StringComparison.CurrentCultureIgnoreCase))
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
        #endregion
    }
}

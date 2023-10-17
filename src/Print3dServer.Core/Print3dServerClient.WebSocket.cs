using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.Core.Enums;
using AndreasReitberger.Core.Utilities;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;

#if NET_WS
using WebSocket4Net;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;
#else
using Websocket.Client;
#endif

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
#if NET_WS
        WebSocket? webSocket;
#else
        WebsocketClient? webSocket;
#endif

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        Timer pingTimer;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        long lastPingTimestamp;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int pingInterval = 10;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int pingCounter = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int refreshCounter = 0;

        [ObservableProperty]
        string pingCommand;

        [ObservableProperty]
        string webSocketTargetUri;

        [ObservableProperty]
        string webSocketTarget = "/socket/";
        partial void OnWebSocketTargetChanged(string value)
        {
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListeningToWebsocket)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isListeningToWebsocket = false;
        partial void OnIsListeningToWebsocketChanged(bool value)
        {
            OnListeningChangedEvent(new ListeningChangedEventArgs()
            {
                SessonId = SessionId,
                IsListening = IsListening,
                IsListeningToWebSocket = value,
            });
        }
#endregion

        #region Methods

        protected string GetWebSocketTargetUri()
        {
            string webSocketTarget = $"{(IsSecure ? "wss" : "ws")}://{ServerAddress}:{Port}/{WebSocketTarget}";
            switch (Target)
            {
                case Enums.Print3dServerTarget.Moonraker:
                    break;
                case Enums.Print3dServerTarget.RepetierServer:
                    if (AuthHeaders?.ContainsKey("apikey") is true)
                    {
                        webSocketTarget += $"?apikey={AuthHeaders?["apikey"].Token}";
                    }
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
            return webSocketTarget;
        }
        void StopPingTimer()
        {
            if (PingTimer != null)
            {
                try
                {
                    PingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    PingTimer = null;
                    IsListeningToWebsocket = false;
                }
                catch (ObjectDisposedException)
                {
                    //PingTimer = null;
                }
            }
        }

#if !NET_WS
        /// <summary>
        /// Source: https://github.com/Z0rdak/RepetierSharp/blob/main/RepetierConnection.cs
        /// </summary>
        /// <returns></returns>
        WebsocketClient GetWebSocketClient()
        {
            WebsocketClient client = new(new Uri(GetWebSocketTargetUri()))
            {
                ReconnectTimeout = TimeSpan.FromSeconds(15)
            };
            client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                {
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

        async Task SendPingAsync()
        {
            await Task.Run(() => WebSocket?.Send(BuildPingCommand()));
        }
#endif


        public string BuildPingCommand(object? data = null)
        {
            switch (Target)
            {
                case Enums.Print3dServerTarget.Moonraker:
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
                    break;
                case Enums.Print3dServerTarget.PrusaConnect:
                    break;
                case Enums.Print3dServerTarget.Custom:
                    break;
                default:
                    break;
            }
#if false
            return $"{{\"action\":\"ping\",\"data\":{{\"source\":\"{"App"}\"}},\"printer\":\"{GetActivePrinterSlug()}\",\"callback_id\":{PingCounter}}}";
#else
            return JsonConvert.SerializeObject(data);
#endif
        }

#if NET_WS
        public void PingServer(string? pingCommand = null)
        {
            try
            {
                if (WebSocket != null)
                    if (WebSocket.State == WebSocketState.Open)
                    {
                        pingCommand ??= PingCommand;
                        if(pingCommand is not null)
                            WebSocket.Send(pingCommand);
                    }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }

        }

        public void PingServerWithObject(object? pingCommand = null)
        {
            try
            {
                if (WebSocket != null)
                    if (WebSocket.State == WebSocketState.Open)
                    {
                        string cmd = BuildPingCommand(pingCommand);
                        if(pingCommand is not null)
                            WebSocket.Send(cmd);
                    }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }

        }
#endif

        public async Task UpdateWebSocketAsync(List<Task>? refreshFunctions = null)
        {
            if (!string.IsNullOrEmpty(WebSocketTargetUri))
            {
                await StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunctions: refreshFunctions)
                    .ConfigureAwait(false);
            }
        }

        public async Task StartListeningAsync(string target, bool stopActiveListening = false, List<Task>? refreshFunctions = null)
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
            await ConnectWebSocketAsync(target).ConfigureAwait(false);
            Timer = new Timer(async (action) =>
            {
                // Do not check the online state ever tick
                if (RefreshCounter > 5)
                {
                    RefreshCounter = 0;
                    await CheckOnlineAsync(commandBase: CheckOnlineTargetUri, authHeaders: AuthHeaders, timeout: 3500).ConfigureAwait(false);
                }
                else RefreshCounter++;
                if (IsOnline)
                {
                    if(refreshFunctions?.Count > 0)
                        await Task.WhenAll(refreshFunctions).ConfigureAwait(false);
                }
                else if (IsListening)
                {
                    await StopListeningAsync(); // StopListening();
                }
            }, null, 0, RefreshInterval * 1000);
            IsListening = true;
        }
        public async Task StopListeningAsync()
        {
            CancelCurrentRequests();
#if NET_WS
            StopPingTimer();
#endif
            StopTimer();

            if (IsListeningToWebsocket)
            {
                await DisconnectWebSocketAsync().ConfigureAwait(false);
            }
            IsListening = false;
        }

        public async Task ConnectWebSocketAsync(string target)
        {
            try
            {
                if (!string.IsNullOrEmpty(FullWebAddress) && (
                    Regex.IsMatch(FullWebAddress, RegexHelper.IPv4AddressRegex) ||
                    Regex.IsMatch(FullWebAddress, RegexHelper.IPv6AddressRegex) ||
                    Regex.IsMatch(FullWebAddress, RegexHelper.Fqdn)))
                {
                    return;
                }
                await DisconnectWebSocketAsync();
                //string target = $"{(IsSecure ? "wss" : "ws")}://{ServerAddress}:{Port}/socket/{(!string.IsNullOrEmpty(ApiKey) ? $"?apikey={ApiKey}" : "")}";
#if NET_WS
                WebSocket = new WebSocket(target)
                {
                    EnableAutoSendPing = true,
                    AutoSendPingInterval = 0,
                };

                if (IsSecure)
                {
                    // https://github.com/sta/websocket-sharp/issues/219#issuecomment-453535816
                    SslProtocols sslProtocolHack = (SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
                    //Avoid TlsHandshakeFailure
                    if (WebSocket.Security.EnabledSslProtocols != sslProtocolHack)
                    {
                        WebSocket.Security.EnabledSslProtocols = sslProtocolHack;
                    }
                }

                WebSocket.MessageReceived += WebSocket_MessageReceived;
                WebSocket.DataReceived += WebSocket_DataReceived;
                WebSocket.Opened += WebSocket_Opened;
                WebSocket.Closed += WebSocket_Closed;
                WebSocket.Error += WebSocket_Error;

                await WebSocket.OpenAsync();
#else
                WebSocket = GetWebSocketClient();
                await WebSocket.StartOrFail()
                    .ContinueWith(t => SendPingAsync());
#endif
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        public async Task DisconnectWebSocketAsync()
        {
            try
            {
                if (WebSocket != null)
                {
#if NET_WS
                    if (WebSocket.State == WebSocketState.Open)
                        await WebSocket.CloseAsync();
                    StopPingTimer();

                    WebSocket.MessageReceived -= WebSocket_MessageReceived;
                    WebSocket.DataReceived -= WebSocket_DataReceived;
                    WebSocket.Opened -= WebSocket_Opened;
                    WebSocket.Closed -= WebSocket_Closed;
                    WebSocket.Error -= WebSocket_Error;
#else
                    await Task.Delay(10);
                    WebSocket.Dispose();
#endif
                    WebSocket = null;
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

#if NET_WS
        protected void WebSocket_Error(object? sender, ErrorEventArgs e)
        {
            try
            {
                IsListeningToWebsocket = false;
                OnWebSocketError(e);
                OnError(e);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            finally
            {
                _ =  DisconnectWebSocketAsync();
            }
        }
#endif

#if NET_WS

        protected void WebSocket_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            try
            {
                if (e.Message == null || string.IsNullOrEmpty(e.Message))
                    return;
                OnWebSocketMessageReceived(new WebsocketEventArgs()
                {
                    CallbackId = PingCounter,
                    Message = e.Message,
                    SessonId = SessionId,
                });
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = e.Message,
                    Message = jecx.Message,
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
#else
        protected void WebSocket_MessageReceived(ResponseMessage? msg)
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
                    LastPingTimestamp = timestamp;
                    Task.Run(async () => await SendPingAsync());
                }
                OnWebSocketMessageReceived(new WebsocketEventArgs()
                {
                    CallbackId = PingCounter,
                    Message = msg.Text,
                    SessonId = SessionId,
                });
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = msg.Text,
                    Message = jecx.Message,
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
#endif

#if NET_WS
        protected void WebSocket_Closed(object? sender, EventArgs e)
        {
            try
            {
                IsListeningToWebsocket = false;
                StopPingTimer();
                OnWebSocketDisconnected(new Print3dBaseEventArgs()
                {
                    Message = $"WebSocket connection to {WebSocket} closed. Connection state while closing was '{(IsOnline ? "online" : "offline")}'",
                    Printer = GetActivePrinterSlug(),
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
#else
        protected void WebSocket_Closed(DisconnectionInfo? info)
        {
            try
            {
                IsListeningToWebsocket = false;
                StopPingTimer();
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
#endif

#if NET_WS
        protected void WebSocket_Opened(object? sender, EventArgs e)
        {
            try
            {
                // Trigger ping to get session id
                //WebSocket?.Send(BuildPingCommand());

                //PingTimer = new Timer((action) => PingServer(PingCommand), null, 0, 2500);
                //PingTimer = new Timer((action) => PingServerWithObject(), null, 0, 2500);

                IsListeningToWebsocket = true;
                OnWebSocketConnected(new Print3dBaseEventArgs()
                {
                    Message = $"WebSocket connection to {WebSocket} established. Connection state while opening was '{(IsOnline ? "online" : "offline")}'",
                    Printer = GetActivePrinterSlug(),
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

        protected void WebSocket_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                OnWebSocketDataReceived(new WebsocketEventArgs()
                {
                    CallbackId = PingCounter,
                    Data = e.Data,
                    SessonId = SessionId,
                });
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
#endif

#endregion

    }
}

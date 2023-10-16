using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.Core.Enums;
using AndreasReitberger.Core.Utilities;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using WebSocket4Net;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        WebSocket? webSocket;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        Timer pingTimer;

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

        protected void PingServer(string? pingCommand = null)
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
            StopPingTimer();
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
                WebSocket = new WebSocket(target)
                {
                    EnableAutoSendPing = false,
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
                    if (WebSocket.State == WebSocketState.Open)
                        await WebSocket.CloseAsync();
                    StopPingTimer();

                    WebSocket.MessageReceived -= WebSocket_MessageReceived;
                    WebSocket.DataReceived -= WebSocket_DataReceived;
                    WebSocket.Opened -= WebSocket_Opened;
                    WebSocket.Closed -= WebSocket_Closed;
                    WebSocket.Error -= WebSocket_Error;

                    WebSocket = null;
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }

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

        protected void WebSocket_Opened(object? sender, EventArgs e)
        {
            try
            {
                // Trigger ping to get session id
                WebSocket?.Send(PingCommand);

                PingTimer = new Timer((action) => PingServer(PingCommand), null, 0, 2500);

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

        void WebSocket_DataReceived(object? sender, DataReceivedEventArgs e)
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

        #endregion

    }
}

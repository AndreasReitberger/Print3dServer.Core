using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Exceptions;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.Core.Utilities;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public abstract partial class Print3dServerClient : ObservableObject, IPrint3dServerClient
    {
        #region Variables
        protected RestClient? restClient;
        protected HttpClient? httpClient;
        protected int _retries = 0;
        #endregion

        #region Properties

        #region Base
        [ObservableProperty]
        Guid id = Guid.Empty;

        [ObservableProperty]
        Print3dServerTarget target = Print3dServerTarget.Custom;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        Func<Task>? onRefresh;
        #endregion

        #region Instance
   
        public static IPrint3dServerClient? Instance;

        [ObservableProperty]
        bool isActive = false;

        [ObservableProperty]
        bool updateInstance = false;
        partial void OnUpdateInstanceChanged(bool value)
        {
            if (value)
            {
                InitInstance(ServerAddress, Port, ApiKey, IsSecure);
            }
        }

        [ObservableProperty]
        bool isInitialized = false;

        #endregion

        #region RefreshTimer
        [ObservableProperty, Obsolete("Try to replace with WebSocket pinging")]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        Timer? timer;

        [ObservableProperty]
        int refreshInterval = 5;
        partial void OnRefreshIntervalChanged(int value)
        {
            if (IsListening)
            {
                _ = StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunction: OnRefresh);
            }
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isListening = false;
        partial void OnIsListeningChanged(bool value)
        {
            OnListeningChangedEvent(new ListeningChangedEventArgs()
            {
                SessionId = SessionId,
                IsListening = value,
                IsListeningToWebSocket = value,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool initialDataFetched = false;

        #endregion

        #region Debug
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<string, string> ignoredJsonResults = [];
        partial void OnIgnoredJsonResultsChanged(ConcurrentDictionary<string, string> value)
        {
            OnIgnoredJsonResultsChanged(new IgnoredJsonResultsChangedEventArgs()
            {
                NewIgnoredJsonResults = value,
            });
        }
        #endregion

        #region Api

        [ObservableProperty]
        string apiVersion = string.Empty;

        [ObservableProperty]
        string apiTargetPath = string.Empty;
        #endregion

        #region Connection

        [ObservableProperty]
        [property: XmlIgnore]
        Dictionary<string, IAuthenticationHeader> authHeaders = [];

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string sessionId = string.Empty;
        partial void OnSessionIdChanged(string value)
        {
            AddOrUpdateAuthHeader("session", value);
            OnSessionChangedEvent(new()
            {
                SessionId = value,
                Session = value,
                AuthToken = ApiKey,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        string serverName = string.Empty;

        [ObservableProperty]
        string checkOnlineTargetUri = string.Empty;

        [ObservableProperty]
        string serverAddress = string.Empty;
        partial void OnServerAddressChanged(string value)
        {
            UpdateRestClientInstance();
            _ = UpdateWebSocketAsync();
        }

        [ObservableProperty]
        bool loginRequired = false;

        [ObservableProperty]
        bool isSecure = false;
        partial void OnIsSecureChanged(bool value)
        {
            UpdateRestClientInstance();
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty]
        string apiKey = string.Empty;
        partial void OnApiKeyChanged(string value)
        {
            switch (Target)
            {
                case Print3dServerTarget.Custom:
                    break;
                case Print3dServerTarget.PrusaConnect:
                case Print3dServerTarget.OctoPrint:
                case Print3dServerTarget.Moonraker:
                case Print3dServerTarget.RepetierServer:
                default:
                    AddOrUpdateAuthHeader("apikey", value);
                    break;
            }
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty]
        string apiKeyRegexPattern = string.Empty;

        [ObservableProperty]
        int port = 3344;
        partial void OnPortChanged(int value)
        {
            UpdateRestClientInstance();
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty]
        int defaultTimeout = 10000;

        [ObservableProperty]
        bool overrideValidationRules = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isOnline = false;
        partial void OnIsOnlineChanged(bool value)
        {
            if (value)
            {
                OnServerWentOnline(new Print3dBaseEventArgs()
                {
                    SessionId = SessionId,
                });
            }
            else
            {
                OnServerWentOffline(new Print3dBaseEventArgs()
                {
                    SessionId = SessionId,
                });
            }
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isConnecting = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool authenticationFailed = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isRefreshing = false;

        [ObservableProperty]
        int retriesWhenOffline = 2;

        #endregion

        #region Update

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool updateAvailable = false;
        partial void OnUpdateAvailableChanged(bool value)
        {
            if (value)
            {
                OnServerUpdateAvailable(new Print3dBaseEventArgs()
                {
                    SessionId = SessionId,
                });
            }
        }
        
        #endregion

        #region DiskSpace
        [ObservableProperty]
        long freeDiskSpace = 0;

        [ObservableProperty]
        long usedDiskSpace = 0;

        [ObservableProperty]
        long totalDiskSpace = 0;

        #endregion

        #region PrinterStateInformation

        #region ConfigurationInfo

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool shutdownAfterPrint = false;
        
        #endregion

        #region PrinterState
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isPrinting = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isPaused = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isConnectedPrinterOnline = false;

        #endregion

        #region Temperatures

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double temperatureExtruderMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double temperatureExtruderSecondary = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double temperatureHeatedBedMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double temperatureHeatedChamberMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double speedFactor = 100;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double speedFactorTarget = 100;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double flowFactor = 100;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double flowFactorTarget = 100;

        #endregion

        #region Fans
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        int speedFanMain = 0;

        #endregion

        #region Printers
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IPrinter3d? activePrinter;
        partial void OnActivePrinterChanging(IPrinter3d? value)
        {
            OnActivePrinterChangedEvent(new ActivePrinterChangedEventArgs()
            {
                SessionId = SessionId,
                NewPrinter = value,
                OldPrinter = ActivePrinter,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrinter3d> printers = [];
        partial void OnPrintersChanged(ObservableCollection<IPrinter3d> value)
        {
            if (value?.Count > 0 && ActivePrinter == null)
            {
                ActivePrinter = value.FirstOrDefault();
            }
            OnRemotePrintersChanged(new PrintersChangedEventArgs()
            {
                SessionId = SessionId,
                NewPrinters = value ?? [],
                Printer = GetActivePrinterSlug(),
                AuthToken = ApiKey,
            });
        }

        #endregion

        #region Files
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IGcodeGroup> groups = [];
        partial void OnGroupsChanged(ObservableCollection<IGcodeGroup> value)
        {
            OnGcodeGroupsChangedEvent(new GcodeGroupsChangedEventArgs()
            {
                NewModelGroups = value,
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IGcode> files = [];
        partial void OnFilesChanged(ObservableCollection<IGcode> value)
        {
            OnGcodesChangedEvent(new GcodesChangedEventArgs()
            {
                NewModels = value,
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        #endregion

        #region Jobs
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        byte[] currentPrintImage = [];
        partial void OnCurrentPrintImageChanging(byte[] value)
        {
            OnActivePrintImageChanged(new ActivePrintImageChangedEventArgs()
            {
                NewImage = value,
                PreviousImage = CurrentPrintImage,
                SessionId = SessionId,
                CallbackId = -1,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrint3dJob> jobs = [];
        partial void OnJobsChanged(ObservableCollection<IPrint3dJob> value)
        {
            OnJobListChangedEvent(new JobListChangedEventArgs()
            {
                NewJobList = value,
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IPrint3dJobStatus? activeJob;
        partial void OnActiveJobChanging(IPrint3dJobStatus? oldValue, IPrint3dJobStatus? newValue)
        {
            OnJobStatusChangedEvent(new JobStatusChangedEventArgs()
            {
                NewJobStatus = newValue,
                PreviousJobStatus = oldValue,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrint3dJobStatus> activeJobs = [];

        #endregion

        #region Position

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double x = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double y = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        double z = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long layer = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long layers = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool yHomed = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool zHomed = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool xHomed = false;

        #endregion

        #endregion

        #region ReadOnly
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public string FullWebAddress => $"{(IsSecure ? "https" : "http")}://{ServerAddress}:{Port}";

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public bool IsReady
        {
            get
            {
                return
                    !string.IsNullOrEmpty(ServerAddress) && Port > 0 && //  !string.IsNullOrEmpty(API)) &&
                    (
                        // Address
                        (Regex.IsMatch(ServerAddress, RegexHelper.IPv4AddressRegex) || Regex.IsMatch(ServerAddress, RegexHelper.IPv6AddressRegex) || Regex.IsMatch(ServerAddress, RegexHelper.Fqdn)) &&
                        // API-Key (also allow empty key if the user performs a login instead
                        (string.IsNullOrEmpty(ApiKey) || Regex.IsMatch(ApiKey, ApiKeyRegexPattern))
                    ||
                        // Or validation rules are overriden
                        OverrideValidationRules
                    );
            }
        }
        #endregion

        #endregion

        #region Ctor
        public Print3dServerClient()
        {
            Id = Guid.NewGuid();
            UpdateRestClientInstance();
        }

        public Print3dServerClient(string serverAddress, string api, int port, bool isSecure = false)
        {
            Id = Guid.NewGuid();
            InitInstance(serverAddress, port, api, isSecure);
            UpdateRestClientInstance();
        }

        public Print3dServerClient(string serverAddress, int port, bool isSecure = false)
        {
            Id = Guid.NewGuid();
            InitInstance(serverAddress, port, "", isSecure);
            UpdateRestClientInstance();
        }
        #endregion

        #region Dtor
        ~Print3dServerClient()
        {
            
        }
        #endregion

        #region Methods

        #region Private

        #region ValidateActivePrinter
        protected virtual string GetActivePrinterSlug()
        {
            try
            {
                if (!IsReady || ActivePrinter == null)
                {
                    return string.Empty;
                }
                return ActivePrinter?.Slug ?? string.Empty;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return string.Empty;
            }
        }
        protected virtual bool IsPrinterSlugSelected(string PrinterSlug)
        {
            try
            {
                return PrinterSlug == GetActivePrinterSlug();
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #endregion

        #region Public

        #region Init
        public virtual void InitInstance()
        {
            try
            {
                Instance = this;
                if (Instance != null)
                {
                    Instance.UpdateInstance = false;
                    Instance.IsInitialized = true;
                }
                UpdateInstance = false;
                IsInitialized = true;
            }
            catch (Exception exc)
            {
                //UpdateInstance = true;
                OnError(new UnhandledExceptionEventArgs(exc, false));
                IsInitialized = false;
            }
        }
        public static void UpdateSingleInstance(Print3dServerClient Inst)
        {
            try
            {
                Instance = Inst;
            }
            catch (Exception)
            {
                //OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        public virtual void InitInstance(string serverAddress, int port, string api = "", bool isSecure = false)
        {
            try
            {
                ServerAddress = serverAddress;
                ApiKey = api;
                Port = port;
                IsSecure = isSecure;

                Instance = this;
                if (Instance != null)
                {
                    Instance.UpdateInstance = false;
                    Instance.IsInitialized = true;
                }
                UpdateInstance = false;
                IsInitialized = true;
            }
            catch (Exception exc)
            {
                //UpdateInstance = true;
                OnError(new UnhandledExceptionEventArgs(exc, false));
                IsInitialized = false;
            }
        }
        #endregion

        #region Online Check
        public virtual async Task CheckOnlineAsync(int timeout = 10000)
        {
            CancellationTokenSource cts = new(timeout);
            await CheckOnlineAsync(FullWebAddress, AuthHeaders, "", cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public virtual async Task CheckOnlineAsync(string commandBase,Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            CancellationTokenSource cts = new(timeout);
            await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource? cts = default)
        {
            if (IsConnecting) return; // Avoid multiple calls
            IsConnecting = true;
            bool isReachable = false;
            try
            {
                string uriString = FullWebAddress;
                try
                {
                    // Send a blank api request in order to check if the server is reachable
                    IRestApiRequestRespone? respone = await SendRestApiRequestAsync(
                       requestTargetUri: commandBase, 
                       method: Method.Get,
                       command: command,
                       jsonObject: null,
                       authHeaders: authHeaders,
                       cts: cts)
                    .ConfigureAwait(false);
                    /*
                    IRestApiRequestRespone? respone = await SendOnlineCheckRestApiRequestAsync(
                       commandBase, command,
                       authHeaders: authHeaders,
                       cts: cts)
                    .ConfigureAwait(false);
                    */

                    isReachable = respone?.IsOnline == true;
                }
                catch (InvalidOperationException iexc)
                {
                    OnError(new UnhandledExceptionEventArgs(iexc, false));
                }
                catch (HttpRequestException rexc)
                {
                    OnError(new UnhandledExceptionEventArgs(rexc, false));
                }
                catch (TaskCanceledException)
                {
                    // Throws an exception on timeout, not actually an error
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            IsConnecting = false;
            // Avoid offline message for short connection loss
            if (!IsOnline || isReachable || _retries > RetriesWhenOffline)
            {
                // Do not check if the previous state was already offline
                _retries = 0;
                IsOnline = isReachable;
            }
            else
            {
                // Retry with shorter timeout to see if the connection loss is real
                _retries++;
                cts = new(3500);
                await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            }
        }

        public virtual async Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            try
            {
                if (IsOnline)
                {
                    RestApiRequestRespone? respone = await SendRestApiRequestAsync(
                        requestTargetUri: commandBase,
                        method: Method.Get,
                        command: command,
                        authHeaders: authHeaders,
                        cts: new(timeout))
                        .ConfigureAwait(false) as RestApiRequestRespone;
                    if (respone?.HasAuthenticationError is true)
                    {
                        AuthenticationFailed = true;
                        if (respone.EventArgs is RestEventArgs rArgs)
                            OnRestApiAuthenticationError(rArgs);
                    }
                    else
                    {
                        AuthenticationFailed = false;
                        if (respone?.EventArgs is RestEventArgs rArgs)
                            OnRestApiAuthenticationSucceeded(rArgs);
                    }
                    return AuthenticationFailed;
                }
                else
                    return false;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        
        public virtual async Task<bool> SendGcodeAsync(string command = "send", object? data = null, string? targetUri = null)
        {
            try
            {
                data ??= new
                {
                    cmd = "",
                };
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "continueJob",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Refreshing
        public virtual async Task RefreshAllAsync()
        {
            try
            {
                if (!IsOnline) throw new ServerNotReachableException($"The server '{ServerName} ({FullWebAddress})' is not reachable. Make sure to call `CheckOnlineAsync()` first! ");
                // Avoid multiple calls
                if (IsRefreshing) return;
                IsRefreshing = true;
                if (OnRefresh is not null)
                    await OnRefresh.Invoke();
                //await Task.WhenAll(RefreshTasks).ConfigureAwait(false);
                if (!InitialDataFetched)
                    InitialDataFetched = true;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            IsRefreshing = false;
        }
        #endregion

        #region Misc

        public virtual void AddOrUpdateAuthHeader(string key, string value, int order = 0)
        {
            if (AuthHeaders?.ContainsKey(key) is true)
            {
                AuthHeaders[key] = new AuthenticationHeader() { Token = value, Order = order };
            }
            else
            {
                AuthHeaders?.Add(key, new AuthenticationHeader() { Token = value, Order = order });
            }
        }

        public virtual IAuthenticationHeader? GetAuthHeader(string key)
        {
            if (AuthHeaders?.ContainsKey(key) is true)
            {
                return AuthHeaders?[key]; 
            }
            return null;
        }

        public virtual void CancelCurrentRequests()
        {
            try
            {
                if (httpClient != null)
                {
                    httpClient.CancelPendingRequests();
                    UpdateRestClientInstance();
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
        }
        #endregion

        #region Toolheads
        public virtual async Task<bool> HomeAsync(bool x, bool y, bool z, string? targetUri = null)
        {
            try
            {
                bool result;
                if (x && y && z)
                {
                    result = await SendGcodeAsync(command: "send", new { cmd = "G28" }, targetUri: targetUri).ConfigureAwait(false);
                }
                else
                {
                    string cmd = string.Format("G28{0}{1}{2}", x ? " X0 " : "", y ? " Y0 " : "", z ? " Z0 " : "");
                    result = await SendGcodeAsync(command: "send", new { cmd = cmd }, targetUri: targetUri).ConfigureAwait(false);
                }
                return result;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return false;
        }

        public virtual async Task<bool> SetExtruderTemperatureAsync(string command = "setExtruderTemperature", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "setExtruderTemperature",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> SetBedTemperatureAsync(string command = "setBedTemperature", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "setBedTemperature",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> SetChamberTemperatureAsync(string command = "setChamberTemperature", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "setChamberTemperature",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Jobs

        public virtual async Task<bool> StartJobAsync(IPrint3dJob job, string command = "startJob", object? data = null, string? targetUri = null)
        {
            try
            {
                // "{{\"id\":{0}}}", id
                data ??= new 
                {
                    id = job.JobId,
                };
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath, 
                        method: Method.Post, 
                        command: command ?? "continueJob", 
                        authHeaders: AuthHeaders, 
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> RemoveJobAsync(IPrint3dJob job, string command = "removeJob", object? data = null, string? targetUri = null)
        {
            try
            {
                // "{{\"id\":{0}}}", id
                data ??= new
                {
                    id = job.JobId,
                };
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "removeJob",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> ContinueJobAsync(string command = "continueJob", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "continueJob",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> PauseJobAsync(string command = "pauseJob", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "pauseJob",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public virtual async Task<bool> StopJobAsync(string command = "continueJob", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "stopJob",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Printer Control
        /// <summary>
        /// Override this method
        /// </summary>
        /// <returns></returns>
        public abstract Task<ObservableCollection<IPrinter3d>> GetPrintersAsync();
        public virtual async Task SetPrinterActiveAsync(string slug, bool refreshPrinterList = true)
        {
            if (refreshPrinterList)
            {
                Printers = await GetPrintersAsync().ConfigureAwait(false);
            }
            IPrinter3d? printer = Printers?.FirstOrDefault(prt => prt?.Slug == slug);
            if (printer is not null)
                ActivePrinter = printer;
            else 
                ActivePrinter = Printers?.FirstOrDefault(printer => printer.IsOnline); 
        }
        public virtual async Task SetPrinterActiveAsync(int index = -1, bool refreshPrinterList = true)
        {
            if (refreshPrinterList)
            {
                Printers = await GetPrintersAsync().ConfigureAwait(false);
            }
            if (Printers?.Count > index && index >= 0)
            {
                ActivePrinter = Printers[index];
            }
            else
            {
                // If no index is provided, or it's out of bound, the first online printer is used
                ActivePrinter = Printers?.FirstOrDefault(printer => printer.IsOnline);
                // If no online printers is found, however there is at least one printer configured, use this one
                if (ActivePrinter is null && Printers?.Count > 0)
                {
                    ActivePrinter = Printers[0];
                }
            }
        }
        public virtual async Task<bool> SetFanSpeedAsync(string command = "setFanSpeed", object? data = null, string? targetUri = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: targetUri ?? ApiTargetPath,
                        method: Method.Post,
                        command: command ?? "setFanSpeed",
                        authHeaders: AuthHeaders,
                        jsonObject: data)
                    .ConfigureAwait(false);
                return GetQueryResult(result?.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Files

        /// <summary>
        /// Override this method
        /// </summary>
        /// <returns></returns>
        public abstract Task<ObservableCollection<IGcode>> GetFilesAsync();

        /// <summary>
        /// Override this method
        /// </summary>
        /// <returns></returns>
        //public abstract Task<IRestApiRequestRespone?> DeleteFileAsync(string filePath);
        /// <summary>
        /// Override this method
        /// </summary>
        /// <returns></returns>
        //public abstract Task<IRestApiRequestRespone?> UploadFileAsync(string filePath);

        /// <summary>
        /// Override this method
        /// </summary>
        /// <returns></returns>
        public abstract Task<byte[]?> DownloadFileAsync(string filePath);
        #endregion

        #endregion

        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public override bool Equals(object? obj)
        {
            if (obj is not Print3dServerClient item)
                return false;
            return Id.Equals(item.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            // Ordinarily, we release unmanaged resources here;
            // but all are wrapped by safe handles.

            // Release disposable objects.
            if (disposing)
            {
                _ = StopListeningAsync();
                _ = DisconnectWebSocketAsync();
            }
        }
        #endregion

        #region Clone

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}

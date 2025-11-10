using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Exceptions;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Enums;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public abstract partial class Print3dServerClient : RestApiClient, IPrint3dServerClient
    {
        #region Properties

        #region Base

        [ObservableProperty]
        public partial Print3dServerTarget Target { get; set; } = Print3dServerTarget.Custom;

        #endregion

        #region Instance

        public new static IPrint3dServerClient? Instance { get; private set; }

        #endregion

        #region RefreshTimer
      
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool InitialDataFetched { get; set; } = false;

        #endregion

        #region Debug
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<string, string> IgnoredJsonResults { get; set; } = [];
        partial void OnIgnoredJsonResultsChanged(ConcurrentDictionary<string, string> value)
        {
            OnIgnoredJsonResultsChanged(new IgnoredJsonResultsChangedEventArgs()
            {
                NewIgnoredJsonResults = value,
            });
        }
        #endregion

        #region Connection

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public new partial string SessionId { get; set; } = string.Empty;
        partial void OnSessionIdChanged(string value)
        {
            switch (Target)
            {
                case Print3dServerTarget.Moonraker:
                    AddOrUpdateAuthHeader("Authorization", value, AuthenticationHeaderTarget.Header);
                    break;
                case Print3dServerTarget.RepetierServer:
                    AddOrUpdateAuthHeader("sess", value, AuthenticationHeaderTarget.UrlSegment);
                    break;
                case Print3dServerTarget.OctoPrint:
                case Print3dServerTarget.PrusaConnect:
                case Print3dServerTarget.Custom:
                default:
                    AddOrUpdateAuthHeader("session", value, AuthenticationHeaderTarget.Header);
                    break;
            }
            OnSessionChangedEvent(new()
            {
                SessionId = value,
                Session = value,
                AuthToken = ApiKey,
                Message = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        public partial string ServerName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CheckOnlineTargetUri { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool LoginRequired { get; set; } = false;

        [ObservableProperty]
        public partial string ApiKey { get; set; } = string.Empty;
        partial void OnApiKeyChanged(string value)
        {
            // Octoprint: https://docs.octoprint.org/en/master/api/general.html#authorization
            // Repetier Server: https://www.repetier-server.com/manuals/programming/API/index.html
            switch (Target)
            {
                case Print3dServerTarget.PrusaConnect:
                case Print3dServerTarget.Custom:
                    break;
                case Print3dServerTarget.RepetierServer:
                    AddOrUpdateAuthHeader("apikey", value, AuthenticationHeaderTarget.UrlSegment, 0, AuthenticationTypeTarget.Both);
                    break;
                case Print3dServerTarget.OctoPrint:
                case Print3dServerTarget.Moonraker:
                default:
                    AddOrUpdateAuthHeader("X-Api-Key", value, AuthenticationHeaderTarget.Header, 0, AuthenticationTypeTarget.Both);
                    break;
            }
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListening)
            {
                _ = UpdateWebSocketAsync();
            }
        }

        [ObservableProperty]
        public partial string ApiKeyRegexPattern { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool OverrideValidationRules { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public new partial bool IsOnline { get; set; } = false;
        partial void OnIsOnlineChanged(bool value)
        {
            base.IsOnline = value;
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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsRefreshing { get; set; } = false;

        #endregion

        #region Update

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool UpdateAvailable { get; set; } = false;
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
        public partial long FreeDiskSpace { get; set; } = 0;

        [ObservableProperty]
        public partial long UsedDiskSpace { get; set; } = 0;

        [ObservableProperty]
        public partial long TotalDiskSpace { get; set; } = 0;

        #endregion

        #region PrinterStateInformation

        #region ConfigurationInfo

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool ShutdownAfterPrint { get; set; } = false;

        #endregion

        #region PrinterState
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsPrinting { get; set; } = false;
        partial void OnIsPrintingChanged(bool value)
        {
            OnIsPrintingStateChanged(new IsPrintingStateChangedEventArgs()
            {
                IsPrinting = value,
                IsPaused = IsPaused,
                SessionId = SessionId,
                CallbackId = -1,
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsPaused { get; set; } = false;

        partial void OnIsPausedChanged(bool value)
        {
            OnIsPrintingStateChanged(new IsPrintingStateChangedEventArgs()
            {
                IsPrinting = IsPrinting,
                IsPaused = value,
                SessionId = SessionId,
                CallbackId = -1,
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsConnectedPrinterOnline { get; set; } = false;

        #endregion

        #region Temperatures

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double TemperatureExtruderMain { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double TemperatureExtruderSecondary { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double TemperatureHeatedBedMain { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double TemperatureHeatedChamberMain { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double SpeedFactor { get; set; } = 100;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double SpeedFactorTarget { get; set; } = 100;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double FlowFactor { get; set; } = 100;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double FlowFactorTarget { get; set; } = 100;

        #endregion

        #region Fans
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int SpeedFanMain { get; set; } = 0;

        #endregion

        #region Printers
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IPrinter3d? ActivePrinter { get; set; }
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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ObservableCollection<IPrinter3d> Printers { get; set; } = [];
        partial void OnPrintersChanged(ObservableCollection<IPrinter3d> value)
        {
            if (value?.Count > 0 && ActivePrinter == null)
            {
                ActivePrinter = value.FirstOrDefault();
            }
            OnRemotePrintersChanged(new PrintersChangedEventArgs()
            {
                SessionId = SessionId,
                NewPrinters = value is null ? [] : [.. value],
                Printer = GetActivePrinterSlug(),
                AuthToken = ApiKey,
            });
        }

        #endregion

        #region Files
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ObservableCollection<IGcodeGroup> Groups { get; set; } = [];
        partial void OnGroupsChanged(ObservableCollection<IGcodeGroup> value)
        {
            OnGcodeGroupsChangedEvent(new GcodeGroupsChangedEventArgs()
            {
                NewModelGroups = [.. value],
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ObservableCollection<IGcode> Files { get; set; } = [];
        partial void OnFilesChanged(ObservableCollection<IGcode> value)
        {
            OnGcodesChangedEvent(new GcodesChangedEventArgs()
            {
                NewModels = [.. value],
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        #endregion

        #region Jobs
        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial byte[] CurrentPrintImage { get; set; } = [];
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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ObservableCollection<IPrint3dJob> Jobs { get; set; } = [];
        partial void OnJobsChanged(ObservableCollection<IPrint3dJob> value)
        {
            OnJobListChangedEvent(new JobListChangedEventArgs()
            {
                NewJobList = [.. value],
                SessionId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IPrint3dJobStatus? ActiveJob { get; set; }
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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ObservableCollection<IPrint3dJobStatus> ActiveJobs { get; set; } = [];

        #endregion

        #region Position

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double X { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double Y { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial double Z { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial long Layer { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial long Layers { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool YHomed { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool ZHomed { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool XHomed { get; set; } = false;

        #endregion

        #endregion

        #region ReadOnly
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public string FullWebAddress => $"{ApiTargetPath}{(!string.IsNullOrEmpty(ApiVersion) ? $"/{ApiVersion}" : string.Empty)}";
        //$"{(IsSecure ? "https" : "http")}://{ApiTargetPath}:{Port}{(!string.IsNullOrEmpty(ApiVersion) ? $"/{ApiVersion}" : string.Empty)}";

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
#if NET6_0_OR_GREATER
        public bool IsReady
            => !string.IsNullOrEmpty(ApiTargetPath) &&
            (
                // Address
                //Uri.TryCreate(ApiTargetPath, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeWs) &&
                // API-Key (also allow empty key if the user performs a login instead
                (string.IsNullOrEmpty(ApiKey) || Regex.IsMatch(ApiKey, ApiKeyRegexPattern)) ||
                // Or validation rules are overriden
                OverrideValidationRules
            );
#else
        public bool IsReady   
            => !string.IsNullOrEmpty(ApiTargetPath) &&
            (
                // Address
                //Regex.IsMatch(ApiTargetPath, @"/^(wss?:\/\/)([0-9]{1,3}(?:\.[0-9]{1,3}){3}|[a-zA-Z]+):([0-9]{1,5})$/") &&
                // API-Key (also allow empty key if the user performs a login instead
                (string.IsNullOrEmpty(ApiKey) || Regex.IsMatch(ApiKey, ApiKeyRegexPattern))
                ||
                // Or validation rules are overriden
                OverrideValidationRules
            ); 
#endif
        #endregion

        #endregion

        #region Ctor
        public Print3dServerClient()
        {
            Id = Guid.NewGuid();
            UpdateRestClientInstance();
        }
        public Print3dServerClient(string serverAddress)
        {
            Id = Guid.NewGuid();
            InitInstance(serverAddress, "");
            UpdateRestClientInstance();
        }
        public Print3dServerClient(string serverAddress, string api) : base(serverAddress)
        {
            Id = Guid.NewGuid();
            InitInstance(serverAddress, api);
            UpdateRestClientInstance();
        }

        #endregion

        #region Dtor
        ~Print3dServerClient()
        {
            if (WebSocket is not null && WebSocket.IsRunning)
            {
                WebSocket.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, $"{nameof(Print3dServerClient)} was disposed...");
            }
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
        public virtual void InitInstance(string serverAddress, string api = "")
        {
            try
            {
                ApiTargetPath = serverAddress;
                ApiKey = api;

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
        public new virtual async Task CheckOnlineAsync(int timeout = 10)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeout));
            await CheckOnlineAsync(FullWebAddress, AuthHeaders, "", cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public new virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10)
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeout));
            await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public new virtual async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource? cts = default)
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
                cts = new(TimeSpan.FromSeconds(3));
                await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
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
                    result = await SendGcodeAsync(command: "send", new { cmd }, targetUri: targetUri).ConfigureAwait(false);
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
        public abstract Task<List<IPrinter3d>> GetPrintersAsync();
        public virtual async Task SetPrinterActiveAsync(string slug, bool refreshPrinterList = true)
        {
            if (refreshPrinterList)
            {
                List<IPrinter3d> printers = await GetPrintersAsync().ConfigureAwait(false);
                Printers = [.. printers];
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
                List<IPrinter3d> printers = await GetPrintersAsync().ConfigureAwait(false);
                Printers = [.. printers];
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
        public abstract Task<List<IGcode>> GetFilesAsync();
        public abstract Task<List<IGcodeGroup>> GetModelGroupsAsync(string path = "");

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
        public override int GetHashCode() => Id.GetHashCode();
        
        #endregion

        #region Dispose
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected new void Dispose(bool disposing)
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
    }
}

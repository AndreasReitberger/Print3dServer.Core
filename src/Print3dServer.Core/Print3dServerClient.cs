using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.Core.Utilities;
using RestSharp;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient : ObservableObject, IPrint3dServerClient
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
        List<Task> refreshTasks = new();
        #endregion

        #region Instance

        static readonly object Lock = new();
        static Print3dServerClient? _instance = null;
        public static Print3dServerClient Instance
        {
            get
            {
                lock (Lock)
                {
                    _instance ??= new Print3dServerClient();
                }
                return _instance;
            }

            set
            {
                if (_instance == value) return;
                lock (Lock)
                {
                    _instance = value;
                }
            }

        }

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
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        Timer timer;

        [ObservableProperty]
        int refreshInterval = 3;
        partial void OnRefreshIntervalChanged(int value)
        {
            if (IsListening)
            {
                _ = StartListeningAsync(target: WebSocketTargetUri, stopActiveListening: true, refreshFunctions: refreshTasks);
            }
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isListening = false;
        partial void OnIsListeningChanged(bool value)
        {
            OnListeningChangedEvent(new ListeningChangedEventArgs()
            {
                SessonId = SessionId,
                IsListening = value,
                IsListeningToWebSocket = IsListeningToWebsocket,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool initialDataFetched = false;

        #endregion

        #region Debug
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<string, string> ignoredJsonResults = new();
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
        Dictionary<string, IAuthenticationHeader> authHeaders = new();

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string sessionId = string.Empty;
        partial void OnSessionIdChanged(string value)
        {
            if (AuthHeaders?.ContainsKey("session") is true)
            {
                AuthHeaders["session"] = new AuthenticationHeader() { Token = value };
            }
            else
            {
                AuthHeaders?.Add("session", new AuthenticationHeader() { Token = value });
            }
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
            if (IsInitialized && IsListeningToWebsocket)
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
                case Print3dServerTarget.PrusaConnect:
                case Print3dServerTarget.OctoPrint:
                case Print3dServerTarget.Moonraker:
                case Print3dServerTarget.RepetierServer:
                    if (AuthHeaders?.ContainsKey("apikey") is true)
                    {
                        AuthHeaders["apikey"] = new AuthenticationHeader() { Token = value };
                    }
                    else
                    {
                        AuthHeaders?.Add("apikey", new AuthenticationHeader() { Token = value });
                    }          
                    break;
                case Print3dServerTarget.Custom:
                    break;
                default:
                    break;
            }
            WebSocketTargetUri = GetWebSocketTargetUri();
            if (IsInitialized && IsListeningToWebsocket)
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
            if (IsInitialized && IsListeningToWebsocket)
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
                    SessonId = SessionId,
                });
            }
            else
            {
                OnServerWentOffline(new Print3dBaseEventArgs()
                {
                    SessonId = SessionId,
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
                    SessonId = SessionId,
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
        long activeToolHead = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        long numberOfToolHeads = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isMultiExtruder = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasHeatedBed = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasHeatedChamber = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasFan = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasWebCam = false;

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
                SessonId = SessionId,
                NewPrinter = value,
                OldPrinter = ActivePrinter,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrinter3d> printers = new();
        partial void OnPrintersChanged(ObservableCollection<IPrinter3d> value)
        {
            if (value?.Count > 0 && ActivePrinter == null)
            {
                ActivePrinter = value.FirstOrDefault();
            }
            OnRemotePrintersChanged(new PrintersChangedEventArgs()
            {
                SessonId = SessionId,
                NewPrinters = value,
                Printer = GetActivePrinterSlug(),
                AuthToken = ApiKey,
            });
        }

        #endregion

        #region Files
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IGcodeGroup> groups = new();
        partial void OnGroupsChanged(ObservableCollection<IGcodeGroup> value)
        {
            OnGcodeGroupsChangedEvent(new GcodeGroupsChangedEventArgs()
            {
                NewModelGroups = value,
                SessonId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IGcode> files = new();
        partial void OnFilesChanged(ObservableCollection<IGcode> value)
        {
            OnGcodesChangedEvent(new GcodesChangedEventArgs()
            {
                NewModels = value,
                SessonId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        #endregion

        #region Jobs
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        byte[] currentPrintImage = Array.Empty<byte>();
        partial void OnCurrentPrintImageChanging(byte[] value)
        {
            OnActivePrintImageChanged(new ActivePrintImageChangedEventArgs()
            {
                NewImage = value,
                PreviousImage = CurrentPrintImage,
                SessonId = SessionId,
                CallbackId = -1,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrint3dJob> jobs = new();
        partial void OnJobsChanged(ObservableCollection<IPrint3dJob> value)
        {
            OnJobListChangedEvent(new JobListChangedEventArgs()
            {
                NewJobList = value,
                SessonId = SessionId,
                CallbackId = -1,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IPrint3dJobStatus? jobStatus;

        #endregion

        #region WebCams

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IWebCamConfig selectedWebCam;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IWebCamConfig> webCams = new();
        #endregion

        #region Fans

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IPrint3dFan> fans = new();
        #endregion

        #region Toolheads
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IHeaterComponent> toolheads = new();
        #endregion

        #region Beds

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IHeaterComponent> heatedBeds = new();

        #endregion

        #region Chambers

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IHeaterComponent> heatedChambers = new();
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
        public string FullWebAddress => $"{(IsSecure ? "https" : "http")}://{ServerAddress}:{Port}";
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

        public Print3dServerClient(string serverAddress, string api, int port = 3344, bool isSecure = false)
        {
            Id = Guid.NewGuid();
            InitInstance(serverAddress, port, api, isSecure);
            UpdateRestClientInstance();
        }

        public Print3dServerClient(string serverAddress, int port = 3344, bool isSecure = false)
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

        #region ValidateResult

        protected bool GetQueryResult(string result, bool emptyResultIsValid = false)
        {
            try
            {
                if ((string.IsNullOrEmpty(result) || result == "{}") && emptyResultIsValid)
                    return true;
                IQueryActionResult? actionResult = GetObjectFromJson<QueryActionResult>(result);
                return actionResult?.Ok ?? false;
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result,
                    Message = jecx.Message,
                });
                return false;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        protected IRestApiRequestRespone ValidateRespone(RestResponse respone, Uri targetUri)
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            try
            {
                if ((
                    respone.StatusCode == HttpStatusCode.OK || respone.StatusCode == HttpStatusCode.NoContent) &&
                    respone.ResponseStatus == ResponseStatus.Completed)
                {
                    apiRsponeResult.IsOnline = true;
                    AuthenticationFailed = false;
                    apiRsponeResult.Result = respone.Content;
                    apiRsponeResult.Succeeded = true;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else if (respone.StatusCode == HttpStatusCode.NonAuthoritativeInformation
                    || respone.StatusCode == HttpStatusCode.Forbidden
                    || respone.StatusCode == HttpStatusCode.Unauthorized
                    )
                {
                    apiRsponeResult.IsOnline = true;
                    apiRsponeResult.HasAuthenticationError = true;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else if (respone.StatusCode == HttpStatusCode.Conflict)
                {
                    apiRsponeResult.IsOnline = true;
                    apiRsponeResult.HasAuthenticationError = false;
                    apiRsponeResult.EventArgs = new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    };
                }
                else
                {
                    OnRestApiError(new RestEventArgs()
                    {
                        Status = respone.ResponseStatus.ToString(),
                        Exception = respone.ErrorException,
                        Message = respone.ErrorMessage,
                        Uri = targetUri,
                    });
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }
        #endregion

        #region ValidateActivePrinter
        protected string GetActivePrinterSlug()
        {
            try
            {
                if (!IsReady || ActivePrinter == null)
                {
                    return string.Empty;
                }
                return ActivePrinter?.Slug;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return string.Empty;
            }
        }
        protected bool IsPrinterSlugSelected(string PrinterSlug)
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

        #region Rest Api
        protected async Task<IRestApiRequestRespone?> SendRestApiRequestAsync(
            string requestTargetUri,
            Method method,
            string command,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            object jsonObject = null,
            CancellationTokenSource cts = default,
            Dictionary<string, string> urlSegments = null
            )
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            if (!IsOnline) return apiRsponeResult;

            try
            {
                if (cts == default)
                {
                    cts = new(DefaultTimeout);
                }
                // https://github.com/Arksine/moonraker/blob/master/docs/web_api.md
                if (restClient == null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new($"{requestTargetUri}/{command}")
                {
                    RequestFormat = DataFormat.Json,
                    Method = method
                };

                if(authHeaders?.Count > 0)
                {
                    switch (Target)
                    {
                        // Special handling for Repetier Server
                        case Print3dServerTarget.RepetierServer:
                            string? key = authHeaders?.FirstOrDefault(x => x.Key == "apikey").Value?.Token;
                            if(key is not null)
                                request.AddParameter("apikey", key, ParameterType.QueryString);
                            break;
                        case Print3dServerTarget.Moonraker:
                            if (!string.IsNullOrEmpty(SessionId))
                            {
                                request.AddHeader("Authorization", $"Bearer {SessionId}");
                            }
                            else
                            {
                                string? apiKey = authHeaders?.FirstOrDefault(x => x.Key == "apikey").Value?.Token;
                                if (apiKey is not null)
                                {
                                    request.AddHeader("X-Api-Key", $"Bearer {apiKey}");
                                }
                            }
                            
                            break;
                        case Print3dServerTarget.OctoPrint:
                        case Print3dServerTarget.PrusaConnect:
                        case Print3dServerTarget.Custom:       
                        default:
                            foreach (var header in authHeaders)
                            {
                                //  "Authorization", $"Bearer {UserToken}"
                                //  "X-Api-Key", $"{ApiKey}"
                                request.AddHeader(header.Key, header.Value.Token);
                            }
                            break;
                    }
                }
                switch (Target)
                {
                    case Print3dServerTarget.RepetierServer:
                        if (string.IsNullOrEmpty(command)) break;
                        urlSegments ??= new();
                        urlSegments.Add("a", command);
                        break;
                    case Print3dServerTarget.Moonraker:
                        break;
                    case Print3dServerTarget.OctoPrint:
                        break;
                    case Print3dServerTarget.PrusaConnect:
                        break;
                    case Print3dServerTarget.Custom:
                        break;
                    default:
                        break;
                }
                if (urlSegments != null)
                {
                    foreach (KeyValuePair<string, string> pair in urlSegments)
                    {
                        request.AddParameter(pair.Key, pair.Value, ParameterType.QueryString);
                    }
                }

                if (jsonObject != null)
                {
                    request.AddJsonBody(jsonObject, "application/json");
                }
                Uri fullUri = restClient.BuildUri(request);
                try
                {
                    RestResponse respone = await restClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
                    apiRsponeResult = ValidateRespone(respone, fullUri) as RestApiRequestRespone;
                }
                catch (TaskCanceledException texp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }

        protected async Task<IRestApiRequestRespone?> SendOnlineCheckRestApiRequestAsync(
            string requestTargetUri,
            string command,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            CancellationTokenSource cts
            )
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = false };
            try
            {
                if (cts == default)
                {
                    cts = new(DefaultTimeout);
                }
                if (restClient == null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new( $"{requestTargetUri}/{command}")
                {
                    RequestFormat = DataFormat.Json,
                    Method = Method.Get
                };
                if (authHeaders?.Count > 0)
                {
                    foreach (var header in authHeaders)
                    {
                        //  "Authorization", $"Bearer {UserToken}"
                        //  "X-Api-Key", $"{ApiKey}"
                        request.AddHeader(header.Key, header.Value.Token);
                    }
                }
                Uri fullUri = restClient.BuildUri(request);
                try
                {
                    RestResponse respone = await restClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
                    apiRsponeResult = ValidateRespone(respone, fullUri) as RestApiRequestRespone;
                }
                catch (TaskCanceledException)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                }
                catch (HttpRequestException)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                }
                catch (TimeoutException)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                }

            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }

        protected async Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(
            string filePath,
            Dictionary<string, string> authHeaders,
            string root = "/server/files/upload/",
            string fileTarget = "gcodes",
            string path = "",
            string contentType = "multipart/form-data",
            string fileTargetName = "file",
            string fileContentType = "application/octet-stream",
            int timeout = 100000
            )
        {
            RestApiRequestRespone apiRsponeResult = new();
            if (!IsOnline) return apiRsponeResult;

            try
            {
                if (restClient == null)
                {
                    UpdateRestClientInstance();
                }
                CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 0, timeout));
                RestRequest request = new(root);

                if (authHeaders?.Count > 0)
                {
                    foreach (var header in authHeaders)
                    {
                        //  "Authorization", $"Bearer {UserToken}"
                        //  "X-Api-Key", $"{ApiKey}"
                        request.AddHeader(header.Key, header.Value);
                    }
                }

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;

                //Multiform
                request.AddHeader("Content-Type", contentType ?? "multipart/form-data");
                request.AddFile(fileTargetName ?? "file", filePath, fileContentType ?? "application/octet-stream");
                request.AddParameter("root", fileTarget, ParameterType.GetOrPost);
                request.AddParameter("path", path, ParameterType.GetOrPost);

                Uri fullUri = restClient.BuildUri(request);
                try
                {
                    RestResponse respone = await restClient.ExecuteAsync(request, cts.Token);
                    apiRsponeResult = ValidateRespone(respone, fullUri) as RestApiRequestRespone;                    
                }
                catch (TaskCanceledException texp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }

        protected async Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestAsync(
            string fileName,
            byte[] file,
            Dictionary<string, string> authHeaders,
            string root = "/server/files/upload/",
            string fileTarget = "gcodes",
            string path = "",
            string contentType = "multipart/form-data",
            string fileTargetName = "file",
            string fileContentType = "application/octet-stream",
            int timeout = 100000
            )
        {
            RestApiRequestRespone apiRsponeResult = new();
            if (!IsOnline) return apiRsponeResult;

            try
            {
                if (restClient is null)
                {
                    UpdateRestClientInstance();
                }
                CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 0, timeout));
                RestRequest request = new("/server/files/upload");

                if (authHeaders?.Count > 0)
                {
                    foreach (var header in authHeaders)
                    {
                        //  "Authorization", $"Bearer {UserToken}"
                        //  "X-Api-Key", $"{ApiKey}"
                        request.AddHeader(header.Key, header.Value);
                    }
                }

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;

                //Multiform
                request.AddHeader("Content-Type", contentType ?? "multipart/form-data");
                request.AddFile(fileTargetName ?? "file", file, fileName, fileContentType ?? "application/octet-stream");
                request.AddParameter("root", fileTarget, ParameterType.GetOrPost);
                request.AddParameter("path", path, ParameterType.GetOrPost);

                Uri fullUri = restClient.BuildUri(request);
                try
                {
                    RestResponse respone = await restClient.ExecuteAsync(request, cts.Token);
                    apiRsponeResult = ValidateRespone(respone, fullUri) as RestApiRequestRespone;                 
                }
                catch (TaskCanceledException texp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(texp, false));
                    }
                }
                catch (HttpRequestException hexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(hexp, false));
                    }
                }
                catch (TimeoutException toexp)
                {
                    // Throws exception on timeout, not actually an error but indicates if the server is not reachable.
                    if (!IsOnline)
                    {
                        OnError(new UnhandledExceptionEventArgs(toexp, false));
                    }
                }
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return apiRsponeResult;
        }
        #endregion

        #region Timers

        void StopTimer()
        {
            if (Timer != null)
            {
                try
                {
                    Timer?.Change(Timeout.Infinite, Timeout.Infinite);
                    Timer = null;
                    IsListening = false;
                }
                catch (ObjectDisposedException)
                {
                    //PingTimer = null;
                }
            }
        }
        #endregion

        #endregion

        #region Public

        #region Init
        public void InitInstance()
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
        public void InitInstance(string serverAddress, int port, string api = "", bool isSecure = false)
        {
            try
            {
                ServerAddress = serverAddress;
                ApiKey = api;
                Port = port;
                IsSecure = isSecure;
                //WebSocketTargetUri = GetWebSocketTargetUri();

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

        #region Rest Api
        public async Task CheckOnlineAsync(string commandBase,Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            CancellationTokenSource cts = new(timeout);
            await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            cts?.Dispose();
        }

        public async Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource cts = default)
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
                    IRestApiRequestRespone? respone = await SendOnlineCheckRestApiRequestAsync(
                       commandBase, command,
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
                cts = new(3500);
                await CheckOnlineAsync(commandBase, authHeaders, command, cts).ConfigureAwait(false);
            }
        }

        public async Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000)
        {
            try
            {
                if (IsOnline)
                {
                    RestApiRequestRespone? respone = await SendRestApiRequestAsync(
                        commandBase, Method.Post, command,
                        authHeaders: authHeaders,
                        cts: new(timeout))
                        .ConfigureAwait(false) as RestApiRequestRespone;
                    if (respone.HasAuthenticationError)
                    {
                        AuthenticationFailed = true;
                        OnRestApiAuthenticationError(respone.EventArgs as RestEventArgs);
                    }
                    else
                    {
                        AuthenticationFailed = false;
                        OnRestApiAuthenticationSucceeded(respone.EventArgs as RestEventArgs);
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
        
        public async Task<bool> SendGcodeAsync(string command = "send", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Download
        public async Task<byte[]?> DownloadFileFromUriAsync(
            string path, 
            Dictionary<string, string> authHeaders,
            Dictionary<string, string> urlSegements = null, 
            int timeout = 10000
            )
        {
            try
            {
                if (restClient is null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(path);
                if (authHeaders?.Count > 0)
                {
                    foreach (var header in authHeaders)
                    {
                        //  "Authorization", $"Bearer {UserToken}"
                        //  "X-Api-Key", $"{ApiKey}"
                        request.AddHeader(header.Key, header.Value);
                    }
                }

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Get;
                request.Timeout = timeout;
                if (urlSegements?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> segment in urlSegements)
                    {
                        request.AddParameter(segment.Key, segment.Value);
                    }
                }

                Uri fullUrl = restClient.BuildUri(request);
                CancellationTokenSource cts = new(timeout);
                byte[]? respone = await restClient.DownloadDataAsync(request, cts.Token)
                    .ConfigureAwait(false)
                    ;

                return respone;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return null;
            }
        }
        #endregion

        #region Misc
        public void CancelCurrentRequests()
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
        public async Task<bool> HomeAsync(bool x, bool y, bool z, string? targetUri = null)
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

        public async Task<bool> SetExtruderTemperatureAsync(string command = "setExtruderTemperature", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public async Task<bool> SetBedTemperatureAsync(string command = "setBedTemperature", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public async Task<bool> SetChamberTemperatureAsync(string command = "setChamberTemperature", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }
        #endregion

        #region Jobs

        public async Task<bool> StartJobAsync(IPrint3dJob job, string command = "startJob", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public async Task<bool> RemoveJobAsync(IPrint3dJob job, string command = "removeJob", object? data = null, string? targetUri = null)
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
                return GetQueryResult(result.Result, true);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return false;
            }
        }

        public async Task<bool> ContinueJobAsync(string command = "continueJob", object? data = null, string? targetUri = null)
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

        public async Task<bool> PauseJobAsync(string command = "pauseJob", object? data = null, string? targetUri = null)
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

        public async Task<bool> StopJobAsync(string command = "continueJob", object? data = null, string? targetUri = null)
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
        public async Task<bool> SetFanSpeedAsync(string command = "setFanSpeed", object? data = null, string? targetUri = null)
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

        #endregion

        #endregion

        #region Overrides
        public override string ToString()
        {
            try
            {
                return FullWebAddress;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return string.Empty;
            }
        }
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

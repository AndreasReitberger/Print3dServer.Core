using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.Core.Utilities;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient : ObservableObject, IPrint3dServerClient
    {
        #region Variables
        RestClient? restClient;
        HttpClient? httpClient;
        int _retries = 0;
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
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
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
        Dictionary<string, IAuthenticationHeader> authHeaders = new();

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        string sessionId = string.Empty;

        [ObservableProperty]
        string serverName = string.Empty;

        [ObservableProperty]
        string checkOnlineTargetUri = string.Empty;

        [ObservableProperty]
        string serverAddress = string.Empty;
        partial void OnServerAddressChanged(string value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        bool loginRequired = false;

        [ObservableProperty]
        bool isSecure = false;
        partial void OnIsSecureChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        string apiKey = string.Empty;

        [ObservableProperty]
        string apiKeyRegexPattern = string.Empty;

        [ObservableProperty]
        int port = 3344;
        partial void OnPortChanged(int value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        int defaultTimeout = 10000;

        [ObservableProperty]
        bool overrideValidationRules = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
        bool isConnecting = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool authenticationFailed = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isRefreshing = false;

        [ObservableProperty]
        int retriesWhenOffline = 2;

        #endregion

        #region Update

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
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

        #region Proxy
        [ObservableProperty]
        bool enableProxy = false;
        partial void OnEnableProxyChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        bool proxyUserUsesDefaultCredentials = true;
        partial void OnProxyUserUsesDefaultCredentialsChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        bool secureProxyConnection = true;
        partial void OnSecureProxyConnectionChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        string proxyAddress = string.Empty;
        partial void OnProxyAddressChanged(string value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        int proxyPort = 443;
        partial void OnProxyPortChanged(int value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        string proxyUser = string.Empty;
        partial void OnProxyUserChanged(string value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        SecureString proxyPassword;
        partial void OnProxyPasswordChanged(SecureString value)
        {
            UpdateRestClientInstance();
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
        [property: JsonIgnore, XmlIgnore]
        long activeToolHead = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        long numberOfToolHeads = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isDualExtruder = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool hasHeatedBed = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool hasHeatedChamber = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool hasFan = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool hasWebCam = false;

        #endregion

        #region PrinterState
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isPrinting = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isPaused = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool isConnectedPrinterOnline = false;

        #endregion

        #region Temperatures

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double temperatureExtruderMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double temperatureExtruderSecondary = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double temperatureHeatedBedMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double temperatureHeatedChamberMain = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double speedFactor = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double speedFactorTarget = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double flowFactor = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double flowFactorTarget = 0;

        #endregion

        #region Fans
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int speedFanMain = 0;

        #endregion

        #region Printers
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
        ObservableCollection<IPrinter3d> printers = new();
        partial void OnPrintersChanged(ObservableCollection<IPrinter3d> value)
        {
            if (value?.Count > 0 && ActivePrinter == null)
            {
                ActivePrinter = value.FirstOrDefault();
            }
        }

        #endregion

        #region Files
        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
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
        [property: JsonIgnore, XmlIgnore]
        IPrint3dJobStatus? jobStatus;

        #endregion

        #region Position

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double x = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double y = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        double z = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int layer = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        int layers = 0;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool yHomed = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
        bool zHomed = false;

        [ObservableProperty]
        [property: JsonIgnore, XmlIgnore]
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
                    foreach (var header in authHeaders)
                    {
                        //  "Authorization", $"Bearer {UserToken}"
                        //  "X-Api-Key", $"{ApiKey}"
                        request.AddHeader(header.Key, header.Value.Token);
                    }
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
        
        public async Task<bool> SendGcodeAsync(string command = "send", object? data = null)
        {
            try
            {
                data ??= new
                {
                    cmd = "",
                };
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: ApiTargetPath,
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

        #region Proxy
        Uri GetProxyUri() => ProxyAddress.StartsWith("http://") || ProxyAddress.StartsWith("https://") ? new Uri($"{ProxyAddress}:{ProxyPort}") : new Uri($"{(SecureProxyConnection ? "https" : "http")}://{ProxyAddress}:{ProxyPort}");

        WebProxy GetCurrentProxy()
        {
            WebProxy proxy = new()
            {
                Address = GetProxyUri(),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = ProxyUserUsesDefaultCredentials,
            };
            if (ProxyUserUsesDefaultCredentials && !string.IsNullOrEmpty(ProxyUser))
            {
                proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
            }
            else
            {
                proxy.UseDefaultCredentials = ProxyUserUsesDefaultCredentials;
            }
            return proxy;
        }
        void UpdateRestClientInstance()
        {
            if (string.IsNullOrEmpty(ServerAddress))
            {
                return;
            }
            if (EnableProxy && !string.IsNullOrEmpty(ProxyAddress))
            {
                RestClientOptions options = new(FullWebAddress)
                {
                    ThrowOnAnyError = true,
                    MaxTimeout = 10000,
                };
                HttpClientHandler httpHandler = new()
                {
                    UseProxy = true,
                    Proxy = GetCurrentProxy(),
                    AllowAutoRedirect = true,
                };

                httpClient = new(handler: httpHandler, disposeHandler: true);
                restClient = new(httpClient: httpClient, options: options);
            }
            else
            {
                httpClient = null;
                restClient = new(baseUrl: FullWebAddress);
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
        public async Task<bool> HomeAsync(bool x, bool y, bool z)
        {
            try
            {
                bool result;
                if (x && y && z)
                {
                    result = await SendGcodeAsync(command: "send", new { cmd = "G28" }).ConfigureAwait(false);
                }
                else
                {
                    string cmd = string.Format("G28{0}{1}{2}", x ? " X0 " : "", y ? " Y0 " : "", z ? " Z0 " : "");
                    result = await SendGcodeAsync(command: "send", new { cmd = cmd }).ConfigureAwait(false);
                }
                return result;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
            }
            return false;
        }
        #endregion

        #region Jobs

        public async Task<bool> StartJobAsync(IPrint3dJob job, string command = "startJob", object? data = null)
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
                        requestTargetUri: ApiTargetPath, 
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

        public async Task<bool> RemoveJobAsync(IPrint3dJob job, string command = "removeJob", object? data = null)
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
                        requestTargetUri: ApiTargetPath,
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

        public async Task<bool> ContinueJobAsync(string command = "continueJob", object? data = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: ApiTargetPath,
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

        public async Task<bool> PauseJobAsync(string command = "pauseJob", object? data = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: ApiTargetPath,
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

        public async Task<bool> StopJobAsync(string command = "continueJob", object? data = null)
        {
            try
            {
                IRestApiRequestRespone? result =
                    await SendRestApiRequestAsync(
                        requestTargetUri: ApiTargetPath,
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

        #endregion

        #endregion

        #region Interface, unused
        public async Task<bool> DeleteAsync()
        {
            throw new NotSupportedException("This method is not supported on this object!");
        }
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

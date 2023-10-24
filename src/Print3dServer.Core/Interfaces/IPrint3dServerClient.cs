using AndreasReitberger.API.Print3dServer.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Security;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dServerClient : IPrint3dBase
    {
        #region Properties

        #region General
        public Print3dServerTarget Target { get; set; }
        public Guid Id { get; set; }
        #endregion

        #region Instance
        public bool UpdateInstance { get; set; }
        public static IPrint3dServerClient? Instance { get; set; }
        #endregion

        #region Connection
        public string SessionId { get; set; }
        public string ServerName { get; set; }
        public string ServerAddress { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyRegexPattern { get; set; }
        public string CheckOnlineTargetUri { get; set; }
        public int Port { get; set; }
        public int DefaultTimeout { get; set; }
        public int RetriesWhenOffline { get; set; }
        public bool IsSecure { get; set; }
        public bool OverrideValidationRules { get; set; }
        #endregion

        #region Auth
        Dictionary<string, IAuthenticationHeader> AuthHeaders { get; set; }
        public bool LoginRequired { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool AuthenticationFailed { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        #endregion

        #region States
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsConnectedPrinterOnline { get; set; }
        public bool IsRefreshing { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsListening { get; set; }
        public bool InitialDataFetched { get; set; }
        public bool UpdateAvailable { get; set; }
        #endregion

        #region Api
        public string ApiVersion { get; set; }
        public string ApiTargetPath { get; set; }

        #endregion

        #region Proxy

        public bool EnableProxy { get; set; }
        public bool ProxyUserUsesDefaultCredentials { get; set; }
        public bool SecureProxyConnection { get; set; }
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUser { get; set; }
        public SecureString ProxyPassword { get; set; }

        #endregion

        #region Timing
        public Timer Timer { get; set; }
        public int RefreshInterval { get; set; }
        #endregion

        #region Data Storage

        public long FreeDiskSpace { get; set; }
        public long UsedDiskSpace { get; set; }
        public long TotalDiskSpace { get; set; }

        #endregion

        #region Printer States
        public IPrint3dJobStatus ActiveJob { get; set; }
        public byte[] CurrentPrintImage { get; set; }

        public double TemperatureExtruderMain { get; set; }
        public double TemperatureExtruderSecondary { get; set; }
        public double TemperatureHeatedBedMain { get; set; }
        public double TemperatureHeatedChamberMain { get; set; }

        public double SpeedFactor { get; set; }
        public double SpeedFactorTarget { get; set; }
        public double FlowFactor { get; set; }
        public double FlowFactorTarget { get; set; }

        public int NumberOfToolHeads { get; set; }
        public int ActiveToolheadIndex { get; set; }

        public bool IsMultiExtruder { get; set; }
        public bool HasHeatedBed { get; set; }
        public bool HasFan { get; set; }
        public bool HasHeatedChamber { get; set; }
        public bool ShutdownAfterPrint { get; set; }

        #endregion

        #region Heaters
        public IHeaterComponent ActiveHeatedBed { get; set; }
        public IHeaterComponent ActiveHeatedChamber { get; set; }
        #endregion

        #region ToolHead
        public IToolhead ActiveToolhead { get; set; }   
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool XHomed { get; set; }
        public bool YHomed { get; set; }
        public bool ZHomed { get; set; }
        public long Layer { get; set; }
        public long Layers { get; set; }
        #endregion

        #region WebCam
        public bool HasWebCam { get; set; }
        public IWebCamConfig SelectedWebCam { get; set; }
        public string WebCamTargetUri { get; set; }
        public string WebCamTarget { get; set; }
        public int WebCamIndex { get; set; }
        public string WebCamMultiCamTarget { get; set; }
        #endregion

        #region Printer

        public IPrinter3d ActivePrinter { get; set; }

        #endregion

        #region WebSocket
        public string PingCommand { get; set; }
        public long PingCounter { get; set; }
        public int PingInterval { get; set; }
        public string WebSocketTargetUri { get; set; }
        public string WebSocketTarget { get; set; }
        public long LastPingTimestamp { get; set; }
        public long LastRefreshTimestamp { get; set; }
        public Func<Task>? OnRefresh { get; set; }
        #endregion

        #region Data Convertion

        public ConcurrentDictionary<string, string> IgnoredJsonResults { get;set; }


        #endregion

        #endregion

        #region Collections
        public ObservableCollection<IPrinter3d> Printers { get; set; }
        public ObservableCollection<IGcodeGroup> Groups { get; set; }
        public ObservableCollection<IGcode> Files { get; set; }
        public ObservableCollection<IPrint3dJob> Jobs { get; set; }
        public ObservableCollection<IPrint3dJobStatus> ActiveJobs { get; set; }
        public ObservableCollection<IWebCamConfig> WebCams { get; set; }
        public ObservableCollection<IPrint3dFan> Fans { get; set; }
        public ConcurrentDictionary<int, IToolhead> Toolheads { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> HeatedBeds { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> HeatedChambers { get; set; }
        #endregion

        #region Methods

        #region OnlineCheck
        public Task CheckOnlineAsync(int timeout = 10000);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000);
        public Task CheckOnlineAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, CancellationTokenSource cts = default);
        public Task<bool> CheckIfApiIsValidAsync(string commandBase, Dictionary<string, IAuthenticationHeader> authHeaders, string? command = null, int timeout = 10000);
        #endregion

        #region Refreshing
        public Task RefreshAllAsync();
        public Task<ObservableCollection<IPrinter3d>> GetPrintersAsync();
        public Task<ObservableCollection<IGcode>> GetFilesAsync();
        #endregion

        #region Proxy
        public Uri GetProxyUri();
        public WebProxy GetCurrentProxy();
        public void UpdateRestClientInstance();
        public void SetProxy(bool secure, string address, int port, bool enable = true);
        public void SetProxy(bool secure, string address, int port, string user = "", SecureString? password = null, bool enable = true);
        #endregion

        #region WebSocket
        public string BuildPingCommand(object? data);
        public Task StartListeningAsync(bool stopActiveListening = false);
        public Task StartListeningAsync(string target, bool stopActiveListening = false, Func<Task>? refreshFunctions = null);
        public Task StopListeningAsync();
        public Task ConnectWebSocketAsync(string target);
        public Task DisconnectWebSocketAsync();
        public Task SendWebSocketCommandAsync(string command);
        public Task SendPingAsync();

        public Task UpdateWebSocketAsync(Func<Task>? refreshFunctions);
        #endregion

        #region Printer
        public Task SetPrinterActiveAsync(int index = -1, bool refreshPrinterList = true);
        public Task SetPrinterActiveAsync(string slug, bool refreshPrinterList = true);
        public Task<bool> SendGcodeAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> HomeAsync(bool x, bool y, bool z, string? targetUri = null);
        public Task<bool> SetFanSpeedAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> SetExtruderTemperatureAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> SetBedTemperatureAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> SetChamberTemperatureAsync(string command, object? data = null, string? targetUri = null);
        #endregion

        #region Jobs
        public Task<bool> StartJobAsync(IPrint3dJob job, string command, object? data = null, string? targetUri = null);
        public Task<bool> RemoveJobAsync(IPrint3dJob job, string command, object? data = null, string? targetUri = null);
        public Task<bool> ContinueJobAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> PauseJobAsync(string command, object? data = null, string? targetUri = null);
        public Task<bool> StopJobAsync(string command, object? data = null, string? targetUri = null);
        #endregion

        #region WebCam
        public Task<ObservableCollection<IWebCamConfig>> GetWebCamConfigsAsync();
        public Task<ObservableCollection<IWebCamConfig>> GetWebCamConfigsAsync(string command, object? data = null, string? targetUri = null);
        public string GetDefaultWebCamUri();
        public string GetWebCamUri(IWebCamConfig? config);
        public Task<string> GetWebCamUriAsync(int index = 0, bool refreshWebCamConfig = false);
        #endregion

        #region Misc

        public void CancelCurrentRequests();
        public IAuthenticationHeader? GetAuthHeader(string key);
        public void AddOrUpdateAuthHeader(string key, string value, int order = 0);
        #endregion

        #endregion
    }
}

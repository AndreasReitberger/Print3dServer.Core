using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.REST.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dServerClient : IRestApiClient
    {
        #region Properties

        #region General
        public Print3dServerTarget Target { get; set; }
        #endregion

        #region Instance
        public new static IPrint3dServerClient? Instance { get; set; }
        #endregion

        #region Connection
        public string SessionId { get; set; }
        public string ServerName { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyRegexPattern { get; set; }
        public string CheckOnlineTargetUri { get; set; }
        public int DefaultTimeout { get; set; }
        public int RetriesWhenOffline { get; set; }
        public bool OverrideValidationRules { get; set; }
        #endregion

        #region Auth
        public bool LoginRequired { get; set; }
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        #endregion

        #region States
        public bool IsActive { get; set; }
        public bool IsReady { get; }
        public bool IsPrinting { get; set; }
        public bool IsPaused { get; set; }
        public bool IsConnectedPrinterOnline { get; set; }
        public bool IsRefreshing { get; set; }
        public bool IsListening { get; set; }
        public bool InitialDataFetched { get; set; }
        public bool UpdateAvailable { get; set; }
        #endregion

        #region Timing
        public int RefreshInterval { get; set; }
        #endregion

        #region Data Storage

        public long FreeDiskSpace { get; set; }
        public long UsedDiskSpace { get; set; }
        public long TotalDiskSpace { get; set; }

        #endregion

        #region Printer States
        public IPrint3dJobStatus? ActiveJob { get; set; }
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

        public int NumberOfSensors { get; set; }

        public int NumberOfFans { get; set; }
        public string ActiveFanIndex { get; set; }

        public bool IsMultiExtruder { get; set; }
        public bool HasHeatedBed { get; set; }
        public bool HasFan { get; set; }
        public bool HasHeatedChamber { get; set; }
        public bool ShutdownAfterPrint { get; set; }

        #endregion

        #region Heaters
        public IHeaterComponent? ActiveHeatedBed { get; set; }
        public IHeaterComponent? ActiveHeatedChamber { get; set; }
        #endregion

        #region ToolHead
        public IToolhead? ActiveToolhead { get; set; }
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
        public IWebCamConfig? SelectedWebCam { get; set; }
        public string WebCamTargetUri { get; set; }
        public string WebCamTarget { get; set; }
        public int WebCamIndex { get; set; }
        public string WebCamMultiCamTarget { get; set; }
        #endregion

        #region Printer

        public IPrinter3d? ActivePrinter { get; set; }

        #endregion

        #region WebSocket
        public string PingCommand { get; set; }
        public long PingCounter { get; set; }
        public int PingInterval { get; set; }
        public int OnRefreshInterval { get; set; }
        public string WebSocketTargetUri { get; set; }
        public long LastPingTimestamp { get; set; }
        public long LastRefreshTimestamp { get; set; }
        public Func<Task>? OnRefresh { get; set; }
        #endregion

        #region Data Convertion

        public ConcurrentDictionary<string, string> IgnoredJsonResults { get; set; }


        #endregion

        #endregion

        #region Collections
        public ObservableCollection<IPrinter3d> Printers { get; set; }
        public ObservableCollection<IGcodeGroup> Groups { get; set; }
        public ObservableCollection<IGcode> Files { get; set; }
        public ObservableCollection<IPrint3dJob> Jobs { get; set; }
        public ObservableCollection<IPrint3dJobStatus> ActiveJobs { get; set; }
        public ObservableCollection<IWebCamConfig> WebCams { get; set; }
        //public ObservableCollection<IPrint3dFan> Fans { get; set; }
        public ConcurrentDictionary<string, ISensorComponent> Sensors { get; set; }
        public ConcurrentDictionary<string, IPrint3dFan> Fans { get; set; }
        public ConcurrentDictionary<int, IToolhead> Toolheads { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> HeatedBeds { get; set; }
        public ConcurrentDictionary<int, IHeaterComponent> HeatedChambers { get; set; }
        #endregion

        #region EventHandlers
        public event EventHandler<IgnoredJsonResultsChangedEventArgs>? IgnoredJsonResultsChanged;
        /*
        public event EventHandler<Print3dBaseEventArgs>? WebSocketConnected;
        public event EventHandler<Print3dBaseEventArgs>? WebSocketDisconnected;
        public event EventHandler<ErrorEventArgs>? WebSocketError;
        public event EventHandler<WebsocketEventArgs>? WebSocketMessageReceived;
        public event EventHandler<WebsocketEventArgs>? WebSocketDataReceived;
        public event EventHandler<LoginRequiredEventArgs>? LoginResultReceived;
        public event EventHandler<SessionChangedEventArgs>? SessionChanged;
        public event EventHandler<ListeningChangedEventArgs>? ListeningChanged;
        */
        public event EventHandler<Print3dBaseEventArgs>? ServerWentOffline;
        public event EventHandler<Print3dBaseEventArgs>? ServerWentOnline;
        public event EventHandler<Print3dBaseEventArgs>? ServerUpdateAvailable;

        public event EventHandler<JobStartedEventArgs>? JobsStarted;
        public event EventHandler<JobsChangedEventArgs>? JobsChanged;
        public event EventHandler<JobFinishedEventArgs>? JobFinished;
        public event EventHandler<JobStatusFinishedEventArgs>? JobStatusFinished;
        public event EventHandler<TemperatureDataEventArgs>? TemperatureDataReceived;
        public event EventHandler<GcodesChangedEventArgs>? GcodesChanged;
        public event EventHandler<GcodeGroupsChangedEventArgs>? GcodeGroupsChanged;
        public event EventHandler<ActivePrinterChangedEventArgs>? ActivePrinterChanged;
        public event EventHandler<ActivePrintImageChangedEventArgs>? ActivePrintImageChanged;
        public event EventHandler<JobListChangedEventArgs>? JobListChanged;

        public event EventHandler<PrintersChangedEventArgs>? RemotePrintersChanged;

        public event EventHandler<WebCamConfigChangedEventArgs>? WebCamConfigChanged;
        public event EventHandler<WebCamConfigsChangedEventArgs>? WebCamConfigsChanged;

        public event EventHandler<HeaterChangedEventArgs>? HeaterChanged;
        public event EventHandler<HeatersChangedEventArgs>? HeatersChanged;

        public event EventHandler<ToolheadChangedEventArgs>? ToolheadChanged;
        public event EventHandler<ToolheadsChangedEventArgs>? ToolheadsChanged;
        #endregion

        #region Methods

        #region Refreshing
        public Task RefreshAllAsync();
        #endregion

        #region Files
        public Task<List<IGcode>> GetFilesAsync();
        public Task<List<IGcodeGroup>> GetModelGroupsAsync(string path = "");
        /*
        public Task<IRestApiRequestRespone?> DeleteFileAsync(string filePath);
        public Task<IRestApiRequestRespone?> UploadFileAsync(string filePath);
        */
        public Task<byte[]?> DownloadFileAsync(string filePath);
        #endregion

        #region WebSocket
        /*
        public string BuildPingCommand(object? data);
        public Task StartListeningAsync(bool stopActiveListening = false, string[]? commandsOnConnect = null);
        public Task StartListeningAsync(string target, bool stopActiveListening = false, Func<Task>? refreshFunctions = null, string[]? commandsOnConnect = null);
        public Task StopListeningAsync();
        public Task DisconnectWebSocketAsync();
        public Task SendWebSocketCommandAsync(string command);
        public Task UpdateWebSocketAsync(Func<Task>? refreshFunctions, string[]? commandsOnConnect = null);
        public Task ConnectWebSocketAsync(string target, string commandOnConnect);
        public Task ConnectWebSocketAsync(string target, string[]? commandsOnConnect = null);
        */
        #endregion

        #region Printer
        public Task<List<IPrinter3d>> GetPrintersAsync();
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
        public Task<List<IWebCamConfig>?> GetWebCamConfigsAsync();
        public Task<List<IWebCamConfig>?> GetWebCamConfigsAsync(string command, object? data = null, string? targetUri = null);
        public string GetDefaultWebCamUri();
        public string GetWebCamUri(IWebCamConfig? config);
        public Task<string> GetWebCamUriAsync(int index = 0, bool refreshWebCamConfig = false);
        #endregion

        #endregion
    }
}

using AndreasReitberger.API.Print3dServer.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
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
        #endregion

        #region Auth
        Dictionary<string, IAuthenticationHeader> AuthHeaders { get; set; }
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
        public bool AuthenticationFailed { get; set; }
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
        public IPrint3dJobStatus JobStatus { get; set; }
        public byte[] CurrentPrintImage { get; set; }
        public double SpeedFactor { get; set; }
        public double SpeedFactorTarget { get; set; }
        public double FlowFactor { get; set; }
        public double FlowFactorTarget { get; set; }
        public long NumberOfToolHeads { get; set; }
        public long ActiveToolHead { get; set; }
        public bool HasHeatedBed { get; set; }
        public bool HasFan { get; set; }
        public bool HasHeatedChamber { get; set; }

        #endregion

        #region ToolHead
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool XHomed { get; set; }
        public bool YHomed { get; set; }
        public bool ZHomed { get; set; }
        public int Layer { get; set; }
        public int Layers { get; set; }
        #endregion

        #region WebCam
        public bool HasWebCam { get; set; }

        #endregion

        #region Printer

        public IPrinter3d ActivePrinter { get; set; }

        #endregion

        #region WebSocket
        public string PingCommand { get; set; }
        public string WebSocketTargetUri { get; set; }
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
        #endregion

        #region Methods
        public Task<bool> SendGcodeAsync(string command, object? data = null);

        public Task<bool> HomeAsync(bool x, bool y, bool z);

        public Task<bool> StartJobAsync(IPrint3dJob job, string command, object? data = null);
        public Task<bool> RemoveJobAsync(IPrint3dJob job, string command, object? data = null);
        public Task<bool> ContinueJobAsync(string command, object? data = null);
        public Task<bool> PauseJobAsync(string command, object? data = null);
        public Task<bool> StopJobAsync(string command, object? data = null);

        public void CancelCurrentRequests();
        #endregion
    }
}

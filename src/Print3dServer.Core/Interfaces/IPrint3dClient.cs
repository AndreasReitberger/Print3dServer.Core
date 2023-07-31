using AndreasReitberger.API.Print3dServer.Core.Enums;
using System.Collections.Concurrent;
using System.Security;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dClient : IPrint3dBase
    {
        #region Properties

        #region General
        public Print3dServerTarget Target { get; set; }
        #endregion

        #region Connection
        public string SessionId { get; set; }
        public string HostName { get; set; }
        public string Host { get; set; }
        public string ApiKey { get; set; }
        public int Port { get; set; }
        public int DefaultTimeout { get; set; }
        public int RetriesWhenOffline { get; set; }
        public bool IsSecure { get; set; }
        #endregion

        #region States
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public bool IsConnecting { get; set; }
        public bool IsRefreshing { get; set; }
        public bool IsInitialized { get; set; }
        public bool IsListening { get; set; }
        public bool InitialDataFetched { get; set; }
        public bool AuthenticationFailed { get; set; }
        public bool UpdateAvailable { get; set; }
        #endregion

        #region Api

        public string ApiVersion { get; set; }

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
        public int NumberOfToolHeads { get; set; }
        public int ActiveToolHead { get; set; }
        public bool HasHeatedBed { get; set; }
        public bool HasFan { get; set; }
        public bool HasHeatedChamber { get; set; }

        #endregion

        #region ToolHead
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int Layer { get; set; }
        public int Layers { get; set; }
        #endregion

        #region Data Convertion

        public ConcurrentDictionary<string, string> IgnoredJsonResults { get;set; }


        #endregion

        #region Instance
        public static IPrint3dClient Instance { get; set; }
        #endregion

        #endregion

        #region Collections
        public IObservable<IPrinter3d> Printers { get; set; }
        public IObservable<IGcode> Files { get; set; }
        public IObservable<IPrint3dJob> Jobs { get; set; }
        #endregion

        #region Methods

        public Task RefreshAsync();
        public Task<bool> CheckOnlineAsync(int timeout);

        public Task<IGcode> GetFilesAsync();
        public Task<IPrinter3d> GetPrintersAsync();
        public Task<IPrint3dJob> GetJobQueueAsync();
        #endregion
    }
}

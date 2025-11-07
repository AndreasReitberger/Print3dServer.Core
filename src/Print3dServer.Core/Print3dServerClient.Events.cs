using AndreasReitberger.API.Print3dServer.Core.Events;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Event Handlers

        #region Debug
        public event EventHandler<IgnoredJsonResultsChangedEventArgs>? IgnoredJsonResultsChanged;
        protected virtual void OnIgnoredJsonResultsChanged(IgnoredJsonResultsChangedEventArgs e)
        {
            IgnoredJsonResultsChanged?.Invoke(this, e);
        }
        #endregion

        #region ServerConnectionState

        public event EventHandler<Print3dBaseEventArgs>? ServerWentOffline;
        protected virtual void OnServerWentOffline(Print3dBaseEventArgs e)
        {
            ServerWentOffline?.Invoke(this, e);
        }

        public event EventHandler<Print3dBaseEventArgs>? ServerWentOnline;
        protected virtual void OnServerWentOnline(Print3dBaseEventArgs e)
        {
            ServerWentOnline?.Invoke(this, e);
        }

        public event EventHandler<Print3dBaseEventArgs>? ServerUpdateAvailable;
        protected virtual void OnServerUpdateAvailable(Print3dBaseEventArgs e)
        {
            ServerUpdateAvailable?.Invoke(this, e);
        }
        #endregion

        #region Errors

        public new event EventHandler? Error;
        protected virtual void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        #endregion

        #region ServerStateChanges

        public event EventHandler<JobStartedEventArgs>? JobsStarted;
        protected virtual void OnJobStarted(JobStartedEventArgs e)
        {
            JobsStarted?.Invoke(this, e);
        }

        public event EventHandler<JobsChangedEventArgs>? JobsChanged;
        protected virtual void OnJobsChangedEvent(JobsChangedEventArgs e)
        {
            JobsChanged?.Invoke(this, e);
        }

        public event EventHandler<JobFinishedEventArgs>? JobFinished;
        protected virtual void OnJobFinished(JobFinishedEventArgs e)
        {
            JobFinished?.Invoke(this, e);
        }

        public event EventHandler<JobStatusFinishedEventArgs>? JobStatusFinished;
        protected virtual void OnJobStatusFinished(JobStatusFinishedEventArgs e)
        {
            JobStatusFinished?.Invoke(this, e);
        }

        public event EventHandler<TemperatureDataEventArgs>? TemperatureDataReceived;
        protected virtual void OnTemperatureDataReceived(TemperatureDataEventArgs e)
        {
            TemperatureDataReceived?.Invoke(this, e);
        }

        public event EventHandler<GcodesChangedEventArgs>? GcodesChanged;
        protected virtual void OnGcodesChangedEvent(GcodesChangedEventArgs e)
        {
            GcodesChanged?.Invoke(this, e);
        }

        public event EventHandler<GcodeGroupsChangedEventArgs>? GcodeGroupsChanged;
        protected virtual void OnGcodeGroupsChangedEvent(GcodeGroupsChangedEventArgs e)
        {
            GcodeGroupsChanged?.Invoke(this, e);
        }

        public event EventHandler<ActivePrinterChangedEventArgs>? ActivePrinterChanged;
        protected virtual void OnActivePrinterChangedEvent(ActivePrinterChangedEventArgs e)
        {
            ActivePrinterChanged?.Invoke(this, e);
        }

        public event EventHandler<IsPrintingStateChangedEventArgs>? IsPrintingStateChanged;
        protected virtual void OnIsPrintingStateChanged(IsPrintingStateChangedEventArgs e)
        {
            IsPrintingStateChanged?.Invoke(this, e);
        }
        #endregion

        #region Jobs & Queue
        public event EventHandler<ActivePrintImageChangedEventArgs>? ActivePrintImageChanged;
        protected virtual void OnActivePrintImageChanged(ActivePrintImageChangedEventArgs e)
        {
            ActivePrintImageChanged?.Invoke(this, e);
        }

        public event EventHandler<JobListChangedEventArgs>? JobListChanged;
        protected virtual void OnJobListChangedEvent(JobListChangedEventArgs e)
        {
            JobListChanged?.Invoke(this, e);
        }

        public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
        protected virtual void OnJobStatusChangedEvent(JobStatusChangedEventArgs e)
        {
            JobStatusChanged?.Invoke(this, e);
        }
        #endregion

        #region Printers
        public event EventHandler<PrintersChangedEventArgs>? RemotePrintersChanged;
        protected virtual void OnRemotePrintersChanged(PrintersChangedEventArgs e)
        {
            RemotePrintersChanged?.Invoke(this, e);
        }
        #endregion

        #region WebCams
        public event EventHandler<WebCamConfigChangedEventArgs>? WebCamConfigChanged;
        protected virtual void OnWebCamConfigChanged(WebCamConfigChangedEventArgs e)
        {
            WebCamConfigChanged?.Invoke(this, e);
        }

        public event EventHandler<WebCamConfigsChangedEventArgs>? WebCamConfigsChanged;
        protected virtual void OnWebCamConfigsChanged(WebCamConfigsChangedEventArgs e)
        {
            WebCamConfigsChanged?.Invoke(this, e);
        }

        #endregion

        #region Heaters
        public event EventHandler<HeaterChangedEventArgs>? HeaterChanged;
        protected virtual void OnHeaterChangedEvent(HeaterChangedEventArgs e)
        {
            HeaterChanged?.Invoke(this, e);
        }

        public event EventHandler<HeatersChangedEventArgs>? HeatersChanged;
        protected virtual void OnHeatersChangedEvent(HeatersChangedEventArgs e)
        {
            HeatersChanged?.Invoke(this, e);
        }

        #endregion

        #region Toolheads
        public event EventHandler<ToolheadChangedEventArgs>? ToolheadChanged;
        protected virtual void OnToolheadChangedEvent(ToolheadChangedEventArgs e)
        {
            ToolheadChanged?.Invoke(this, e);
        }

        public event EventHandler<ToolheadsChangedEventArgs>? ToolheadsChanged;
        protected virtual void OnToolheadsChangedEvent(ToolheadsChangedEventArgs e)
        {
            ToolheadsChanged?.Invoke(this, e);
        }

        #endregion

        #region Fans
        public event EventHandler<FanChangedEventArgs>? FanChanged;
        protected virtual void OnFanChangedEvent(FanChangedEventArgs e)
        {
            FanChanged?.Invoke(this, e);
        }

        public event EventHandler<FansChangedEventArgs>? FansChanged;
        protected virtual void OnFansChangedEvent(FansChangedEventArgs e)
        {
            FansChanged?.Invoke(this, e);
        }

        #endregion

        #region Sensors
        public event EventHandler<SensorsChangedEventArgs>? SensorsChanged;
        protected virtual void OnSensorsChangedEvent(SensorsChangedEventArgs e)
        {
            SensorsChanged?.Invoke(this, e);
        }

        #endregion

        #endregion
    }
}

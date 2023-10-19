using AndreasReitberger.API.Print3dServer.Core.Events;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region EventHandlerss

        #region Debug
        public event EventHandler<IgnoredJsonResultsChangedEventArgs> IgnoredJsonResultsChanged;
        protected virtual void OnIgnoredJsonResultsChanged(IgnoredJsonResultsChangedEventArgs e)
        {
            IgnoredJsonResultsChanged?.Invoke(this, e);
        }
        #endregion

        #region WebSocket

        public event EventHandler<Print3dBaseEventArgs> WebSocketConnected;
        protected virtual void OnWebSocketConnected(Print3dBaseEventArgs e)
        {
            WebSocketConnected?.Invoke(this, e);
        }

        public event EventHandler<Print3dBaseEventArgs> WebSocketDisconnected;
        protected virtual void OnWebSocketDisconnected(Print3dBaseEventArgs e)
        {
            WebSocketDisconnected?.Invoke(this, e);
        }

        public event EventHandler<ErrorEventArgs> WebSocketError;
        protected virtual void OnWebSocketError(ErrorEventArgs e)
        {
            WebSocketError?.Invoke(this, e);
        }

        public event EventHandler<WebsocketEventArgs> WebSocketMessageReceived;
        protected virtual void OnWebSocketMessageReceived(WebsocketEventArgs e)
        {
            WebSocketMessageReceived?.Invoke(this, e);
        }

        public event EventHandler<WebsocketEventArgs> WebSocketDataReceived;
        protected virtual void OnWebSocketDataReceived(WebsocketEventArgs e)
        {
            WebSocketDataReceived?.Invoke(this, e);
        }

        public event EventHandler<LoginRequiredEventArgs> LoginResultReceived;
        protected virtual void OnLoginResultReceived(LoginRequiredEventArgs e)
        {
            LoginResultReceived?.Invoke(this, e);
        }

        #endregion

        #region ServerConnectionState

        public event EventHandler<Print3dBaseEventArgs> ServerWentOffline;
        protected virtual void OnServerWentOffline(Print3dBaseEventArgs e)
        {
            ServerWentOffline?.Invoke(this, e);
        }

        public event EventHandler<Print3dBaseEventArgs> ServerWentOnline;
        protected virtual void OnServerWentOnline(Print3dBaseEventArgs e)
        {
            ServerWentOnline?.Invoke(this, e);
        }

        public event EventHandler<Print3dBaseEventArgs> ServerUpdateAvailable;
        protected virtual void OnServerUpdateAvailable(Print3dBaseEventArgs e)
        {
            ServerUpdateAvailable?.Invoke(this, e);
        }
        #endregion

        #region Errors

        public event EventHandler Error;
        protected virtual void OnError()
        {
            Error?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnError(ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        protected virtual void OnError(UnhandledExceptionEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        protected virtual void OnError(JsonConvertEventArgs e)
        {
            Error?.Invoke(this, e);
        }
        public event EventHandler<RestEventArgs> RestApiError;
        protected virtual void OnRestApiError(RestEventArgs e)
        {
            RestApiError?.Invoke(this, e);
        }

        public event EventHandler<RestEventArgs> RestApiAuthenticationError;
        protected virtual void OnRestApiAuthenticationError(RestEventArgs e)
        {
            RestApiAuthenticationError?.Invoke(this, e);
        }
        public event EventHandler<RestEventArgs> RestApiAuthenticationSucceeded;
        protected virtual void OnRestApiAuthenticationSucceeded(RestEventArgs e)
        {
            RestApiAuthenticationSucceeded?.Invoke(this, e);
        }

        public event EventHandler<JsonConvertEventArgs> RestJsonConvertError;
        protected virtual void OnRestJsonConvertError(JsonConvertEventArgs e)
        {
            RestJsonConvertError?.Invoke(this, e);
        }

        #endregion

        #region ServerStateChanges

        public event EventHandler<ListeningChangedEventArgs> ListeningChanged;
        protected virtual void OnListeningChangedEvent(ListeningChangedEventArgs e)
        {
            ListeningChanged?.Invoke(this, e);
        }

        public event EventHandler<SessionChangedEventArgs> SessionChanged;
        protected virtual void OnSessionChangedEvent(SessionChangedEventArgs e)
        {
            SessionChanged?.Invoke(this, e);
        }

        public event EventHandler<JobStartedEventArgs> JobsStarted;
        protected virtual void OnJobStarted(JobStartedEventArgs e)
        {
            JobsStarted?.Invoke(this, e);
        }

        public event EventHandler<JobsChangedEventArgs> JobsChanged;
        protected virtual void OnJobsChangedEvent(JobsChangedEventArgs e)
        {
            JobsChanged?.Invoke(this, e);
        }

        public event EventHandler<JobFinishedEventArgs> JobFinished;
        protected virtual void OnJobFinished(JobFinishedEventArgs e)
        {
            JobFinished?.Invoke(this, e);
        }

        public event EventHandler<JobStatusFinishedEventArgs> JobStatusFinished;
        protected virtual void OnJobStatusFinished(JobStatusFinishedEventArgs e)
        {
            JobStatusFinished?.Invoke(this, e);
        }

        public event EventHandler<TemperatureDataEventArgs> TemperatureDataReceived;
        protected virtual void OnTemperatureDataReceived(TemperatureDataEventArgs e)
        {
            TemperatureDataReceived?.Invoke(this, e);
        }

        public event EventHandler<GcodesChangedEventArgs> GcodesChanged;
        protected virtual void OnGcodesChangedEvent(GcodesChangedEventArgs e)
        {
            GcodesChanged?.Invoke(this, e);
        }

        public event EventHandler<GcodeGroupsChangedEventArgs> GcodeGroupsChanged;
        protected virtual void OnGcodeGroupsChangedEvent(GcodeGroupsChangedEventArgs e)
        {
            GcodeGroupsChanged?.Invoke(this, e);
        }

        public event EventHandler<ActivePrinterChangedEventArgs> ActivePrinterChanged;
        protected virtual void OnActivePrinterChangedEvent(ActivePrinterChangedEventArgs e)
        {
            ActivePrinterChanged?.Invoke(this, e);
        }

        #endregion

        #region Jobs & Queue
        public event EventHandler<ActivePrintImageChangedEventArgs> ActivePrintImageChanged;
        protected virtual void OnActivePrintImageChanged(ActivePrintImageChangedEventArgs e)
        {
            ActivePrintImageChanged?.Invoke(this, e);
        }

        public event EventHandler<JobListChangedEventArgs> JobListChanged;
        protected virtual void OnJobListChangedEvent(JobListChangedEventArgs e)
        {
            JobListChanged?.Invoke(this, e);
        }
        #endregion

        #region Printers
        public event EventHandler<PrintersChangedEventArgs> RemotePrintersChanged;
        protected virtual void OnRemotePrintersChanged(PrintersChangedEventArgs e)
        {
            RemotePrintersChanged?.Invoke(this, e);
        }
        #endregion

        #endregion
    }
}

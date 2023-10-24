using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IWebCamConfig selectedWebCam;
        partial void OnSelectedWebCamChanged(IWebCamConfig value)
        {
            OnWebCamConfigChanged(new WebCamConfigChangedEventArgs()
            {
                NewConfig = value,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IWebCamConfig> webCams = new();
        partial void OnWebCamsChanged(ObservableCollection<IWebCamConfig> value)
        {
            OnWebCamConfigsChanged(new WebCamConfigsChangedEventArgs()
            {
                NewConfigs = value,
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string webCamTargetUri;

        [ObservableProperty]
        string webCamTarget = "/webcam/?action=stream";
        partial void OnWebCamTargetChanged(string value)
        {
            WebCamTargetUri = GetDefaultWebCamUri();
        }

        [ObservableProperty]
        int webCamIndex = 0;
        partial void OnWebCamIndexChanged(int value)
        {
            WebCamTargetUri = GetDefaultWebCamUri();
        }

        [ObservableProperty]
        string webCamMultiCamTarget = "?cam=";
        partial void OnWebCamMultiCamTargetChanged(string value)
        {
            WebCamTargetUri = GetDefaultWebCamUri();
        }
        #endregion

        #region Methods

        public abstract Task<ObservableCollection<IWebCamConfig>> GetWebCamConfigsAsync();

        public virtual async Task<ObservableCollection<IWebCamConfig>> GetWebCamConfigsAsync(string command, object? data = null, string? targetUri = null)
        {
            IRestApiRequestRespone? result = null;
            ObservableCollection<IWebCamConfig> resultObject = new();
            try
            {
                result = await SendRestApiRequestAsync(
                       requestTargetUri: targetUri,
                       method: Method.Post,
                       command: command,
                       jsonObject: data,
                       authHeaders: AuthHeaders,
                       //urlSegments: urlSegments,
                       cts: default
                       )
                    .ConfigureAwait(false);
                return GetObjectFromJson<ObservableCollection<IWebCamConfig>>(result?.Result, NewtonsoftJsonSerializerSettings) ?? new();
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result?.Result,
                    TargetType = nameof(ObservableCollection<IWebCamConfig>),
                    Message = jecx.Message,
                });
                return resultObject;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return resultObject;
            }
        }

        public virtual string GetDefaultWebCamUri()
        {
            try
            {
                string baseStream = $"{FullWebAddress}{WebCamTarget}";
                string? token;
                switch (Target)
                {
                    case Print3dServerTarget.OctoPrint:
                    case Print3dServerTarget.Moonraker:
                        token = GetAuthHeader("apikey")?.Token ?? GetAuthHeader("usertoken")?.Token;
                        if (!string.IsNullOrEmpty(token))
                            baseStream += $"?t={token}";
                        break;
                    case Print3dServerTarget.RepetierServer:
                        // $"{FullWebAddress}/printer/{(type == RepetierWebcamType.Dynamic ? "cammjpg" : "camjpg")}/{currentPrinter}?cam={camIndex}&apikey={ApiKey}";
                        baseStream += $"{GetActivePrinterSlug()}{WebCamMultiCamTarget}{WebCamIndex}"; 
                        token = GetAuthHeader("apikey")?.Token;
                        if (!string.IsNullOrEmpty(token))
                            baseStream += $"&apikey={token}";                    
                        break;
                    case Print3dServerTarget.PrusaConnect:
                    case Print3dServerTarget.Custom:
                    default:
                        break;
                }
                return baseStream;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return "";
            }
        }

        public virtual string GetWebCamUri(IWebCamConfig? config)
        {
            try
            {
                string baseStream = $"{FullWebAddress}{WebCamTarget}";
                string? token;
                switch (Target)
                {
                    case Print3dServerTarget.OctoPrint:
                    case Print3dServerTarget.Moonraker:
                        baseStream += config?.WebCamUrlDynamic;
                        token = GetAuthHeader("apikey")?.Token ?? GetAuthHeader("usertoken")?.Token;
                        if (!string.IsNullOrEmpty(token))
                            baseStream += $"?t={token}";
                        break;
                    case Print3dServerTarget.RepetierServer:
                        baseStream += $"{GetActivePrinterSlug()}{WebCamMultiCamTarget}{config?.Position ?? 0}";
                        token = GetAuthHeader("apikey")?.Token;
                        if (!string.IsNullOrEmpty(token))
                            baseStream += $"&apikey={token}";
                        break;
                    case Print3dServerTarget.PrusaConnect:
                    case Print3dServerTarget.Custom:
                        break;
                    default:
                        break;
                }
                return baseStream;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return "";
            }
        }

        public virtual async Task<string> GetWebCamUriAsync(int index = 0, bool refreshWebCamConfig = false)
        {
            try
            {
                if (WebCams?.Count <= 0 || refreshWebCamConfig)
                {
                    await GetWebCamConfigsAsync().ConfigureAwait(false);
                }
                IWebCamConfig? config = null;
                if (WebCams?.Count > index)
                {
                    config = WebCams[index];
                }
                else if (WebCams?.Count > 0)
                {
                    config = WebCams?.FirstOrDefault();
                }
                // If nothing is found, try the default setup
                else
                {
                    return GetDefaultWebCamUri();
                }
                return GetWebCamUri(config);
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return "";
            }
        }
        #endregion
    }
}

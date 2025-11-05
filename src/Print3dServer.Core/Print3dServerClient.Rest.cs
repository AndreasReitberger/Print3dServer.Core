using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.REST;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient : IPrint3dServerClient
    {
        #region Methods

        #region Download
        public virtual async Task<byte[]?> DownloadFileFromUriAsync(
            string path,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            Dictionary<string, string>? urlSegments = null,
            int timeout = 10000
            )
        {
            try
            {
                if (RestClient is null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(path);
                if (authHeaders?.Count > 0)
                {
                    switch (Target)
                    {
                        // Special handling for Repetier Server
                        case Print3dServerTarget.RepetierServer:
                            string? key = authHeaders?.FirstOrDefault(x => x.Key == "apikey").Value?.Token;
                            if (key is not null)
                                request.AddParameter("apikey", key, ParameterType.QueryString);
                            break;
                        case Print3dServerTarget.Moonraker:
                            string? apiKey = authHeaders?.FirstOrDefault(x => x.Key == "apikey").Value?.Token;
                            if (apiKey is not null)
                            {
                                request.AddHeader("X-Api-Key", $"{apiKey}");
                            }
                            else if (authHeaders?.ContainsKey("oneshottoken") is true)
                            {
                                apiKey = authHeaders?.FirstOrDefault(x => x.Key == "oneshottoken").Value?.Token;
                                if (apiKey is not null)
                                    request.AddHeader("Authorization", $"Bearer {SessionId}");
                            }
                            else if (!string.IsNullOrEmpty(SessionId))
                            {
                                request.AddHeader("Authorization", $"Bearer {SessionId}");
                            }
                            break;
                        case Print3dServerTarget.OctoPrint:
                            string? octoKey = authHeaders?.FirstOrDefault(x => x.Key == "apikey").Value?.Token;
                            if (octoKey is not null)
                                request.AddHeader("X-Api-Key", octoKey);
                            break;
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

                request.RequestFormat = DataFormat.Json;
                request.Method = Method.Get;
                request.Timeout = TimeSpan.FromMilliseconds(timeout);
                if (urlSegments?.Count > 0)
                {
                    foreach (KeyValuePair<string, string> segment in urlSegments)
                    {
                        request.AddParameter(segment.Key, segment.Value, ParameterType.QueryString);
                    }
                }

                Uri? fullUrl = RestClient?.BuildUri(request);
                CancellationTokenSource cts = new(timeout);
                if (RestClient is not null)
                {
                    byte[]? respone = await RestClient.DownloadDataAsync(request, cts.Token)
                        .ConfigureAwait(false);
                    return respone;
                }
                else return null;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return null;
            }
        }
        #endregion

        #endregion
    }
}

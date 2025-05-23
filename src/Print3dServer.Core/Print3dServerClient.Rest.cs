﻿using AndreasReitberger.API.Print3dServer.Core.Enums;
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

        #region Rest Api
        [Obsolete("Use the common RestApiClient.SendRestApiRequestAsync instead")]
        public virtual async Task<IRestApiRequestRespone?> SendRestApiRequestLegacyAsync(
            string? requestTargetUri,
            Method method,
            string? command,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            object? jsonObject = null,
            CancellationTokenSource? cts = default,
            Dictionary<string, string>? urlSegments = null
            )
        {
            RestApiRequestRespone apiRsponeResult = new() { IsOnline = IsOnline };
            try
            {
                cts ??= new(DefaultTimeout);
                requestTargetUri ??= string.Empty;
                command ??= string.Empty;
                if (RestClient == null)
                {
                    UpdateRestClientInstance();
                }
                RestRequest request = new(!string.IsNullOrEmpty(command) ? $"{requestTargetUri}/{command}" : requestTargetUri)
                {
                    RequestFormat = DataFormat.Json,
                    Method = method
                };
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
                                    //request.AddHeader("Authorization", $"Bearer {SessionId}");
                                    request.AddHeader("Authorization", $"Bearer {apiKey}");
                            }
                            else if (authHeaders?.ContainsKey("usertoken") is true)
                            {
                                apiKey = authHeaders?.FirstOrDefault(x => x.Key == "usertoken").Value?.Token;
                                if (apiKey is not null)
                                    //request.AddHeader("Authorization", $"Bearer {SessionId}");
                                    request.AddHeader("Authorization", $"Bearer {apiKey}");
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
                switch (Target)
                {
                    case Print3dServerTarget.RepetierServer:
                        if (string.IsNullOrEmpty(command)) break;
                        urlSegments ??= [];
                        urlSegments.Add("a", command);
                        if (jsonObject is not null)
                        {
                            string jsonDataString = "";
                            if (jsonObject is string jsonString)
                            {
                                jsonDataString = jsonString;
                            }
                            else
                            {
                                jsonDataString = JsonConvert.SerializeObject(jsonObject);
                            }

                            request.AddParameter("data", jsonDataString, ParameterType.QueryString);
                        }
                        break;
                    case Print3dServerTarget.Moonraker:
                    case Print3dServerTarget.OctoPrint:
                    case Print3dServerTarget.PrusaConnect:
                    case Print3dServerTarget.Custom:
                    default:
                        if (jsonObject is not null)
                        {
                            request.AddJsonBody(jsonObject, "application/json");
                        }
                        break;
                }
                if (urlSegments != null)
                {
                    foreach (KeyValuePair<string, string> pair in urlSegments)
                    {
                        request.AddParameter(pair.Key, pair.Value, ParameterType.QueryString);
                    }
                }

                Uri? fullUri = RestClient?.BuildUri(request);
                try
                {
                    if (RestClient is not null)
                    {
                        RestResponse? respone = await RestClient.ExecuteAsync(request, cts.Token).ConfigureAwait(false);
#if DEBUG
                        Debug.WriteLine($"REST: Result = '{(respone?.IsSuccessful is true ? "successfully" : "failed")} (Code: {respone?.StatusCode})'\n{respone?.Content}");
#endif
                        if (ValidateResponse(respone, fullUri) is RestApiRequestRespone res)
                            apiRsponeResult = res;
                    }
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

        [Obsolete("Use the common RestApiClient.SendMultipartFormDataFileRestApiRequestAsync instead")]
        public virtual async Task<IRestApiRequestRespone?> SendMultipartFormDataFileRestApiRequestLegacyAsync(
            string requestTargetUri,
            Dictionary<string, IAuthenticationHeader> authHeaders,
            string? fileName = null,
            byte[]? file = null,
            Dictionary<string, string>? parameters = null,
            string? localFilePath = null,
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
                // If there is no file specified
                if (file is null && localFilePath is null)
                    // and there are no additional parameters supplied, throw
                    if (parameters?.Count == 0)
                        throw new ArgumentNullException(
                            nameof(parameters),
                            $"No file, localFilePath and paramaters have been provided! Set at least one of those three parameters!\n" +
                            $"{nameof(SendMultipartFormDataFileRestApiRequestLegacyAsync)}: {nameof(file)} / {nameof(localFilePath)} / {nameof(parameters)}");
                if (RestClient is null)
                {
                    UpdateRestClientInstance();
                }
                CancellationTokenSource cts = new(new TimeSpan(0, 0, 0, 0, timeout));
                RestRequest request = new(requestTargetUri);

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
                request.Method = Method.Post;
                request.AlwaysMultipartFormData = true;

                //Multiform
                request.AddHeader("Content-Type", contentType ?? "multipart/form-data");
                if (file is not null && !string.IsNullOrEmpty(fileName))
                {
                    request.AddFile(fileTargetName ?? "file", file, fileName, fileContentType ?? "application/octet-stream");
                }
                else if (localFilePath is not null)
                {
                    request.AddFile(fileTargetName ?? "file", localFilePath, fileContentType ?? "application/octet-stream");
                }

                if (parameters?.Count > 0)
                {
                    foreach (var para in parameters)
                    {
                        request.AddParameter(para.Key, para.Value, ParameterType.GetOrPost);
                    }
                }
                Uri? fullUri = RestClient?.BuildUri(request);
                try
                {
                    if (RestClient is not null)
                    {
                        RestResponse respone = await RestClient.ExecuteAsync(request, cts.Token);
                        if (ValidateResponse(respone, fullUri) is RestApiRequestRespone res)
                            apiRsponeResult = res;
                    }
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

using AndreasReitberger.API.Print3dServer.Core;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.REST.Events;
using AndreasReitberger.API.REST.Interfaces;
using Newtonsoft.Json;
using RestSharp;

namespace Print3dServer.Core.Test
{
    public partial class MyOwnPrintServerClient : Print3dServerClient
    {
        #region Override Methods

        public override async Task<List<IPrinter3d>> GetPrintersAsync()
        {
            IRestApiRequestRespone? result = null;
            try
            {
                List<IPrinter3d> printers = [];
                if (!IsReady)
                    return printers;

                string targetUri = $"/printer/list";
                result = await SendRestApiRequestAsync(
                       requestTargetUri: targetUri,
                       method: Method.Post,
                       command: "",
                       jsonObject: null,
                       authHeaders: AuthHeaders
                       )
                    .ConfigureAwait(false);
                /*
                 * Data conversion not covered yet.
                 */
                return printers;
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result?.Result,
                    TargetType = nameof(GetPrintersAsync),
                    Message = jecx.Message,
                });
                return [];
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return [];
            }
        }

        public override async Task<List<IGcode>> GetFilesAsync()
        {
            IRestApiRequestRespone? result = null;
            try
            {
                List<IGcode> files = [];
                if (!IsReady)
                    return files;

                string currentPrinter = GetActivePrinterSlug();
                if (string.IsNullOrEmpty(currentPrinter)) return files;

                string targetUri = $"/printer/api/{currentPrinter}";
                result = await SendRestApiRequestAsync(
                       requestTargetUri: targetUri,
                       method: Method.Post,
                       command: "",
                       jsonObject: null,
                       authHeaders: AuthHeaders
                       )
                    .ConfigureAwait(false);
                /*
                 * Data conversion not covered yet.
                 */
                return files;
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result?.Result,
                    TargetType = nameof(GetPrintersAsync),
                    Message = jecx.Message,
                });
                return [];
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return [];
            }
        }

        public override async Task<List<IGcodeGroup>> GetModelGroupsAsync(string path = "")
        {
            IRestApiRequestRespone? result = null;
            List<IGcodeGroup> resultObject = [];

            string currentPrinter = !string.IsNullOrEmpty(path) ? path : GetActivePrinterSlug();
            if (string.IsNullOrEmpty(currentPrinter)) return resultObject;

            try
            {
                string targetUri = $"/printer/api/{currentPrinter}";
                result = await SendRestApiRequestAsync(
                       requestTargetUri: targetUri,
                       method: Method.Post,
                       command: "listModelGroups",
                       jsonObject: null,
                       authHeaders: AuthHeaders
                       )
                    .ConfigureAwait(false);
                /*
                 * Data conversion not covered yet.
                 */
                return resultObject;
            }
            catch (JsonException jecx)
            {
                OnError(new JsonConvertEventArgs()
                {
                    Exception = jecx,
                    OriginalString = result?.Result,
                    TargetType = nameof(String),
                    Message = jecx.Message,
                });
                return [];
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return resultObject;
            }
        }

        public override async Task<List<IWebCamConfig>?> GetWebCamConfigsAsync()
        {
            try
            {
                /*
                 * Data conversion not covered yet.
                 */
                await Task.Delay(10);
                return [];
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return null;
            }
        }

        public override async Task<byte[]?> DownloadFileAsync(string relativeFilePath)
        {
            try
            {
                string uri = $"{ApiTargetPath}/server/files/{relativeFilePath}";
                byte[]? file = await DownloadFileFromUriAsync(uri, AuthHeaders)
                    .ConfigureAwait(false);
                return file;
            }
            catch (Exception exc)
            {
                OnError(new UnhandledExceptionEventArgs(exc, false));
                return null;
            }
        }

        #endregion
    }
}

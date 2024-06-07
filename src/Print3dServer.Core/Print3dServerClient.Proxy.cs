using System.Net;
using System.Security;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {

        #region Properties
        [ObservableProperty]
        bool enableProxy = false;
        partial void OnEnableProxyChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        bool proxyUserUsesDefaultCredentials = true;
        partial void OnProxyUserUsesDefaultCredentialsChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        bool secureProxyConnection = true;
        partial void OnSecureProxyConnectionChanged(bool value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        string proxyAddress = string.Empty;
        partial void OnProxyAddressChanged(string value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        int proxyPort = 443;
        partial void OnProxyPortChanged(int value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        string proxyUser = string.Empty;
        partial void OnProxyUserChanged(string value)
        {
            UpdateRestClientInstance();
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        SecureString? proxyPassword;
        partial void OnProxyPasswordChanged(SecureString? value)
        {
            UpdateRestClientInstance();
        }
        #endregion

        #region Methods

        public virtual void SetProxy(bool secure, string address, int port, bool enable = true)
        {
            EnableProxy = enable;
            ProxyUserUsesDefaultCredentials = true;
            ProxyAddress = address;
            ProxyPort = port;
            ProxyUser = string.Empty;
            ProxyPassword = null;
            SecureProxyConnection = secure;
            UpdateRestClientInstance();
        }

        public virtual void SetProxy(bool secure, string address, int port, string user = "", SecureString? password = null, bool enable = true)
        {
            EnableProxy = enable;
            ProxyUserUsesDefaultCredentials = false;
            ProxyAddress = address;
            ProxyPort = port;
            ProxyUser = user;
            ProxyPassword = password;
            SecureProxyConnection = secure;
            UpdateRestClientInstance();
        }

        public virtual Uri GetProxyUri() => ProxyAddress.StartsWith("http://") || ProxyAddress.StartsWith("https://") ? new Uri($"{ProxyAddress}:{ProxyPort}") : new Uri($"{(SecureProxyConnection ? "https" : "http")}://{ProxyAddress}:{ProxyPort}");

        public virtual WebProxy GetCurrentProxy()
        {
            WebProxy proxy = new()
            {
                Address = GetProxyUri(),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = ProxyUserUsesDefaultCredentials,
            };
            if (ProxyUserUsesDefaultCredentials && !string.IsNullOrEmpty(ProxyUser))
            {
                proxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
            }
            else
            {
                proxy.UseDefaultCredentials = ProxyUserUsesDefaultCredentials;
            }
            return proxy;
        }
        public virtual void UpdateRestClientInstance()
        {
            if (string.IsNullOrEmpty(ServerAddress))
            {
                return;
            }
            if (EnableProxy && !string.IsNullOrEmpty(ProxyAddress))
            {
                RestClientOptions options = new(FullWebAddress)
                {
                    ThrowOnAnyError = true,
                    Timeout = TimeSpan.FromMilliseconds(10000),
                };
                HttpClientHandler httpHandler = new()
                {
                    UseProxy = true,
                    Proxy = GetCurrentProxy(),
                    AllowAutoRedirect = true,
                };

                httpClient = new(handler: httpHandler, disposeHandler: true);
                restClient = new(httpClient: httpClient, options: options);
            }
            else
            {
                httpClient = null;
                restClient = new(baseUrl: FullWebAddress);
            }
        }

        #endregion
    }
}

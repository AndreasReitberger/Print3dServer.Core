using AndreasReitberger.API.Print3dServer.Core.Events;
using System.Security;

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
        SecureString? proxyPassword;
        partial void OnProxyPasswordChanged(SecureString? value)
        {
            UpdateRestClientInstance();
        }
        #endregion

        #region Methods

        public void SetProxy(bool secure, string address, int port, bool enable = true)
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

        public void SetProxy(bool secure, string address, int port, string user = "", SecureString? password = null, bool enable = true)
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

        #endregion
    }
}

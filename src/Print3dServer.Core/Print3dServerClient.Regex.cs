#if NET6_0_OR_GREATER && false
using AndreasReitberger.Shared.Core.Utilities;
using System.Text.RegularExpressions;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Regex
        [GeneratedRegex(RegexHelper.IPv4AddressRegex)]
        public static partial Regex R_IPv4Address();

        [GeneratedRegex(RegexHelper.IPv6AddressRegex)]
        public static partial Regex R_IPv6Address();

        [GeneratedRegex(RegexHelper.Fqdn)]
        public static partial Regex R_Fqdn();
        #endregion
    }
}
#endif
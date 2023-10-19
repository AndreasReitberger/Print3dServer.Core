using System.Text;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Static
        public static string ConvertStackToPath(Stack<string> stack, string separator)
        {
            StringBuilder sb = new();
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                sb.Append(stack.ElementAt(i));
                if (i > 0)
                    sb.Append(separator);
            }
            return sb.ToString();
        }

        #endregion
    }
}

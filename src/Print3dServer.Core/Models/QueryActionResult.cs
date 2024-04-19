using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class QueryActionResult : ObservableObject, IQueryActionResult
    {
        #region Properties
        [ObservableProperty, JsonIgnore]
        [property: JsonProperty("ok")]
        bool ok;
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        
        #endregion
    }
}

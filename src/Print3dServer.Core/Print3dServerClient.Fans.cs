using AndreasReitberger.API.Print3dServer.Core.Enums;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        string activeFanIndex = string.Empty;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        int numberOfFans = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasFan = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IPrint3dFan? activeFan;
        partial void OnActiveFanChanged(IPrint3dFan? value)
        {
            OnFanChangedEvent(new()
            {
                Name = ActiveFanIndex,
                Fan = value,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<string, IPrint3dFan> fans = [];
        partial void OnFansChanged(ConcurrentDictionary<string, IPrint3dFan> value)
        {
            OnFansChangedEvent(new()
            {
                Fans = value,
                Printer = GetActivePrinterSlug(),
            });
            ActiveFan = value?.ContainsKey(ActiveFanIndex) is true ? 
                value?[ActiveFanIndex] : value?.FirstOrDefault().Value;
        }

        #endregion
    }
}

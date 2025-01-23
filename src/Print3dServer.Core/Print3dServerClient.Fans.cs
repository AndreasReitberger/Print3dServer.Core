using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial string ActiveFanIndex { get; set; } = string.Empty;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int NumberOfFans { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool HasFan { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IPrint3dFan? ActiveFan { get; set; }
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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<string, IPrint3dFan> Fans { get; set; } = [];
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

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
        public partial int NumberOfSensors { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<string, ISensorComponent> Sensors { get; set; } = [];
        partial void OnSensorsChanged(ConcurrentDictionary<string, ISensorComponent> value)
        {
            OnSensorsChangedEvent(new()
            {
                Sensors = value,
                Printer = GetActivePrinterSlug(),
            });
        }

        #endregion
    }
}

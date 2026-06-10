using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace AndreasReitberger.API.Print3dServer.Core
{
    public partial class Print3dServerClient
    {
        #region Properties

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
        public partial int NumberOfSensors { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, XmlIgnore]
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

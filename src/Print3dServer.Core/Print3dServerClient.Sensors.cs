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
        int numberOfSensors = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<string, ISensorComponent> sensors = [];
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

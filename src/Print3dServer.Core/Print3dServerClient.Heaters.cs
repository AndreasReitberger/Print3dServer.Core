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
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int ActiveToolheadIndex { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial int NumberOfToolHeads { get; set; } = 0;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool IsMultiExtruder { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool HasHeatedBed { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial bool HasHeatedChamber { get; set; } = false;

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IToolhead? ActiveToolhead { get; set; }
        partial void OnActiveToolheadChanged(IToolhead? value)
        {
            OnToolheadChangedEvent(new()
            {
                Type = Printer3dHeaterType.Extruder,
                Toolhead = value,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<int, IToolhead> Toolheads { get; set; } = [];
        partial void OnToolheadsChanged(ConcurrentDictionary<int, IToolhead> value)
        {
            OnToolheadsChangedEvent(new()
            {
                Type = Printer3dHeaterType.Extruder,
                Toolheads = value,
                Printer = GetActivePrinterSlug(),
            });
            ActiveToolhead = value?.ContainsKey(ActiveToolheadIndex) is true ?
                value?[ActiveToolheadIndex] : value?.FirstOrDefault().Value;
        }


        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IHeaterComponent? ActiveHeatedBed { get; set; }
        partial void OnActiveHeatedBedChanged(IHeaterComponent? value)
        {
            OnHeaterChangedEvent(new()
            {
                Type = Printer3dHeaterType.HeatedBed,
                Heater = value,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<int, IHeaterComponent> HeatedBeds { get; set; } = [];
        partial void OnHeatedBedsChanged(ConcurrentDictionary<int, IHeaterComponent> value)
        {
            OnHeatersChangedEvent(new()
            {
                Type = Printer3dHeaterType.HeatedBed,
                Heaters = value,
                Printer = GetActivePrinterSlug(),
            });
            ActiveHeatedBed = value?.FirstOrDefault().Value;
        }


        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial IHeaterComponent? ActiveHeatedChamber { get; set; }
        partial void OnActiveHeatedChamberChanged(IHeaterComponent? value)
        {
            OnHeaterChangedEvent(new()
            {
                Type = Printer3dHeaterType.HeatedChamber,
                Heater = value,
                Printer = GetActivePrinterSlug(),
            });
        }

        [ObservableProperty]
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        public partial ConcurrentDictionary<int, IHeaterComponent> HeatedChambers { get; set; } = [];
        partial void OnHeatedChambersChanged(ConcurrentDictionary<int, IHeaterComponent> value)
        {
            OnHeatersChangedEvent(new()
            {
                Type = Printer3dHeaterType.HeatedChamber,
                Heaters = value,
                Printer = GetActivePrinterSlug(),
            });
            ActiveHeatedChamber = value?.FirstOrDefault().Value;
        }
        #endregion

        #region Methods

        #endregion
    }
}

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
        int activeToolheadIndex = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        int numberOfToolHeads = 0;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool isMultiExtruder = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasHeatedBed = false;

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        bool hasHeatedChamber = false;

        /*
        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ObservableCollection<IHeaterComponent> toolheads = new();
        */

        [ObservableProperty]
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IToolhead? activeToolhead;
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
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<int, IToolhead> toolheads = [];
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
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IHeaterComponent? activeHeatedBed;
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
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<int, IHeaterComponent> heatedBeds = [];
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
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        IHeaterComponent? activeHeatedChamber;
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
        [property: JsonIgnore, System.Text.Json.Serialization.JsonIgnore, XmlIgnore]
        ConcurrentDictionary<int, IHeaterComponent> heatedChambers = [];
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

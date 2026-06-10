using AndreasReitberger.API.Print3dServer.Core.Events;
using AndreasReitberger.API.Print3dServer.Core.Exceptions;
using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using AndreasReitberger.API.Print3dServer.Core.TypeConverters;
using AndreasReitberger.API.REST.SourceGeneration;
using System.Text.Json.Serialization;

namespace AndreasReitberger.API.Print3dServer.Core.SourceGeneration
{
    [JsonSerializable(typeof(ActivePrinterChangedEventArgs))]
    [JsonSerializable(typeof(ActivePrintImageChangedEventArgs))]
    [JsonSerializable(typeof(FanChangedEventArgs))]
    [JsonSerializable(typeof(FansChangedEventArgs))]
    [JsonSerializable(typeof(GcodeGroupsChangedEventArgs))]
    [JsonSerializable(typeof(GcodesChangedEventArgs))]
    [JsonSerializable(typeof(HeaterChangedEventArgs))]
    [JsonSerializable(typeof(HeatersChangedEventArgs))]
    [JsonSerializable(typeof(IgnoredJsonResultsChangedEventArgs))]
    [JsonSerializable(typeof(IsPrintingStateChangedEventArgs))]
    [JsonSerializable(typeof(JobFinishedEventArgs))]
    [JsonSerializable(typeof(JobListChangedEventArgs))]
    [JsonSerializable(typeof(JobsChangedEventArgs))]
    [JsonSerializable(typeof(JobStartedEventArgs))]
    [JsonSerializable(typeof(JobStatusChangedEventArgs))]
    [JsonSerializable(typeof(JobStatusFinishedEventArgs))]
    [JsonSerializable(typeof(Print3dBaseEventArgs))]
    [JsonSerializable(typeof(PrintersChangedEventArgs))]
    [JsonSerializable(typeof(SensorsChangedEventArgs))]
    [JsonSerializable(typeof(TemperatureDataEventArgs))]
    [JsonSerializable(typeof(ToolheadChangedEventArgs))]
    [JsonSerializable(typeof(ToolheadsChangedEventArgs))]
    [JsonSerializable(typeof(WebCamConfigChangedEventArgs))]
    [JsonSerializable(typeof(WebCamConfigsChangedEventArgs))]
    [JsonSerializable(typeof(ServerNotReachableException))]
    [JsonSerializable(typeof(Print3dServerClient))]
    [JsonSerializable(typeof(List<IWebCamConfig>))]
    [JsonSourceGenerationOptions(WriteIndented = true, Converters = [typeof(RegexConverter)])]
    public partial class Print3dCoreSourceGenerationContext : JsonSerializerContext
    {
    }
}

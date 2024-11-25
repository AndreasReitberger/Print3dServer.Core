# Print3dServer.Core
A C# based core library for our 3d Print Server nugets (Moonraker, Repetier Server, OctoPrint and so on)

# Support me
If you want to support me, you can order over following affilate links (I'll get a small share from your purchase from the corresponding store).

- Prusa: http://www.prusa3d.com/#a_aid=AndreasReitberger *
- Jake3D: https://tidd.ly/3x9JOBp * 
- Amazon: https://amzn.to/2Z8PrDu *
- Coinbase: https://advanced.coinbase.com/join/KTKSEBP * (10€ in BTC for you if you open an account)
- TradeRepublic: https://refnocode.trade.re/wfnk80zm * (10€ in stocks for you open an account)

(*) Affiliate link
Thank you very much for supporting me!

# Nuget
[![NuGet](https://img.shields.io/nuget/v/Print3dServer.Core.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Print3dServer.Core/)

## Available print servers
- OctoPrint (https://github.com/AndreasReitberger/OctoPrintRestApiSharp)
- Repetier Server (https://github.com/AndreasReitberger/RepetierServerSharpApi)
- Moonraker (https://github.com/AndreasReitberger/KlipperRestApiSharp)

## Planned
- PrusaConnect

# Usage

## Base Client
In order to create your own `PrintServerClient`, inherit from the base client `Print3dServerClient, IPrintServerClient` as shown below.

```cs
public partial class OctoPrintClient : Print3dServerClient, IPrintServerClient
{
 //...
}
```

The base class holds all needed `Methods` and `Properties` needed to communicate with a print server (via REST API and WebSocket).

## Example migration for OctoPrint
To query data from your server, simply call send the needed `Command`. The result will contain the data as `JsonString`, 
which can be accessed over the `Result` property.

```cs
async Task<OctoPrintFiles> GetFilesAsync(string location, string path = "", bool recursive = true)
{
    try
    {
        string files = string.IsNullOrEmpty(path) ? string.Format("files/{0}", location) : string.Format("files/{0}/{1}", location, path);
        Dictionary<string, string> urlSegments = new()
        {
            //get all files & folders 
            { "recursive", recursive ? "true" : "false" }
        };

        string targetUri = $"{OctoPrintCommands.Api}";
        IRestApiRequestRespone result = await SendRestApiRequestAsync(
               requestTargetUri: targetUri,
               method: Method.Get,
               command: files,
               jsonObject: null,
               authHeaders: AuthHeaders,
               urlSegments: urlSegments,
               cts: default
               )
            .ConfigureAwait(false);
        OctoPrintFiles list = JsonConvert.DeserializeObject<OctoPrintFiles>(result.Result);
        if (list != null)
        {
            FreeDiskSpace = list.Free;
            TotalDiskSpace = list.Total;
        }
        return list;
    }
    catch (Exception exc)
    {
        OnError(new UnhandledExceptionEventArgs(exc, false));
        return new OctoPrintFiles();
    }
}
```

### Time differences
Each print server uses a different time base for `PrintDuration`, `StartTime` and `EndTime`.

| Property              | Core                     | Moonraker              | OctoPrint | Repetier Server      |
|-----------------------| -------------------------|:----------------------:| ----------|---------------------:|
| `PrintDuration`       | `double?` (UNIX Hours)   | `double?` (UNIX Hours) | tbd       | `double?` (Seconds)  |
| `Modified`            | `double?` (UNIX DT)      | `double?` (UNIX DT)    | tbd       | `double?` (Seconds)  |
| `Created`             | `double?` (UNIX DT)      | `double?` (UNIX DT)    | tbd       | `long?` (Seconds)  |

## Converters
Available `Converters` for `XamlBindings`:
https://github.com/AndreasReitberger/SharedMauiCoreLibrary/tree/main/src/SharedMauiCoreLibrary/Converters

Or for code behind, use `TimeBaseConvertHelper`.

```cs
 [ObservableProperty]
 double printTime;
 partial void OnPrintTimeChanged(double value)
 {
     PrintTimeGeneralized = TimeBaseConvertHelper.FromUnixDoubleHours(value);          
 }

[ObservableProperty]
TimeSpan? printTimeGeneralized;
```

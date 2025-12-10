# HyperGuest SDK

[![.github/workflows/main.yml](https://github.com/IngeniumSE/HyperGuestSDK/actions/workflows/main.yml/badge.svg)](https://github.com/IngeniumSE/HyperGuestSDK/actions/workflows/main.yml) [![.github/workflows/release.yml](https://github.com/IngeniumSE/HyperGuestSDK/actions/workflows/release.yml/badge.svg)](https://github.com/IngeniumSE/HyperGuestSDK/actions/workflows/release.yml)

A .NET SDK built for the HyperGuest platform

## Installation

The HyperGuest SDK is available on NuGet. Please install the `HyperGuestSDK` package.

### .NET CLI

```
dotnet add package HyperGuestSDK
```

### Nuget Package Manager

```
Install-Package HyperGuestSDK
```

### Nuget CLI

```
nuget install HyperGuestSDK
```

## Getting started

### Integration with Microsoft.Extensions.DependencyInjection

The HyperGuest SDK can be integrated with the `Microsoft.Extensions.DependencyInjection` library. This allows for easy dependency injection of the HyperGuest SDK services.

```csharp
services.AddHyperGuest();
```

### Manual setup

If you do not wish to use the `Microsoft.Extensions.DependencyInjection` library, you can manually create an instance of the `HyperGuestClient` class.

```csharp
var http = new HttpClient();
var client = new HyperGuestClient(http, new HyperGuestSettings());
```

Or, alternatively, you can use the API client factory:

```csharp
var httpFactory = new HyperGuestHttpClientFactory();
var clientFactory = new HyperGuestApiClientFactory(httpFactory);
var client = clientFactory.CreateClient(new HyperGuestSettings());
```

**NOTE** - On .NET Framework, it is recommended to use a single instance of `HttpClient` for the lifetime of your application. This is because the `HttpClient` class is designed to be reused and not disposed of after each request.

A `IHyperGuestHttpClientFactory` can be implemented to manage the lifecycle of the `HttpClient` instance.

## Terminology

| Term | Description |
|-|-|
| Meeting | A meeting refers to a Grand Prix or testing weekend and usually includes multiple sessions (practice, qualifying, race, ...). |
| Session | A session refers to a distinct period of track activity during a Grand Prix or testing weekend (practice, qualifying, sprint, race, ...). |
| Stint | A stint refers to a period of continuous driving by a driver during a session. |

Other terms such as Driver, Lap, etc are self-explanatory.

## Supported operations

The SDK supports a subset of the available operations in the Open F1 API. The following operations are supported:

The SDK is designed to be extensible, so any calls not currently supported by the SDK, can be made using the `HyperGuestClient` class directly.

| Endpoint | Client Property | Operation | Description |
|-|-|-|
| `/hotels.json` | `Properties` | `GetPropertiesAsync` | Gets all properties for the account. |

## Example

The following example demonstrates how to use the HyperGuest SDK to get the latest meeting and sessions.

```csharp
// Gets all properties
var properties = await client.Properties.GetPropertiesAsync();
```

### Debugging

To aid in debugging results from the Open F1 API, you can enable the following settings:

```json
{
  "HyperGuest": {
    "CaptureRequestContent": true,
    "CaptureResponseContent": true
  }
}
```

These settings, when enabled will capture the request and response content for each API call, and the content of these will be available to the `HyperGuestResponse` as `RequestContent` and `ResponseContent` properties. The SDK will automatically map these results, but for unexpected results, it is useful to understand what has been sent/received.

## Open Source

This SDK is open source and is available under the MIT license. Feel free to contribute to the project by submitting pull requests or issues.

| Component | Authors | Website | License |
|-----------|---------|---------|---------|
| .NET Platform | Microsoft and contributors | [GitHub](https://github.com/dotnet) | MIT |
| Ben.Demystifier | Ben Adams | [GitHub](https://github.com/benaadams/Ben.Demystifier) | Apache V2 |
| FluentValidation | Jeremy Skinner and contributors | [GitHub](https://github.com/FluentValidation/FluentValidation) | Apache V2 |
| MinVer | Adam Ralph and contributors | [GitHub](https://github.com/adamralph/minver) | Apache V2 |

By using this SDK, you agree to the terms of the MIT license used by this project, as well as the terms of the licenses of the components used by this SDK.

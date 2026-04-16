# holidays.rest .NET SDK

Official .NET SDK for the [holidays.rest](https://holidays.rest) API.

## Requirements

- .NET 8+
- Zero external dependencies — uses `System.Net.Http` and `System.Text.Json` (built-in)

## Installation

```bash
dotnet add package HolidaysRest
```

## Quick Start

```csharp
using HolidaysRest;

await using var client = new HolidaysClient("YOUR_API_KEY");

var holidays = await client.GetHolidaysAsync(new HolidaysParams
{
    Country = "US",
    Year    = 2024,
});

foreach (var h in holidays)
    Console.WriteLine($"{h.Date} — {h.Name}");
```

Get an API key at [holidays.rest/dashboard](https://www.holidays.rest/dashboard).

---

## API

### `HolidaysClient`

```csharp
// Default — client owns and disposes the HttpClient
var client = new HolidaysClient("YOUR_API_KEY");

// Override base URL (useful for testing)
var client = new HolidaysClient("YOUR_API_KEY", baseUrl: "https://localhost:5000");

// Bring your own HttpClient (e.g. from IHttpClientFactory)
var client = new HolidaysClient("YOUR_API_KEY", httpClient: myHttpClient);
```

`HolidaysClient` implements `IDisposable`. Use `using` or `await using` to release resources.

---

### `GetHolidaysAsync(HolidaysParams, CancellationToken?)` → `Task<IReadOnlyList<Holiday>>`

```csharp
public sealed class HolidaysParams
{
    public required string Country { get; init; }           // ISO 3166 alpha-2 (e.g. "US")
    public required int    Year    { get; init; }           // e.g. 2024

    public int?                  Month    { get; init; }   // 1–12
    public int?                  Day      { get; init; }   // 1–31
    public IReadOnlyList<string>? Type    { get; init; }   // "religious", "national", "local"
    public IReadOnlyList<int>?   Religion { get; init; }   // religion codes 1–11
    public IReadOnlyList<string>? Region  { get; init; }   // subdivision codes from GetCountryAsync
    public IReadOnlyList<string>? Lang    { get; init; }   // language codes from GetLanguagesAsync
    public string?               Response { get; init; }   // "json" | "xml" | "yaml" | "csv"
}
```

```csharp
// All US holidays in 2024
await client.GetHolidaysAsync(new() { Country = "US", Year = 2024 });

// National holidays only
await client.GetHolidaysAsync(new() { Country = "DE", Year = 2024, Type = ["national"] });

// Multiple types
await client.GetHolidaysAsync(new() { Country = "TR", Year = 2024, Type = ["national", "religious"] });

// Filter by month and day
await client.GetHolidaysAsync(new() { Country = "GB", Year = 2024, Month = 12, Day = 25 });

// Specific region
await client.GetHolidaysAsync(new() { Country = "US", Year = 2024, Region = ["US-CA"] });

// With cancellation
await client.GetHolidaysAsync(new() { Country = "US", Year = 2024 }, cancellationToken);
```

---

### `GetCountriesAsync(CancellationToken?)` → `Task<IReadOnlyList<Country>>`

```csharp
var countries = await client.GetCountriesAsync();
foreach (var c in countries)
    Console.WriteLine($"{c.Alpha2} — {c.Name}");
```

---

### `GetCountryAsync(string, CancellationToken?)` → `Task<Country>`

Returns country details including subdivision codes usable as `Region` filters.

```csharp
var us = await client.GetCountryAsync("US");
foreach (var s in us.Subdivisions ?? [])
    Console.WriteLine($"{s.Code} — {s.Name}");
```

---

### `GetLanguagesAsync(CancellationToken?)` → `Task<IReadOnlyList<Language>>`

```csharp
var languages = await client.GetLanguagesAsync();
```

---

## Models

All responses deserialize into immutable `record` types.

```csharp
record Holiday(string Name, string Date, string Type, string Country,
               string? Region, string? Religion, string? Language);

record Country(string Name, string Alpha2, IReadOnlyList<Subdivision>? Subdivisions);

record Subdivision(string Code, string Name);

record Language(string Code, string Name);
```

---

## Error Handling

Non-2xx responses throw `HolidaysApiException`:

```csharp
try
{
    var holidays = await client.GetHolidaysAsync(new() { Country = "US", Year = 2024 });
}
catch (HolidaysApiException ex)
{
    Console.WriteLine(ex.StatusCode);  // HTTP status code (int)
    Console.WriteLine(ex.Message);     // Error message (string)
    Console.WriteLine(ex.Body);        // Raw response body (string)
}
```

| Status | Meaning             |
|--------|---------------------|
| 400    | Bad request         |
| 401    | Invalid API key     |
| 404    | Not found           |
| 500    | Server error        |
| 503    | Service unavailable |

---

## Dependency Injection

Works cleanly with `IHttpClientFactory`:

```csharp
// Program.cs
builder.Services.AddHttpClient<HolidaysClient>((sp, http) =>
{
    http.BaseAddress = new Uri("https://api.holidays.rest/v1/");
});

// Register the client
builder.Services.AddSingleton(sp =>
    new HolidaysClient(
        apiKey: builder.Configuration["HolidaysRest:ApiKey"]!,
        httpClient: sp.GetRequiredService<IHttpClientFactory>().CreateClient()
    ));
```

---

## Publishing

```bash
dotnet pack -c Release
dotnet nuget push bin/Release/HolidaysRest.1.0.0.nupkg --api-key YOUR_NUGET_KEY --source https://api.nuget.org/v3/index.json
```

---

## License

MIT

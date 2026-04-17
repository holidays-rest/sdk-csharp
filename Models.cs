using System.Text.Json.Serialization;

namespace HolidaysRest;

public sealed record HolidayDay(
    [property: JsonPropertyName("actual")]   string Actual,
    [property: JsonPropertyName("observed")] string Observed
);

public sealed record Holiday(
    [property: JsonPropertyName("name")]         IReadOnlyDictionary<string, string> Name,
    [property: JsonPropertyName("date")]         string Date,
    [property: JsonPropertyName("country_code")] string CountryCode,
    [property: JsonPropertyName("country_name")] string CountryName,
    [property: JsonPropertyName("isNational")]   bool IsNational,
    [property: JsonPropertyName("isReligious")]  bool IsReligious,
    [property: JsonPropertyName("isLocal")]      bool IsLocal,
    [property: JsonPropertyName("isEstimate")]   bool IsEstimate,
    [property: JsonPropertyName("day")]          HolidayDay Day,
    [property: JsonPropertyName("religion")]     string? Religion,
    [property: JsonPropertyName("regions")]      IReadOnlyList<string> Regions
);

public sealed record Subdivision(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name
);

public sealed record Country(
    [property: JsonPropertyName("name")]         string Name,
    [property: JsonPropertyName("alpha2")]        string Alpha2,
    [property: JsonPropertyName("subdivisions")]  IReadOnlyList<Subdivision>? Subdivisions
);

public sealed record Language(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name
);

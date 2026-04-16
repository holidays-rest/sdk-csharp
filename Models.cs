using System.Text.Json.Serialization;

namespace HolidaysRest;

public sealed record Holiday(
    [property: JsonPropertyName("name")]     string Name,
    [property: JsonPropertyName("date")]     string Date,
    [property: JsonPropertyName("type")]     string Type,
    [property: JsonPropertyName("country")]  string Country,
    [property: JsonPropertyName("region")]   string? Region,
    [property: JsonPropertyName("religion")] string? Religion,
    [property: JsonPropertyName("language")] string? Language
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

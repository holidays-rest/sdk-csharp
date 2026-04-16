namespace HolidaysRest;

/// <summary>Parameters for <see cref="HolidaysClient.GetHolidaysAsync"/>.</summary>
public sealed class HolidaysParams
{
    /// <summary>ISO 3166 alpha-2 country code, e.g. "US". Required.</summary>
    public required string Country { get; init; }

    /// <summary>Four-digit year, e.g. 2024. Required.</summary>
    public required int Year { get; init; }

    /// <summary>Month filter (1–12). Optional.</summary>
    public int? Month { get; init; }

    /// <summary>Day filter (1–31). Optional.</summary>
    public int? Day { get; init; }

    /// <summary>Holiday type(s): "religious", "national", "local". Optional.</summary>
    public IReadOnlyList<string>? Type { get; init; }

    /// <summary>Religion code(s) 1–11. Optional.</summary>
    public IReadOnlyList<int>? Religion { get; init; }

    /// <summary>Region/subdivision code(s) — see GetCountryAsync. Optional.</summary>
    public IReadOnlyList<string>? Region { get; init; }

    /// <summary>Language code(s) — see GetLanguagesAsync. Optional.</summary>
    public IReadOnlyList<string>? Lang { get; init; }

    /// <summary>"json" (default) | "xml" | "yaml" | "csv". Optional.</summary>
    public string? Response { get; init; }
}

using System.Net;
using Xunit;

namespace HolidaysRest.Tests;

public sealed class HolidaysClientTests
{
    // ── constructor ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyApiKey_Throws(string key)
    {
        Assert.Throws<ArgumentException>(() => new HolidaysClient(key));
    }

    [Fact]
    public void Constructor_ValidKey_DoesNotThrow()
    {
        using var client = new HolidaysClient("valid-key");
        Assert.NotNull(client);
    }

    // ── GetHolidaysAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetHolidaysAsync_NullParams_Throws()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out _);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.GetHolidaysAsync(null!));
    }

    [Fact]
    public async Task GetHolidaysAsync_EmptyArray_ReturnsEmpty()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out _);
        var result = await client.GetHolidaysAsync(new HolidaysParams { Country = "US", Year = 2024 });
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetHolidaysAsync_ValidJson_DeserializesHolidays()
    {
        const string json = """
            [{"name":{"en":"New Year"},"date":"2024-01-01","country_code":"US",
              "country_name":"United States","isNational":true,"isReligious":false,
              "isLocal":false,"isEstimate":false,
              "day":{"actual":"Monday","observed":"Monday"},
              "religion":null,"regions":[]}]
            """;

        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, json, out _);
        var result = await client.GetHolidaysAsync(new HolidaysParams { Country = "US", Year = 2024 });

        Assert.Single(result);
        Assert.Equal("US", result[0].CountryCode);
        Assert.Equal("2024-01-01", result[0].Date);
        Assert.True(result[0].IsNational);
        Assert.Equal("New Year", result[0].Name["en"]);
    }

    [Fact]
    public async Task GetHolidaysAsync_BuildsRequiredQueryParams()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out var handler);
        await client.GetHolidaysAsync(new HolidaysParams { Country = "DE", Year = 2025 });

        var query = handler.LastRequestUri!.Query;
        Assert.Contains("country=DE", query);
        Assert.Contains("year=2025", query);
    }

    [Fact]
    public async Task GetHolidaysAsync_BuildsOptionalQueryParams()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out var handler);
        await client.GetHolidaysAsync(new HolidaysParams
        {
            Country = "DE",
            Year    = 2025,
            Month   = 12,
            Day     = 25,
            Type    = ["national"],
            Region  = ["DE-BY"],
            Lang    = ["en", "de"],
        });

        var query = handler.LastRequestUri!.Query;
        Assert.Contains("month=12", query);
        Assert.Contains("day=25", query);
        Assert.Contains("type=national", query);
        Assert.Contains("region=DE-BY", query);
        Assert.Contains("lang=en%2cde", query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetHolidaysAsync_ApiError_ThrowsHolidaysApiException()
    {
        const string errorBody = """{"message":"Unauthorized"}""";
        using var client = FakeHttpHandler.Client(HttpStatusCode.Unauthorized, errorBody, out _);

        var ex = await Assert.ThrowsAsync<HolidaysApiException>(() =>
            client.GetHolidaysAsync(new HolidaysParams { Country = "US", Year = 2024 }));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("Unauthorized", ex.Message);
        Assert.Equal(errorBody, ex.Body);
    }

    [Fact]
    public async Task GetHolidaysAsync_ApiError_NoMessageField_UsesReasonPhrase()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.Forbidden, "{}", out _);

        var ex = await Assert.ThrowsAsync<HolidaysApiException>(() =>
            client.GetHolidaysAsync(new HolidaysParams { Country = "US", Year = 2024 }));

        Assert.Equal(403, ex.StatusCode);
    }

    // ── GetCountriesAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetCountriesAsync_EmptyArray_ReturnsEmpty()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out _);
        var result = await client.GetCountriesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCountriesAsync_ValidJson_DeserializesCountries()
    {
        const string json = """
            [{"name":"Germany","alpha2":"DE","subdivisions":[{"code":"DE-BY","name":"Bavaria"}]}]
            """;

        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, json, out _);
        var result = await client.GetCountriesAsync();

        Assert.Single(result);
        Assert.Equal("DE", result[0].Alpha2);
        Assert.Equal("Germany", result[0].Name);
        Assert.Single(result[0].Subdivisions!);
        Assert.Equal("DE-BY", result[0].Subdivisions![0].Code);
    }

    // ── GetCountryAsync ──────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCountryAsync_EmptyCode_Throws(string code)
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "{}", out _);
        await Assert.ThrowsAsync<ArgumentException>(() => client.GetCountryAsync(code));
    }

    [Fact]
    public async Task GetCountryAsync_ValidJson_DeserializesCountry()
    {
        const string json = """{"name":"United States","alpha2":"US","subdivisions":null}""";
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, json, out _);

        var result = await client.GetCountryAsync("US");

        Assert.Equal("US", result.Alpha2);
        Assert.Equal("United States", result.Name);
    }

    [Fact]
    public async Task GetCountryAsync_EscapesCountryCodeInUrl()
    {
        const string json = """{"name":"United States","alpha2":"US","subdivisions":null}""";
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, json, out var handler);

        await client.GetCountryAsync("US");

        Assert.Contains("/country/US", handler.LastRequestUri!.AbsolutePath);
    }

    // ── GetLanguagesAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetLanguagesAsync_EmptyArray_ReturnsEmpty()
    {
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, "[]", out _);
        var result = await client.GetLanguagesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLanguagesAsync_ValidJson_DeserializesLanguages()
    {
        const string json = """[{"code":"en","name":"English"},{"code":"de","name":"German"}]""";
        using var client = FakeHttpHandler.Client(HttpStatusCode.OK, json, out _);

        var result = await client.GetLanguagesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("en", result[0].Code);
        Assert.Equal("German", result[1].Name);
    }

    // ── Dispose ──────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_OwnedHttpClient_DoesNotThrow()
    {
        var client = new HolidaysClient("key", "https://fake.test");
        var ex = Record.Exception(() => client.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_ExternalHttpClient_DoesNotDisposeIt()
    {
        var http = new HttpClient { BaseAddress = new Uri("https://fake.test/v1/") };
        var client = new HolidaysClient("key", http);
        client.Dispose();
        // HttpClient still usable — no ObjectDisposedException
        var ex = Record.Exception(() => http.BaseAddress);
        Assert.Null(ex);
        http.Dispose();
    }
}

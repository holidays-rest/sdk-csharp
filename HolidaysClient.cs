using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace HolidaysRest;

/// <summary>
/// Async client for the holidays.rest API.
/// See https://docs.holidays.rest for full documentation.
/// </summary>
public sealed class HolidaysClient : IDisposable
{
    private const string DefaultBaseUrl = "https://api.holidays.rest/v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// Creates a new client with a dedicated <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="apiKey">Bearer token from https://www.holidays.rest/dashboard.</param>
    /// <param name="baseUrl">Override base URL. Useful for testing.</param>
    public HolidaysClient(string apiKey, string baseUrl = DefaultBaseUrl)
        : this(apiKey, new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") }, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// Creates a new client using an externally managed <see cref="HttpClient"/>.
    /// The caller is responsible for the client's lifetime.
    /// </summary>
    /// <param name="apiKey">Bearer token from https://www.holidays.rest/dashboard.</param>
    /// <param name="httpClient">Pre-configured HTTP client (e.g. from IHttpClientFactory).</param>
    public HolidaysClient(string apiKey, HttpClient httpClient)
        : this(apiKey, httpClient, ownsHttpClient: false)
    {
    }

    private HolidaysClient(string apiKey, HttpClient httpClient, bool ownsHttpClient)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("apiKey must not be empty.", nameof(apiKey));

        _http = httpClient;
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        _ownsHttpClient = ownsHttpClient;
    }

    // ── public API ────────────────────────────────────────────────────────

    /// <summary>Fetches public holidays matching the given parameters.</summary>
    public async Task<IReadOnlyList<Holiday>> GetHolidaysAsync(
        HolidaysParams p, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(p);

        var q = HttpUtility.ParseQueryString(string.Empty);
        q["country"] = p.Country;
        q["year"]    = p.Year.ToString();

        if (p.Month.HasValue)    q["month"]    = p.Month.Value.ToString();
        if (p.Day.HasValue)      q["day"]      = p.Day.Value.ToString();
        if (p.Type is { Count: > 0 })     q["type"]     = string.Join(",", p.Type);
        if (p.Religion is { Count: > 0 }) q["religion"] = string.Join(",", p.Religion);
        if (p.Region is { Count: > 0 })   q["region"]   = string.Join(",", p.Region);
        if (p.Lang is { Count: > 0 })     q["lang"]     = string.Join(",", p.Lang);
        if (p.Response is not null)        q["response"] = p.Response;

        return await GetAsync<IReadOnlyList<Holiday>>($"holidays?{q}", cancellationToken)
               ?? [];
    }

    /// <summary>Returns all supported countries.</summary>
    public async Task<IReadOnlyList<Country>> GetCountriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<IReadOnlyList<Country>>("countries", cancellationToken) ?? [];
    }

    /// <summary>
    /// Returns details for one country, including subdivision codes
    /// that can be used as region filters in <see cref="GetHolidaysAsync"/>.
    /// </summary>
    /// <param name="countryCode">ISO 3166 alpha-2 code, e.g. "US".</param>
    public async Task<Country> GetCountryAsync(
        string countryCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException("countryCode must not be empty.", nameof(countryCode));

        return await GetAsync<Country>($"country/{Uri.EscapeDataString(countryCode)}", cancellationToken)
               ?? throw new HolidaysApiException("Empty response", 200, string.Empty);
    }

    /// <summary>Returns all supported language codes.</summary>
    public async Task<IReadOnlyList<Language>> GetLanguagesAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<IReadOnlyList<Language>>("languages", cancellationToken) ?? [];
    }

    // ── internal ─────────────────────────────────────────────────────────

    private async Task<T?> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(relativeUrl, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string message;
            try
            {
                var err = JsonSerializer.Deserialize<JsonElement>(body);
                message = err.TryGetProperty("message", out var m) ? m.GetString() ?? response.ReasonPhrase! : response.ReasonPhrase!;
            }
            catch
            {
                message = response.ReasonPhrase ?? response.StatusCode.ToString();
            }

            throw new HolidaysApiException(message, (int)response.StatusCode, body);
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_ownsHttpClient) _http.Dispose();
    }
}

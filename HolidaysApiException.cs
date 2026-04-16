namespace HolidaysRest;

/// <summary>Thrown when the API returns a non-2xx HTTP response.</summary>
public sealed class HolidaysApiException : Exception
{
    /// <summary>HTTP status code returned by the API.</summary>
    public int StatusCode { get; }

    /// <summary>Raw response body.</summary>
    public string Body { get; }

    public HolidaysApiException(string message, int statusCode, string body)
        : base(message)
    {
        StatusCode = statusCode;
        Body = body;
    }

    public override string ToString() =>
        $"HolidaysApiException: HTTP {StatusCode} — {Message}";
}

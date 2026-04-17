using System.Net;
using System.Text;

namespace HolidaysRest.Tests;

internal sealed class FakeHttpHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _body;

    public Uri? LastRequestUri { get; private set; }

    public FakeHttpHandler(HttpStatusCode statusCode, string body)
    {
        _statusCode = statusCode;
        _body = body;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        };
        return Task.FromResult(response);
    }

    public static HolidaysClient Client(HttpStatusCode status, string body, out FakeHttpHandler handler)
    {
        handler = new FakeHttpHandler(status, body);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://fake.test/v1/") };
        return new HolidaysClient("test-key", http);
    }
}

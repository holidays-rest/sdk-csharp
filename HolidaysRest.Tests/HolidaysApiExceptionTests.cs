using Xunit;

namespace HolidaysRest.Tests;

public sealed class HolidaysApiExceptionTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var ex = new HolidaysApiException("Not Found", 404, """{"message":"Not Found"}""");

        Assert.Equal("Not Found", ex.Message);
        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("""{"message":"Not Found"}""", ex.Body);
    }

    [Fact]
    public void ToString_ContainsStatusCodeAndMessage()
    {
        var ex = new HolidaysApiException("Unauthorized", 401, string.Empty);
        var str = ex.ToString();

        Assert.Contains("401", str);
        Assert.Contains("Unauthorized", str);
    }

    [Fact]
    public void IsException()
    {
        var ex = new HolidaysApiException("err", 500, string.Empty);
        Assert.IsAssignableFrom<Exception>(ex);
    }
}

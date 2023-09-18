using System.Net;
using RichardSzalay.MockHttp;

namespace Slow.Utilities.Http.Tests;

public class RetryHandlerTests
{
    [Theory]
    [InlineData(HttpStatusCode.OK, 1)]
    [InlineData(HttpStatusCode.BadRequest, 1)]
    [InlineData(HttpStatusCode.NotFound, 1)]
    [InlineData(HttpStatusCode.InternalServerError, 1)]
    [InlineData(HttpStatusCode.RequestTimeout, 3)]
    [InlineData(HttpStatusCode.GatewayTimeout, 3)]
    [InlineData(HttpStatusCode.BadGateway, 3)]
    [InlineData(HttpStatusCode.ServiceUnavailable, 3)]
    public async Task ShouldRetryOnStatusCodeAsync(HttpStatusCode statusCode, int expectedRetryCount)
    {
        // Given
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Get, "http://example.com/thing")
            .Respond(statusCode);
        var retryHandler = new RetryHandler(retryCount: 3, backoffSeconds: 1)
        {
            InnerHandler = mockHttp
        };
        var httpClient = new HttpClient(retryHandler);

        // When
        using var response = await httpClient.GetAsync("http://example.com/thing");

        // Then
        Assert.Equal(expectedRetryCount, mockHttp.GetMatchCount(request));
        Assert.NotNull(response);
        Assert.Equal(statusCode, response.StatusCode);
    }

    [Theory]
    [InlineData(typeof(HttpRequestException), 3)]
    [InlineData(typeof(IOException), 1)]
    [InlineData(typeof(NotImplementedException), 1)]
    public async Task ShouldRetryOnExceptionAsync(Type exceptionType, int expectedRetryCount)
    {
        // Given
        var mockHttp = new MockHttpMessageHandler();
        var request = mockHttp.When(HttpMethod.Get, "http://example.com/thing")
            .Throw((Exception)Activator.CreateInstance(exceptionType));
        var retryCount = 3;
        var retryHandler = new RetryHandler(retryCount, backoffSeconds: 1)
        {
            InnerHandler = mockHttp
        };
        var httpClient = new HttpClient(retryHandler);

        // When
        var exception = await Record.ExceptionAsync(async () => await httpClient.GetAsync("http://example.com/thing"));

        // Then
        Assert.NotNull(exception);
        Assert.IsType(exceptionType, exception);
        Assert.Equal(expectedRetryCount, mockHttp.GetMatchCount(request));
    }
}

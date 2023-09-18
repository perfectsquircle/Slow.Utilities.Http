using System.Net;

namespace Slow.Utilities.Http;

/// <summary>
/// An HttpMessageHandler that will retry failed requests up to a given number of times.
/// It will retry on retryable status codes and HttpRequestException. It uses exponentially
/// increasing backoff time for each retry (_backoffTime ^ tryNum).
/// </summary>
public class RetryHandler : DelegatingHandler
{
    private readonly int _retryCount;
    private readonly int _backoffSeconds;

    private readonly HttpStatusCode[] _retryStatusCodes = new[]
    {
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    };

    public RetryHandler(int retryCount, int backoffSeconds)
    {
        _retryCount = retryCount;
        _backoffSeconds = backoffSeconds;
    }

    public RetryHandler()
        : this(retryCount: 3, backoffSeconds: 2)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Task BackoffAsync(int tryNum)
        {
            return Task.Delay(TimeSpan.FromSeconds(Math.Pow(_backoffSeconds, tryNum)), cancellationToken);
        }

        for (var tryNum = 1; tryNum <= _retryCount; tryNum++)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                if (!_retryStatusCodes.Contains(response.StatusCode) || tryNum == _retryCount)
                {
                    return response;
                }
                await BackoffAsync(tryNum);
                continue;
            }
            catch (HttpRequestException)
            {
                if (tryNum == _retryCount)
                {
                    throw;
                };
                await BackoffAsync(tryNum);
            }
        }

        throw new HttpRequestException("Retry exhausted");
    }
}
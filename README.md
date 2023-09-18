# Slow.Utilities.Http

Simple HTTP utilities to make using HttpClient easier.

## Usage

Examples of using HttpRequestBuilder:

```csharp
using static Slow.Utilities.Http.HttpRequestBuilder;

var getRequest = Get($"api/v1/things")
    .WithQueryParameter("page", 3);
var getResponse = await httpClient.SendAsync(getRequest);

var getRequest2 = Get($"api/v1/thing/{thingId}")
    .WithQueryParameter("page", 3);
var getResponse2 = await httpClient.SendAsync(getRequest2);

var postRequest = Post($"api/v1/things")
    .WithJsonContent(new { ThingId = 1, ThingName = "foo" });
var postResponse = await httpClient.SendAsync(getRequest);

```

How to use RetryHandler with [HttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)

```csharp
services.AddTransient<Slow.Utilities.Http.RetryHandler>();
services.AddHttpClient("SomeApi", (client) =>
{
    client.BaseAddress = new Uri(configuration["SomeApi:BaseAddress"]);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
}).AddHttpMessageHandler<Slow.Utilities.Http.RetryHandler>();
```

---

Built with &hearts; by Calvin.

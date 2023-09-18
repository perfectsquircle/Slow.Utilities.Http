using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Slow.Utilities.Http;

public class HttpRequestBuilder
{
    private HttpMethod _method;
    private readonly PathAndQueryBuilder _pathAndQueryBuilder;
    private readonly HttpRequestHeaders _headers;
    private HttpContent _content;


    public HttpRequestBuilder()
    {
        _method = HttpMethod.Get;
        _pathAndQueryBuilder = new PathAndQueryBuilder();
        _headers = (HttpRequestHeaders)Activator.CreateInstance(typeof(HttpRequestHeaders), nonPublic: true);
    }

    public HttpRequestBuilder WithMethod(HttpMethod httpMethod)
    {
        _method = httpMethod;
        return this;
    }

    public HttpRequestBuilder WithPath(string path, params object[] args)
    {
        _pathAndQueryBuilder.WithPath(path, args);
        return this;
    }

    public HttpRequestBuilder WithQuery(object queryObject)
    {
        _pathAndQueryBuilder.WithQuery(queryObject);
        return this;
    }

    public HttpRequestBuilder WithQueryParameter(string name, object value)
    {
        _pathAndQueryBuilder.WithQueryParameter(name, value);
        return this;
    }

    public HttpRequestBuilder WithHeader(string name, string value)
    {
        _headers.Add(name, value);
        return this;
    }

    public HttpRequestBuilder WithHeaders(Action<HttpRequestHeaders> callback)
    {
        callback(_headers);
        return this;
    }

    public HttpRequestBuilder WithContent(HttpContent httpContent)
    {
        _content = httpContent;
        return this;
    }

    public HttpRequestBuilder WithJsonContent(object inputValue, MediaTypeHeaderValue mediaType = null, JsonSerializerOptions options = null)
    {
        _content = JsonContent.Create(inputValue, mediaType, options);
        return this;
    }

    public HttpRequestBuilder WithFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
    {
        _content = new FormUrlEncodedContent(nameValueCollection);
        return this;
    }

    public HttpRequestBuilder WithFormUrlEncodedContent(object formObject)
    {
        var pairs = formObject.GetType().GetProperties().Select(p =>
            new KeyValuePair<string, string>(p.Name, Convert.ToString(p.GetValue(formObject))));
        _content = new FormUrlEncodedContent(pairs);
        return this;
    }

    public HttpRequestMessage Build()
    {
        var pathAndQuery = _pathAndQueryBuilder.Build();
        var message = new HttpRequestMessage(_method, pathAndQuery);
        foreach (var header in _headers)
        {
            message.Headers.Add(header.Key, header.Value);
        }
        if (_content is not null)
        {
            message.Content = _content;
        }
        return message;
    }

    public static implicit operator HttpRequestMessage(HttpRequestBuilder b) => b.Build();

    public static HttpRequestBuilder Get(string path, params object[] args) => new HttpRequestBuilder().WithMethod(HttpMethod.Get).WithPath(path, args);
    public static HttpRequestBuilder Put(string path, params object[] args) => new HttpRequestBuilder().WithMethod(HttpMethod.Put).WithPath(path, args);
    public static HttpRequestBuilder Post(string path, params object[] args) => new HttpRequestBuilder().WithMethod(HttpMethod.Post).WithPath(path, args);
    public static HttpRequestBuilder Delete(string path, params object[] args) => new HttpRequestBuilder().WithMethod(HttpMethod.Delete).WithPath(path, args);
#if NET6_0_OR_GREATER
    public static HttpRequestBuilder Patch(string path, params object[] args) => new HttpRequestBuilder().WithMethod(HttpMethod.Patch).WithPath(path, args);
#endif

#if NETSTANDARD2_0
    public static HttpRequestBuilder Patch(string path, params object[] args) => new HttpRequestBuilder().WithMethod(new HttpMethod("PATCH")).WithPath(path, args);
#endif

    public override string ToString()
    {
        return Build()?.ToString();
    }
}
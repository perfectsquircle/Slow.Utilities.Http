using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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

    public HttpRequestBuilder WithPath(FormattableString path)
    {
        _pathAndQueryBuilder.WithPath(path);
        return this;
    }

    public HttpRequestBuilder WithPathRaw(string path)
    {
        _pathAndQueryBuilder.WithPathRaw(path);
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

    public HttpRequestBuilder WithJsonContent<T>(T inputValue, MediaTypeHeaderValue mediaType = null, JsonSerializerOptions options = null)
    {
        _content = JsonContent.Create<T>(inputValue, mediaType, options);
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

    public static HttpRequestBuilder Get(FormattableString path) => new HttpRequestBuilder().WithMethod(HttpMethod.Get).WithPath(path);
    public static HttpRequestBuilder Put(FormattableString path) => new HttpRequestBuilder().WithMethod(HttpMethod.Put).WithPath(path);
    public static HttpRequestBuilder Post(FormattableString path) => new HttpRequestBuilder().WithMethod(HttpMethod.Post).WithPath(path);
    public static HttpRequestBuilder Delete(FormattableString path) => new HttpRequestBuilder().WithMethod(HttpMethod.Delete).WithPath(path);
#if NET6_0_OR_GREATER
    public static HttpRequestBuilder Patch(FormattableString path) => new HttpRequestBuilder().WithMethod(HttpMethod.Patch).WithPath(path);
#endif

#if NETSTANDARD2_0
    public static HttpRequestBuilder Patch(FormattableString path) => new HttpRequestBuilder().WithMethod(new HttpMethod("PATCH")).WithPath(path);
#endif

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Method: ").Append(_method);
        sb.Append(", Path: ").Append(_pathAndQueryBuilder);
        if (_content is not null)
        {
            sb.Append(", Content: ").Append(_content);
        }
        var headers = _content == null ? _headers : _headers.Concat(_content.Headers);
        if (headers.Any())
        {
            var headerString = string.Join(Environment.NewLine, headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"));
            sb.Append(", Headers: ").Append(Environment.NewLine).Append(headerString);
        }
        return sb.ToString();
    }
}
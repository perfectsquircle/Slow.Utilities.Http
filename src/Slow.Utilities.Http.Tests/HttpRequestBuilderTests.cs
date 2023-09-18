using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using static Slow.Utilities.Http.HttpRequestBuilder;

namespace Slow.Utilities.Http.Tests;
public class HttpRequestBuilderTests
{
    [Fact]
    public void ShouldCreateGet()
    {
        // Given
        var builder = Get("foo/bar");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldCreatePut()
    {
        // Given
        var builder = Put("foo/bar");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldCreatePost()
    {
        // Given
        var builder = Post("foo/bar");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldCreateDelete()
    {
        // Given
        var builder = Delete("foo/bar");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldBuildAbsolute()
    {
        // Given
        var builder = Get("http://example.com/foo/bar");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.True(request.RequestUri.IsAbsoluteUri);
        Assert.Equal("http://example.com/foo/bar", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldBuildPath()
    {
        // Given
        var builder = new HttpRequestBuilder()
            .WithPath("foo/{0}/bar/{1}/bat/{2}", "this has", "special/characters?", 8675309);

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal("foo/this+has/bar/special%2fcharacters%3f/bat/8675309", request.RequestUri.ToString());
    }

    [Fact]
    public void ShouldBuildQuery()
    {
        // Given
        var builder = Get("foo/bar")
            .WithQueryParameter("foo", "bar")
            .WithQueryParameter("& this has", "special/characters?")
            .WithQueryParameter("page", 3);

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Get, request.Method);
        var pathAndQuery = request.RequestUri.ToString().Split("?");
        Assert.Equal("foo/bar", pathAndQuery[0]);
        var query = HttpUtility.ParseQueryString(pathAndQuery[1]);
        Assert.Equal("bar", query["foo"]);
        Assert.Equal("special/characters?", query["& this has"]);
        Assert.Equal("3", query["page"]);
    }


    [Fact]
    public void ShouldBuildQueryFromObject()
    {
        // Given
        var builder = Get("foo/bar")
            .WithQuery(new
            {
                foo = "bar",
                thisHas = "special/characters?",
                page = 3,
            });

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Get, request.Method);
        var pathAndQuery = request.RequestUri.ToString().Split("?");
        Assert.Equal("foo/bar", pathAndQuery[0]);
        var query = HttpUtility.ParseQueryString(pathAndQuery[1]);
        Assert.Equal("bar", query["foo"]);
        Assert.Equal("special/characters?", query["thisHas"]);
        Assert.Equal("3", query["page"]);
    }

    [Fact]
    public async Task ShouldAddJsonContentFromObjectAsync()
    {
        // Given
        var builder = Post("foo/bar")
            .WithJsonContent(new
            {
                foo = "bar",
                thisHas = "special/characters?",
                page = 3,
            });

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
        var content = JsonSerializer.Deserialize<JsonObject>(await request.Content.ReadAsStringAsync());
        Assert.Equal("bar", content["foo"].ToString());
        Assert.Equal("special/characters?", content["thisHas"].ToString());
        Assert.Equal("3", content["page"].ToString());
    }

    [Fact]
    public void ShouldAddHeader()
    {
        // Given
        var builder = Post("foo/bar")
            .WithHeader("Authorization", "Bearer qwerty");

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("qwerty", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public void ShouldAddHeaders()
    {
        // Given
        var builder = Post("foo/bar")
            .WithHeaders(h => h.Authorization = new AuthenticationHeaderValue("Bearer", "qwerty"));

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("foo/bar", request.RequestUri.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("qwerty", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async void ShouldAddFormContentFromObject()
    {
        // Given
        var builder = Post("foo/bar")
            .WithFormUrlEncodedContent(new
            {
                foo = "bar",
                thisHas = "special/characters?",
                page = 3,
            });

        // When
        using var request = builder.Build();

        // Then
        Assert.NotNull(request);
        Assert.Equal(HttpMethod.Post, request.Method);
        var formString = await request.Content.ReadAsStringAsync();
        var form = HttpUtility.ParseQueryString(formString);
        Assert.Equal("bar", form["foo"]);
        Assert.Equal("special/characters?", form["thisHas"]);
        Assert.Equal("3", form["page"]);
    }
}

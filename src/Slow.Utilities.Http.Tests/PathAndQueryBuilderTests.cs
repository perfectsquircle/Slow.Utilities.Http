using System.Web;
using static Slow.Utilities.Http.PathAndQueryBuilder;

namespace Slow.Utilities.Http.Tests;
public class PathAndQueryBuilderTests
{
    [Fact]
    public void ShouldCreatePath()
    {
        // Given
        var builder = CreatePath("foo/bar");

        // When
        var path = builder.Build();

        // Then
        Assert.NotNull(path);
        Assert.Equal("foo/bar", path);
    }

    [Fact]
    public void ShouldBuildAbsolute()
    {
        // Given
        var builder = CreatePath("http://example.com/foo/bar/{0}", 8675309);

        // When
        var result = builder.Build();

        // Then
        Assert.NotNull(result);
        Assert.Equal("http://example.com/foo/bar/8675309", result);
    }

    [Fact]
    public void ShouldBuildPath()
    {
        // Given
        var builder = CreatePath("foo/{0}/bar/{1}/bat/{2}", "this has", "special/characters?", 8675309);

        // When
        var result = builder.Build();

        // Then
        Assert.NotNull(result);
        Assert.Equal("foo/this+has/bar/special%2fcharacters%3f/bat/8675309", result);
    }

    [Fact]
    public void ShouldBuildQuery()
    {
        // Given
        var builder = CreatePath("foo/bar")
            .WithQueryParameter("foo", "bar")
            .WithQueryParameter("& this has", "special/characters?")
            .WithQueryParameter("page", 3);

        // When
        var result = builder.Build();

        // Then
        Assert.NotNull(result);
        var pathAndQuery = result.Split("?");
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
        var builder = CreatePath("foo/bar")
            .WithQuery(new
            {
                foo = "bar",
                thisHas = "special/characters?",
                page = 3,
            });

        // When
        var result = builder.Build();

        // Then
        Assert.NotNull(result);
        var pathAndQuery = result.Split("?");
        Assert.Equal("foo/bar", pathAndQuery[0]);
        var query = HttpUtility.ParseQueryString(pathAndQuery[1]);
        Assert.Equal("bar", query["foo"]);
        Assert.Equal("special/characters?", query["thisHas"]);
        Assert.Equal("3", query["page"]);
    }
}

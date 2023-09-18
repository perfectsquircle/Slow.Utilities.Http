using System.Collections.Specialized;
using System.Web;

namespace Slow.Utilities.Http;

public class PathAndQueryBuilder
{
    private string _path;
    private readonly NameValueCollection _query;

    public PathAndQueryBuilder()
    {
        _path = string.Empty;
        _query = HttpUtility.ParseQueryString(string.Empty);
    }

    public PathAndQueryBuilder WithPath(FormattableString path)
    {
        _path = string.Format(
            path.Format,
            path.GetArguments().Select(Convert.ToString)
                .Select(HttpUtility.UrlEncode)
                .ToArray());
        return this;
    }

    public PathAndQueryBuilder WithPathRaw(string path)
    {
        _path = path;
        return this;
    }

    public PathAndQueryBuilder WithQuery(object queryObject)
    {
        foreach (var p in queryObject.GetType().GetProperties())
        {
            WithQueryParameter(p.Name, p.GetValue(queryObject));
        }
        return this;
    }

    public PathAndQueryBuilder WithQueryParameter(string name, object value)
    {
        if (value is null)
        {
            return this;
        }
        _query.Add(HttpUtility.UrlEncode(name), Convert.ToString(value));
        return this;
    }

    public string Build()
    {
        var pathAndQuery = _path;
        if (_query.Count > 0)
        {
            pathAndQuery += "?" + _query.ToString();
        }
        return pathAndQuery;
    }

    public static implicit operator string(PathAndQueryBuilder b) => b.Build();

    public static PathAndQueryBuilder CreatePath(FormattableString path) => new PathAndQueryBuilder().WithPath(path);

    public override string ToString()
    {
        return Build();
    }
}
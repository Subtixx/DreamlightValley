namespace DLV_Api.Http;

[Flags]
public enum HttpMethod
{
    Get = 1 << 0,
    Post = 1 << 1,
    Put = 1 << 2,
    Delete = 1 << 3,
    Patch = 1 << 4,
    Options = 1 << 5,
    Head = 1 << 6,
    Any = Get | Post | Put | Delete | Patch | Options | Head,
}

public static class RequestMethodExtensions
{
    public static bool IsSet(this HttpMethod httpMethod, HttpMethod method)
    {
        return (httpMethod  & method) == method;
    }
    
    public static bool IsAny(this HttpMethod httpMethod)
    {
        return httpMethod.IsSet(HttpMethod.Any);
    }
    
    public static HttpMethod GetRequestMethod(string method)
    {
        return method.ToLowerInvariant() switch
        {
            "get" => HttpMethod.Get,
            "post" => HttpMethod.Post,
            "put" => HttpMethod.Put,
            "delete" => HttpMethod.Delete,
            "patch" => HttpMethod.Patch,
            "options" => HttpMethod.Options,
            "head" => HttpMethod.Head,
            _ => throw new ArgumentException($"Invalid HTTP method: {method}")
        };
    }
    
    public static System.Net.Http.HttpMethod ToNetHttpMethod(this HttpMethod httpMethod)
    {
        return httpMethod switch
        {
            HttpMethod.Get => System.Net.Http.HttpMethod.Get,
            HttpMethod.Post => System.Net.Http.HttpMethod.Post,
            HttpMethod.Put => System.Net.Http.HttpMethod.Put,
            HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
            HttpMethod.Patch => new System.Net.Http.HttpMethod("PATCH"),
            HttpMethod.Options => System.Net.Http.HttpMethod.Options,
            HttpMethod.Head => System.Net.Http.HttpMethod.Head,
            HttpMethod.Any => System.Net.Http.HttpMethod.Get,
            _ => throw new ArgumentException($"Invalid HTTP method: {httpMethod}")
        };
    }
}
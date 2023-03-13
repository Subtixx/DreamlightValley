namespace DLV_Api;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : Attribute
{
    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    public HttpMethod Method { get; }
    public string Path { get; }

    public EndpointAttribute(HttpMethod method, string path)
    {
        Method = method;
        Path = path;
    }

    public static HttpMethod GetHttpMethod(string method)
    {
        return method switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            _ => throw new ArgumentException($"Invalid HTTP method: {method}")
        };
    }
}
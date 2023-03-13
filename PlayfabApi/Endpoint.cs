namespace PlayfabApi;

// Attribute class for Endpoint("/Client/GetPlayerCombinedInfo")

public enum HTTPMethod
{
    GET,
    POST,
    PUT,
    DELETE
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : Attribute
{
    public HTTPMethod Method { get; }
    public string Path { get; }

    public EndpointAttribute(HTTPMethod method, string path)
    {
        Method = method;
        Path = path;
    }
    
    public static HTTPMethod GetHTTPMethod(string method)
    {
        return method switch
        {
            "GET" => HTTPMethod.GET,
            "POST" => HTTPMethod.POST,
            "PUT" => HTTPMethod.PUT,
            "DELETE" => HTTPMethod.DELETE,
            _ => throw new ArgumentException($"Invalid HTTP method: {method}")
        };
    }
}
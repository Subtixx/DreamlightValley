using System.Net;
using System.Net.Http.Headers;

namespace DLV_Api;

public class HttpResponse
{
    public string Body { get; }

    public string ContentType { get; }

    public ushort StatusCode { get; }

    public HttpResponse(HttpStatusCode statusCode, HttpHeaders responseHeaders, string responseContent)
    {
        StatusCode = (ushort)statusCode;
        Body = responseContent;

        responseHeaders.TryGetValues("Content-Type", out var contentType);
        ContentType = contentType != null ? contentType.First() : "text/plain";
    }
}
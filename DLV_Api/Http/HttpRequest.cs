using System.Net;
using System.Text;

namespace DLV_Api.Http;

public class HttpRequest
{
    public string Body { get; set; }

    public string ContentType { get; }
    
    public Uri Url { get; set; }

    public HttpMethod Method { get; }

    public Dictionary<string, string> Headers { get; }

    public HttpRequest(HttpListenerRequest request)
    {
        if (request.Url == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Url = request.Url;
        Body = new StreamReader(request.InputStream).ReadToEnd();
        ContentType = request.ContentType ?? "text/plain";
        Method = RequestMethodExtensions.GetRequestMethod(request.HttpMethod);
        Headers = request.Headers.AllKeys.Where(key => key != null)
            .ToDictionary(key => key ?? throw new ArgumentNullException(nameof(key)), key => request.Headers[key])!;
    }

    public HttpRequestMessage GetHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method.ToNetHttpMethod(), Url);
        request.Content = new StringContent(Body, Encoding.UTF8, ContentType);
        
        return request;
    }
}
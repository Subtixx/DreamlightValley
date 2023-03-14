using System.Net;
using System.Text;

namespace DLV_Api.Http;

public class HttpResponse
{
    public string Body { get; set; }

    public string ContentType { get; }

    public StatusCode StatusCode { get; }
    
    public HttpMethod Method { get; }
    
    public Dictionary<string, string> Headers { get; }

    public HttpResponse(HttpResponseMessage responseMessage)
    {
        if (responseMessage.RequestMessage == null)
        {
            throw new ArgumentNullException(nameof(responseMessage));
        }
        
        Body = responseMessage.Content.ReadAsStringAsync().Result;
        ContentType = responseMessage.Content.Headers.ContentType?.MediaType ?? "text/plain";
        StatusCode = (StatusCode) responseMessage.StatusCode;
        Method = RequestMethodExtensions.GetRequestMethod(responseMessage.RequestMessage.Method.Method);
        Headers = responseMessage.Headers.ToDictionary(x => x.Key, x => x.Value.First());
    }
}
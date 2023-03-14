using DLV_Api.Http;
using HttpMethod = DLV_Api.Http.HttpMethod;

namespace DLV_Api;

public abstract class ApiRequest
{
    /// <summary>
    /// Method called before the request is sent to the PlayFab API. Return null to cancel the request.
    /// </summary>
    /// <param name="request">The request that will be sent to the PlayFab API</param>
    /// <returns>A string that will be sent to the PlayFab API, or null to cancel the request</returns>
    public virtual string? BeforeRequest(HttpRequest request)
    {
        return null;
    }

    /// <summary>
    /// Method called after the request is sent to the PlayFab API.
    /// </summary>
    /// <param name="response">The response that was received from the PlayFab API</param>
    /// <returns>A string that will be sent to the client, or null to send the original unmodified response</returns>
    public virtual string? AfterRequest(HttpResponse response)
    {
        return null;
    }

    /// <summary>
    /// Get the path of the request, e.g. "/Client/GetPlayerCombinedInfo"
    /// </summary>
    /// <returns>The path of the request</returns>
    public abstract string GetPath();

    /// <summary>
    /// Get the method of the request, e.g. RequestMethod.Post
    /// One can use the & operator to combine multiple methods, e.g. RequestMethod.Post & RequestMethod.Get
    /// Use RequestMethod.Any to listen for all methods e.g. Get, Post, Put, Delete, Patch, Options, Head
    /// </summary>
    /// <returns>The method of the request to listen for</returns>
    public virtual HttpMethod GetMethod()
    {
        return HttpMethod.Any;
    }
}
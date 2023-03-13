using Newtonsoft.Json.Linq;

namespace PlayfabApi.Models;

public class ApiResponse
{
    [Newtonsoft.Json.JsonProperty("code")]
    public int Code { get; set; }
    
    [Newtonsoft.Json.JsonProperty("status")]
    public string Status { get; set; } = "";
    
    [Newtonsoft.Json.JsonProperty("data")]
    public JObject? Data { get; set; }
}
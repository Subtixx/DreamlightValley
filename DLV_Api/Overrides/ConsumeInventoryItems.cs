using DLV_Api.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DLV_Api.Overrides;

public class Item
{
    [JsonProperty("AlternateId")]
    public string? AlternateId { get; set; }
    
    [JsonProperty("Duration")]
    public string? Duration { get; set; }
    
    [JsonProperty("ExpirationDate")]
    public string? ExpirationDate { get; set; }
    
    [JsonProperty("InstanceId")]
    public string? InstanceId { get; set; }
    
    [JsonProperty("IsSubscription")]
    public bool IsSubscription { get; set; }
    
    [JsonProperty("ItemId")]
    public string? ItemId { get; set; }
    
    [JsonProperty("LastRefreshDate")]
    public string? LastRefreshDate { get; set; }
    
    [JsonProperty("Marketplace")]
    public string? Marketplace { get; set; }
    
    [JsonProperty("MergeProperties")]
    public string? MergeProperties { get; set; }
    
    [JsonProperty("NextRecommendedRefreshDate")]
    public string? NextRecommendedRefreshDate { get; set; }
    
    [JsonProperty("Origin")]
    public string? Origin { get; set; }
    
    [JsonProperty("OriginId")]
    public string? OriginId { get; set; }
    
    [JsonProperty("Properties")]
    public string? Properties { get; set; }
    
    [JsonProperty("Quantity")]
    public int Quantity { get; set; }
}

public class ConsumeInventoryItems : ApiRequest
{
    public override string GetPath() => "/Inventory/ConsumeInventoryItems";
    public override string? BeforeRequest(HttpRequest request)
    {
        var jobject = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(request.Body);
        if (jobject == null || !jobject.ContainsKey("Items"))
        {
            return null;
        }
        
        var items = jobject["Items"].ToObject<List<Item>>()!;
        for (var i = 0; i < items.Count; i++)
        {
            items[i].Quantity = 0;
        }
        jobject["Items"] = JObject.FromObject(items);

        return JsonConvert.SerializeObject(jobject);
    }
}

using Newtonsoft.Json;

namespace EveTrading.EveApi;

public class Asset {
    [JsonProperty("is_blueprint_copy")]
    public bool IsBlueprintCopy { get; set; }
    [JsonProperty("is_singleton")]
    public bool IsSingleton { get; set; }
    [JsonProperty("item_id")]
    public long ItemId { get; set; }
    [JsonProperty("location_flag")]
    public string LocationFlag { get; set; }
    [JsonProperty("location_id")]
    public long LocationId { get; set; }
    [JsonProperty("location_type")]
    public string LocationType { get; set; }
    [JsonProperty("quantity")]
    public long Quantity { get; set; }
    [JsonProperty("type_id")]
    public long TypeId { get; set; }
}
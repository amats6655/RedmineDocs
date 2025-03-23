namespace RedmineDocs.Models;

public class ButtonValue
{
    [JsonProperty("id")] public string Id { get; set; }
    [JsonProperty("value")] public string Value { get; set; }
}
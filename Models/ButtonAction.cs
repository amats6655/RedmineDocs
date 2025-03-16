namespace RedmineDocs.Models;

public class ButtonAction
{
	[JsonProperty("action")] public string Action { get; set; }
	[JsonProperty("field_name")] public string FieldName { get; set; }
	[JsonProperty("values")] public List<ButtonValue> Values { get; set; }
}
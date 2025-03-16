using Newtonsoft.Json;

namespace RedmineDocs.Models;

public class ButtonOption
{
	[JsonProperty("field_name")] public string FieldName { get; set; }
	[JsonProperty("invert")] public int Invert { get; set; }

	[JsonProperty("values")] public List<ButtonValue> Values { get; set; } = new();
}
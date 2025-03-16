using Newtonsoft.Json;

namespace RedmineDocs.Models;

public class Tracker
{
	[JsonProperty ("trackerId")] public int Id { get; set; }
	[JsonProperty ("trackerName")] public required string Name { get; set; }
	[JsonProperty ("trackerDescription")] public string? Description { get; set; }
}
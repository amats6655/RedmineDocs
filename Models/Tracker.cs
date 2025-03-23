namespace RedmineDocs.Models;

public class Tracker
{
	[JsonProperty ("trackerId")] public int Id { get; set; }
	[JsonProperty ("trackerName")] public required string Name { get; set; }
}
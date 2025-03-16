using Newtonsoft.Json;
using Serilog;

namespace RedmineDocs.Models;

public class Project
{
	[JsonProperty ("project_id")] public int Id { get; set; }
	[JsonProperty("name")] public string Name { get; set; }
	[JsonProperty("description")] public string Description { get; set; }
	[JsonProperty("trackers_json")] public string TrackersJson { get; set; }
	[JsonProperty("groups_json")] public string GroupsJson { get; set; }
	
	[JsonIgnore] public List<Tracker> Trackers { get; set; } = new List<Tracker>();
	[JsonIgnore] public List<Group> Groups { get; set; } = new List<Group>();

	public void DeserializeJsonFields()
	{
		try
		{
			if (!string.IsNullOrEmpty(TrackersJson))
			{
				Trackers = JsonConvert.DeserializeObject<List<Tracker>>(TrackersJson) ?? new List<Tracker>();
			}

			if (!string.IsNullOrEmpty(GroupsJson))
			{
				Groups = JsonConvert.DeserializeObject<List<Group>>(GroupsJson) ?? new List<Group>();
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Ошибка при десериализации Json-поля проекта {ProjectId}", Id);
			Log.Debug("Trackers: {Trackers}", Trackers);
			Log.Debug("Groups: {Groups}", Groups);
		}
	}
}
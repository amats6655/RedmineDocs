using YamlDotNet.Serialization;

namespace RedmineDocs.Models;

public class RoleSettings
{
    [YamlMember(Alias = "permissions_all_trackers")]
    public Dictionary<string, string>? PermissionsAllTrackers { get; set; }
    [YamlMember(Alias = "permissions_tracker_ids")]
    public Dictionary<string, List<string>>? PermissionsTrackerIds { get; set; }
}
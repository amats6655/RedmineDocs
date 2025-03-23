namespace RedmineDocs.Models;
public class Group
{
	[JsonProperty ("groupId")] public int Id { get; set; }
	[JsonProperty ("groupName")] public required string Name { get; set; }
	[JsonProperty ("roles")] public List<GroupRole>? Roles { get; set; }
	[JsonIgnore] public List<int> RoleIds => Roles?.Select(r => r.RoleId).ToList() ?? new List<int>();
}

public class GroupRole
{
	[JsonProperty ("roleId")] public int RoleId { get; set; }
	[JsonProperty ("roleName")] public required string RoleName { get; set; }
}
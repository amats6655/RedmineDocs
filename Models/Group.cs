namespace RedmineDocs.Models;
public class Group
{
	[JsonProperty ("groupId")] public int Id { get; set; }
	[JsonProperty ("groupName")] public required string Name { get; set; }
	[JsonProperty ("roles")] public List<GroupRole>? RawRoles { get; set; }
	[JsonProperty ("description")] public string? Description { get; set; }

	[JsonIgnore] public List<int> RoleIds => RawRoles?.Select(r => r.RoleId).ToList() ?? new List<int>();
	[JsonIgnore] public List<Role> Roles { get; set; } = new List<Role>();
}

public class GroupRole
{
	[JsonProperty ("roleId")] public int RoleId { get; set; }
	[JsonProperty ("roleName")] public string? RoleName { get; set; }
}
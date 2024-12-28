namespace RedmineDocs.Helpers;

public static class FieldNames
{
	private static readonly Dictionary<string, string> FieldName = new Dictionary<string, string>()
	{
		{ "assigned_to_id", "roles" },
		{ "author_id", "roles" },
		{ "current_user", "roles" },
		{ "current_user_not", "roles" },
		{ "project_id", "projects" },
		{ "status_id", "issue_statuses"},
		{ "tracker_id", "trackers" },
	};

	public static string GetTableName(string fieldName)
	{
		if (fieldName.StartsWith("cf-"))
		{
			return "custom_fields";
		}

		return FieldName.ContainsKey(fieldName) ? FieldName[fieldName] : null;
	}
}
namespace RedmineDocs.Models
{
	public class Group : BaseModel
	{
		// Дополнительные свойства, специфичные для групп
		public string Type { get; set; } // Должно быть "Group"
        
		// Связи
		public List<Role> Roles { get; set; } = new();
		public List<Project> Projects { get; set; } = new();
	}
}
namespace RedmineDocs.Models;

public class Project : BaseModel
{
	public List<Tracker> Trackers { get; set; } = new();

	public List<Button> Buttons { get; set; } = new();
	
	public List<Role> Roles { get; set; } = new();
	public List<Group> Groups { get; set; } = new();
	
}
namespace RedmineDocs.Models;

public class Tracker : BaseModel
{
	public List<Button> Buttons { get; set; } = new();
	public List<Role> Roles { get; set; } = new();
}
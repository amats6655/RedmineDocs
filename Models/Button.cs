namespace RedmineDocs.Models;

public class Button : BaseModel
{
	public string Type { get; set; }
	public List<Option>? Options { get; set; } = new();
	public List<Action>? Actions { get; set; }
}
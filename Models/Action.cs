namespace RedmineDocs.Models;

public class Action
{
	public string FieldName { get; set; }
	public List<string> Values { get; set; }
	public int IsInverted { get; set; }
	public string ActionType { get; set; }
}
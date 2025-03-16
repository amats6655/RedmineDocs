namespace RedmineDocs.Services.Interfaces;

public interface IGroupService
{
    List<Group> GetGroups(List<Project> projects);
    Dictionary<int, List<Group>> GetProjectGroupsAsync(List<Project> projects);
}
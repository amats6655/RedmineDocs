namespace RedmineDocs.Services.Implementation;

public class GroupService : IGroupService
{
    public List<Group> GetGroups(List<Project> projects)
    {
        Log.Information("Начинаем обработку групп в {count} проектах", projects?.Count ?? 0);
        var groups = new List<Group>();

        foreach (var project in projects)
        {
            if (project.Groups.Count > 0)
            {
                foreach (var group in project.Groups)
                {
                    if (groups.All(x => x.Id != group.Id))
                    {
                        groups.Add(group);
                        Log.Debug("Добавлена новая группа : Id={Id}, Name={Name}", group.Id, group.Name, group.Name);
                    }
                }
            }
        }
        
        Log.Information("Обработка групп завершена. Всего групп: {Count}", groups.Count);
        return groups;
    }

    public Dictionary<int, List<Group>> GetProjectGroupsAsync(List<Project> projects)
    {
        throw new NotImplementedException();
    }
}
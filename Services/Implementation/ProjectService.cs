namespace RedmineDocs.Services.Implementation;

public class ProjectService : IProjectService
{
    private readonly IDataService _dataService;

    public ProjectService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        var projects = await _dataService.GetProjectsAsync();
        Log.Information("Начинаем обработку {Count} проектов", projects?.Count ?? 0);

        if (projects == null || !projects.Any())
        {
            Log.Warning("Список проектов пуст");
            return new List<Project>();
        }

        foreach (var project in projects)
        {
            try
            {
                project.DeserializeJsonFields();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке проекта Id={Id}, Name={Name}",
                    project.Id, project.Name);
            }
        }
        Log.Information("Обработка проектов завершена. Обработано {Count} проектов", projects.Count);
        return projects;
    }
}
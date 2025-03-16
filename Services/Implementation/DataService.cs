using System.Data;
using Dapper;

namespace RedmineDocs.Services.Implementation;

public class DataService : IDataService
{
    private readonly IDbConnection _dbConnection;

    public DataService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }
    
    public async Task<List<Button>> GetButtonsAsync()
    {
        const string buttonsViewQuery = """
                                            SELECT
                                                BTN_ID AS Id,
                                                BTN_NAME AS Name,
                                                BTN_TYPE AS Type,
                                                BTN_DESCRIPTION AS Description,
                                                ACTIONS_JSON AS ActionsJson,
                                                OPTIONS_JSON AS OptionsJson
                                            FROM buttons_view
                                        """;

        try
        {
            var buttons = await _dbConnection.QueryAsync<Button>(buttonsViewQuery);
            Log.Information("Получено {Count} кнопок из базы данных", buttons?.Count() ?? 0);
            return buttons?.ToList() ?? new List<Button>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при получении кнопок из базы данных");
            return new List<Button>();
        }
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        const string projectsViewQuery = """
                                            SELECT 
                                                PROJECT_ID AS Id,
                                                PROJECT_NAME AS Name,
                                                PROJECT_DESCRIPTION AS Description,
                                                PARENT_ID AS ParentId,
                                                TRACKERS_JSON AS TrackersJson,
                                                GROUPS_JSON AS GroupsJson
                                            FROM projects_view;
                                         """;

        try
        {
            var projects = await _dbConnection.QueryAsync<Project>(projectsViewQuery);
            Log.Information("Получено {Count} проектов из базы данных", projects?.Count() ?? 0);

            foreach (var project in projects)
            {
                Log.Debug("Проект: Id={Id}, Name={Name}, ParentId={ParentId}",
                    project.Id, project.Name, project.ParentId);
            }

            return projects?.ToList() ?? new List<Project>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при получении проектов из базы данных");
            return new List<Project>();
        }
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        const string rolesQuery = """
                                    SELECT
                                        id AS Id,
                                        name AS Name,
                                        permissions AS Permissions,
                                        settings AS Settings
                                    FROM roles;
                                  """;

        try
        {
            var roles = await _dbConnection.QueryAsync<Role>(rolesQuery);
            Log.Information("Получено {Count} ролей из базы данных", roles?.Count() ?? 0);

            return roles?.ToList() ?? new List<Role>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при получении ролей из базы данных");
            return new List<Role>();
        }
    }
}
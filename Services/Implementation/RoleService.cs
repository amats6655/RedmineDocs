namespace RedmineDocs.Services.Implementation;

public class RoleService : IRoleService
{
    private readonly IDataService _dataService;

    public RoleService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        var roles = await _dataService.GetRolesAsync();
        Log.Information("Начинаем обработку {Count} ролей", roles?.Count ?? 0);

        if (roles == null || !roles.Any())
        {
            Log.Warning("Список ролей пуст");
            return new List<Role>();
        }

        foreach (var role in roles)
        {
            try
            {
                role.ParseYamlFields();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке роли Id={Id}, Name={Name}",
                    role.Id, role.Name);
            }
        }
        Log.Information("Обработка ролей завершена. Обработано {Count} ролей", roles.Count);
        return roles;
    }
}
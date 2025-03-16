namespace RedmineDocs.Services.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetRolesAsync();
}
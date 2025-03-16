namespace RedmineDocs.Services.Interfaces;

public interface IDataService
{
    Task<List<Button>> GetButtonsAsync();
    Task<List<Project>> GetProjectsAsync();
    Task<List<Role>> GetRolesAsync();
}
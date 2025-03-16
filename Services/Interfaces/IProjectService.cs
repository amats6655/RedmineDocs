namespace RedmineDocs.Services.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetProjectsAsync();
}
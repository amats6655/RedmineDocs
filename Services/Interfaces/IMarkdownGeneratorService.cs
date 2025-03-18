namespace RedmineDocs.Services.Interfaces;

public interface IMarkdownGeneratorService
{
    Task GenerateProjectPage(Project project);
    Task GenerateTrackerPage(Tracker tracker);
    Task GenerateGroupPage(Group group);
    Task GenerateRolePage(Role role);
    Task GenerateButtonPage(Button button);
    Task GenerateIndexPage();
}
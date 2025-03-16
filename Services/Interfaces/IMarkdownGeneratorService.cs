namespace RedmineDocs.Services.Interfaces;

public interface IMarkdownGeneratorService
{
    Task GenerateProjectPage(
        Project project,
        List<Tracker> trackers,
        Dictionary<int, List<Group>> groups,
        List<Role> roles,
        List<Button> buttons);

    Task GenerateTrackerPage(Tracker tracker);
    Task GenerateGroupPage(Group group);
    Task GenerateRolePage(Role role);
    Task GenerateButtonPage(Button button);
    Task GenerateIndexPage();
}
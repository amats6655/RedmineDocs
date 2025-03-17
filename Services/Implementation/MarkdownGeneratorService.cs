namespace RedmineDocs.Services.Implementation;

public class MarkdownGeneratorService : IMarkdownGeneratorService
{
    private readonly TrackerMarkdownGenerator _trackerGenerator;
    private readonly List<Project> _projects;
    private readonly List<Role> _roles;
    private readonly List<Tracker> _trackers;
    private readonly List<Group> _groups;
    private readonly List<Button> _buttons;


    public MarkdownGeneratorService(
        string outputPath,
        List<Project> projects,
        List<Role> roles,
        List<Tracker> trackers,
        List<Group> groups,
        List<Button> buttons)
    {
        _projects = projects;
        _groups = groups;
        _roles = roles;
        _trackers = trackers;
        _buttons = buttons;
        
        _trackerGenerator = new TrackerMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
    }
    
    public Task GenerateProjectPage(Project project, List<Tracker> trackers, Dictionary<int, List<Group>> groups, List<Role> roles, List<Button> buttons)
    {
        throw new NotImplementedException();
    }

    public async Task GenerateTrackerPage(Tracker tracker)
    {
        await _trackerGenerator.GenerateTrackerPage(tracker, _projects);
    }

    public Task GenerateGroupPage(Group group)
    {
        throw new NotImplementedException();
    }

    public Task GenerateRolePage(Role role)
    {
        throw new NotImplementedException();
    }

    public Task GenerateButtonPage(Button button)
    {
        throw new NotImplementedException();
    }

    public Task GenerateIndexPage()
    {
        throw new NotImplementedException();
    }
}
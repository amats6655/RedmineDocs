namespace RedmineDocs.Services.Implementation;

public class MarkdownGeneratorService : IMarkdownGeneratorService
{
    private readonly ProjectMarkdownGenerator _projectGenerator;
    private readonly GroupMarkdownGenerator _groupGenerator;
    private readonly TrackerMarkdownGenerator _trackerGenerator;
    private readonly RoleMarkdownGenerator _roleGenerator;
    private readonly ButtonMarkdownGenerator _buttonGenerator;
    private readonly IndexMarkdownGenerator _indexGenerator;
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
        
        _projectGenerator = new ProjectMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
        _groupGenerator = new GroupMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
        _trackerGenerator = new TrackerMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
        _roleGenerator = new RoleMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
        _buttonGenerator = new ButtonMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
        _indexGenerator = new IndexMarkdownGenerator(outputPath, projects, trackers, roles, groups, buttons);
    }
    
    public async Task GenerateProjectPage(Project project)
    {
        await _projectGenerator.GenerateProjectPage(project, _buttons);
    }

    public async Task GenerateTrackerPage(Tracker tracker)
    {
        await _trackerGenerator.GenerateTrackerPage(tracker, _projects);
    }

    public async Task GenerateGroupPage(Group group)
    {
        await _groupGenerator.GenerateGroupPage(group, _projects);
    }

    public async Task GenerateRolePage(Role role)
    {
        await _roleGenerator.GenerateRolePage(role, _buttons);
    }

    public async Task GenerateButtonPage(Button button)
    {
        await _buttonGenerator.GenerateButtonPage(button);
    }

    public async Task GenerateIndexPage()
    {
        await _indexGenerator.GenerateIndexPage();
    }
}
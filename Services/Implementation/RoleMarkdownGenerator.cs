namespace RedmineDocs.Services.Implementation;

public class RoleMarkdownGenerator : MarkdownGeneratorBase
{
    public RoleMarkdownGenerator(
        string outputPath,
        List<Project> projects,
        List<Tracker> trackers,
        List<Role> roles,
        List<Group> groups,
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }
}
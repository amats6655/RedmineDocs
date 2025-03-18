using System.Text;

namespace RedmineDocs.Services.Implementation;

public class GroupMarkdownGenerator : MarkdownGeneratorBase
{
    public GroupMarkdownGenerator(
        string outputPath,
        List<Project> projects,
        List<Tracker> trackers,
        List<Role> roles,
        List<Group> groups,
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }

    public async Task GenerateGroupPage(Group group, List<Project> projects)
    {
        var sb = new StringBuilder();
        
        AppendHeader(sb, new List<string>(){"Группа"}, group.Name );
        
        sb.AppendLine();
        
        GenerateProjectAndRolesSection(sb, group, projects);
        await SaveToFile(sb, "group", group.Name, group.Id);
    }

    private void GenerateProjectAndRolesSection(StringBuilder sb, Group group, List<Project> projects)
    {
        sb.AppendLine("## 👷‍♂️Роли в проектах");
        sb.AppendLine();

        var projectsWithGroup = projects
            .Where(pvg => pvg.Groups.FirstOrDefault(g => g.Id == group.Id) != null)
            .Select(p => new {Project = p, Groups = group})
            .ToList();
        Log.Debug("Найдено {Count} проектов с группой {GroupName}", projectsWithGroup.Count, group.Name);

        if (projectsWithGroup.Any())
        {
            sb.AppendLine("| *Проект* | *Роли* |");
            sb.AppendLine("|----------|--------|");

            foreach (var pg in projectsWithGroup)
            {
                var isFirstRole = true;

                foreach (var role in pg.Groups.Roles!)
                {
                    if (isFirstRole)
                    {
                        sb.AppendLine(
                            $"| {GetMarkdownLink("project", pg.Project.Name, pg.Project.Id)} | {GetMarkdownLink("role", role.RoleName, role.RoleId)} |");
                        isFirstRole = false;
                    }
                    else
                    {
                        sb.AppendLine($"| | {GetMarkdownLink("role", role.RoleName, role.RoleId)} |");
                    }
                }
            }
        }
        else
        {
            sb.AppendLine("*Группа не используется ни в одном проекте*");
        }
        
        sb.AppendLine();
        sb.AppendLine("---");
    }
}
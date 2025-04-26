using System.Text;

namespace RedmineDocs.Services.Implementation;

public class ProjectMarkdownGenerator : MarkdownGeneratorBase
{
    public ProjectMarkdownGenerator(
        string outputPath,
        List<Project> projects,
        List<Tracker> trackers,
        List<Role> roles,
        List<Group> groups,
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }

    public async Task GenerateProjectPage(Project project, List<Button> buttons)
    {
        var sb = new StringBuilder();

        var tags = new List<string>() { "Проект" };
        if (project.ParentId != null)
        {
            tags.Add(Projects.Where(p => p.Id == project.ParentId).FirstOrDefault()?.Name);
        }
        AppendHeader(sb, tags, project.Name);

        GenerateOverviewSection(sb, project);
        
        GenerateTrackersSection(sb, project);

        GenerateGroupAndRolesSection(sb, project);

        GenerateButtonsSection(sb, project, Buttons);

        await SaveToFile(sb, "project", project.Name, project.Id);
    }

    private void GenerateOverviewSection(StringBuilder sb, Project project)
    {
        sb.AppendLine("## 📝 Обзор");
        sb.AppendLine();
        
        if(!string.IsNullOrEmpty(project.Description))
            sb.AppendLine(project.Description);
        
        sb.AppendLine();
        sb.AppendLine("---");
    }

    private void GenerateTrackersSection(StringBuilder sb, Project project)
    {
        sb.AppendLine("## 👁️ Связанные трекеры");
        sb.AppendLine();

        if (project.Trackers.Any())
        {
            sb.AppendLine("| *ID* | *Название трекера* |");
            sb.AppendLine("|------|--------------------|");

            foreach (var tracker in project.Trackers)
            {
                sb.AppendLine($"| {tracker.Id} | {GetMarkdownLink("tracker", tracker.Name, tracker.Id)}|");
            }
        }
        else
            sb.AppendLine("*У проекта нет связанных трекеров*");
        sb.AppendLine();
        sb.AppendLine("---");
    }

    private void GenerateGroupAndRolesSection(StringBuilder sb, Project project)
    {
        sb.AppendLine("## 👷‍♂️ Группы пользователей и роли");
        sb.AppendLine();

        if (project.Groups.Any())
        {
            sb.AppendLine("| *Группа* | *Роль* |");
            sb.AppendLine("|----------|--------|");

            foreach (var group in project.Groups)
            {
                var isFirstRole = true;
                foreach (var role in group.Roles)
                {
                    if (isFirstRole)
                    {
                        sb.AppendLine(
                            $"| {GetMarkdownLink("group", group.Name, group.Id)} | {GetMarkdownLink("role", role.RoleName, role.RoleId)} |");
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
            sb.AppendLine("*У проекта нет настроенных групп пользователей*");
        }
        
        sb.AppendLine();
        sb.AppendLine("---");
    }

    private void GenerateButtonsSection(StringBuilder sb, Project project, List<Button> buttons)
    {
        sb.AppendLine("## ▶️ Доступные кнопки");
        sb.AppendLine();

        var projectWithParents = new List<Project>();
        var projectIds = new HashSet<int>();
        var tempProject = project;

        while (tempProject != null && !projectIds.Contains(tempProject.Id))
        {
            projectWithParents.Add(tempProject);
            projectIds.Add(tempProject.Id);

            tempProject = tempProject.ParentId.HasValue 
                ? Projects.FirstOrDefault(p => p.Id == tempProject.ParentId.Value) 
                : null;
        }

        var buttonsForProject = buttons
            .Where(button =>
            {
                if (button.IsUniversalForProjects)
                    return true;
                bool hasAssociatedProject = button.AssociatedProjectIds.Intersect(projectIds).Any();
                return button.IsProjectInverted ? !hasAssociatedProject : hasAssociatedProject;
            })
            .OrderBy(b => b.Name)
            .ToList();

        if (buttonsForProject.Any())
        {
            sb.AppendLine("| *Название кнопки* | *Доступна в трекерах* | *Доступна для ролей* |");
            sb.AppendLine("|-------------------|-----------------------|----------------------|");

            foreach (var button in buttonsForProject)
            {
                string trackerNames;
                string roleNames;
                
                var trackerLinks = button.AssociatedTrackerIds
                    .Where(id => Trackers.Any(t => t.Id == id))
                    .Select(GetTrackerName)
                    .Where(name => name != null)
                    .Select(name =>
                    {
                        var tracker = Trackers.FirstOrDefault(t => t.Name == name);
                        return tracker != null ? GetMarkdownLink("tracker", name, tracker.Id) : name;
                    })
                    .ToList();
                
                var roleLinks = button.AssociatedRoleIds
                    .Where(id => Roles.Any(r => r.Id == id))
                    .Select(GetRoleName)
                    .Where(name => name != null)
                    .Select(name =>
                    {
                        var role = Roles.FirstOrDefault(r => r.Name == name);
                        return role != null ? GetMarkdownLink("role", name, role.Id) : name;
                    })
                    .ToList();
                
                if (button.IsUniversalForRoles)
                {
                    roleNames = "*Все роли*";
                }
                else if (button.IsProjectInverted)
                {
                    roleNames = roleLinks.Any()
                        ? $"*Кроме* {string.Join(", ", roleLinks)}"
                        : "*Все роли*";
                }
                else
                {
                    roleNames = roleLinks.Any()
                        ? string.Join(", ", roleLinks)
                        : "*Нет доступных ролей*";
                }
                    
                if (button.IsUniversalForTrackers)
                {
                    trackerNames = "*Все трекеры*";
                }
                else if (button.IsTrackerInverted)
                {
                    trackerNames = trackerLinks.Any()
                        ? $"*Кроме* {string.Join(", ", trackerLinks)}"
                        : "*Все трекеры*";
                }
                else
                {
                    trackerNames = trackerLinks.Any()
                        ? string.Join(", ", trackerLinks)
                        : "*Нет доступных трекеров*";
                }
                sb.AppendLine(
                    $"| {GetMarkdownLink("button", button.Name, button.Id)} | {trackerNames} | {roleNames} |");
            }
            sb.AppendLine();
        }
    }
}
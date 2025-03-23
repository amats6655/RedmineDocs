using System.Text;

namespace RedmineDocs.Services.Implementation;

public class TrackerMarkdownGenerator : MarkdownGeneratorBase
{
    public TrackerMarkdownGenerator(
        string outputPath, 
        List<Project> projects, 
        List<Tracker> trackers, 
        List<Role> roles, 
        List<Group> groups, 
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }

    public async Task GenerateTrackerPage(Tracker tracker, List<Project> projects)
    {
        var sb = new StringBuilder();
        
        AppendHeader(sb, new List<string>{"Трекер"}, tracker.Name);

        GenerateProjectsSection(sb, tracker, projects);

        GenerateRolesAndFunctionalitySection(sb, tracker);
        
        await SaveToFile(sb, "трекер", tracker.Name, tracker.Id);
    }

    private void GenerateProjectsSection(StringBuilder sb, Tracker tracker, List<Project> projects)
    {
        sb.AppendLine("## 🗃️ Связанные проекты");
        sb.AppendLine();
        
        var associatedProjects = projects
            .Where(project => project.Trackers?
                .Any(t => t.Id == tracker.Id) == true).ToList();

        if (associatedProjects.Any())
        {
            sb.AppendLine("| *Проект* | *Описание* |");
            sb.AppendLine("|----------|------------|");

            foreach (var project in associatedProjects.OrderBy(project => project.Name))
            {
                sb.AppendLine($"| {GetMarkdownLink("проект", project.Name, project.Id)} | {project.Description ?? "---"} |");
            }
        }
        else
        {
            sb.AppendLine("*Трекер не используется ни в одном проекте*");
        }
        sb.AppendLine();
        sb.AppendLine("---");
    }

    private void GenerateRolesAndFunctionalitySection(StringBuilder sb, Tracker tracker)
    {
        sb.AppendLine("## 🔒 Роли и функционал");
        sb.AppendLine();

        var rolesWithAccess = new Dictionary<int, Dictionary<string, bool>>();

        foreach (var role in Roles)
        {
            var accessRights = new Dictionary<string, bool>();

            if (role.ParsedSettings != null && role.ParsedSettings.PermissionsAllTrackers!.ContainsValue("1"))
            {
                if(role.ParsedSettings.PermissionsAllTrackers.TryGetValue("view_issues", out var viewAccess)
                   && viewAccess == "1")
                {
                    accessRights["view"] = true;
                }
                
                if (role.ParsedSettings.PermissionsAllTrackers.TryGetValue("add_issues", out var addAccess) 
                    && addAccess == "1")
                {
                    accessRights["add"] = true;
                }
                
                if(role.ParsedSettings.PermissionsAllTrackers.TryGetValue("edit_issues", out var editAccess) 
                   && editAccess == "1")
                {
                    accessRights["edit"] = true;
                }
                
                if(role.ParsedSettings.PermissionsAllTrackers.TryGetValue("add_issue_notes", out var commentAccess) 
                   && commentAccess == "1")
                {
                    accessRights["comment"] = true;
                }
            }

            if (role.ParsedSettings?.PermissionsTrackerIds != null)
            {
                if (role.ParsedSettings.PermissionsTrackerIds.TryGetValue("view_issues", out var viewTrackers)
                    && viewTrackers.Contains($"'{tracker.Id}'"))
                {
                    accessRights["view"] = true;
                }

                if (role.ParsedSettings.PermissionsTrackerIds.TryGetValue("add_issues", out var addTrackers)
                    && addTrackers.Contains($"'{tracker.Id}'"))
                {
                    accessRights["add"] = true;
                }

                if (role.ParsedSettings.PermissionsTrackerIds.TryGetValue("edit_issues", out var editTrackers)
                    && editTrackers.Contains($"'{tracker.Id}'"))
                {
                    accessRights["edit"] = true;
                }

                if (role.ParsedSettings.PermissionsTrackerIds.TryGetValue("comment_issues", out var commentTrackers)
                    && commentTrackers.Contains($"'{tracker.Id}'"))
                {
                    accessRights["comment"] = true;
                }
            }

            if (accessRights.Any(r => r.Value))
            {
                rolesWithAccess[role.Id] = accessRights;
            }
        }

        if (rolesWithAccess.Any())
        {
            sb.AppendLine("| *Роль* | *Функционал* |");
            sb.AppendLine("|--------|--------------|");
            
            foreach (var roleEntry in rolesWithAccess.OrderBy(r => Roles.FirstOrDefault(role => role.Id == r.Key)?.Name ?? ""))
            {
                var role = Roles.FirstOrDefault(r => r.Id == roleEntry.Key);
                if(role == null) continue;
                
                var accessRights = roleEntry.Value;
                var functionalityList = new List<string>();
                
                if(accessRights.TryGetValue("view", out var canView) && canView)
                    functionalityList.Add("👀 Просмотр заявок");
                if(accessRights.TryGetValue("add", out var canAdd) && canAdd)
                    functionalityList.Add("➕ Создание заявок");
                if(accessRights.TryGetValue("edit", out var canEdit) && canEdit)
                    functionalityList.Add("📝 Редактирование заявок");
                if(accessRights.TryGetValue("comment", out var canComment) && canComment)
                    functionalityList.Add("🗨️ Добавление комментариев");
                
                string functionalityText = string.Join(",<br>", functionalityList);
                sb.AppendLine($"| {GetMarkdownLink("роль", role.Name, role.Id)} | {functionalityText} |");
            }
        }
        else
        {
            sb.AppendLine("*Нет ролей с настроенным доступом к этому трекеру*");
        }
        
        sb.AppendLine();
        sb.AppendLine("---");
    }
}
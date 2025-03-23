using System.Text;
using RedmineDocs.Resources;

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

    public async Task GenerateRolePage(Role role, List<Button> buttons)
    {
        var sb = new StringBuilder();
        
        AppendHeader(sb, new List<string>(){"Роль"}, role.Name);

        GeneratePermissionsSection(sb, role);

        GenerateSettingsSettings(sb, role);

        GenerateButtonsSection(sb, role, buttons);

        await SaveToFile(sb, "роль", role.Name, role.Id);
    }

    private void GeneratePermissionsSection(StringBuilder sb, Role role)
    {
        if (role.ParsedPermissions?.Any() == true)
        {
            sb.AppendLine("## 🔑 Разрешения");
            sb.AppendLine();

            foreach (var permission in role.ParsedPermissions ?? new List<string>())
            {
                string translatedPermission = Translations.Get(permission);
                sb.AppendLine($"- {translatedPermission}");
            }

            sb.AppendLine();
        }
    }

    private void GenerateSettingsSettings(StringBuilder sb, Role role)
    {
        if (role.ParsedSettings != null)
        {
            sb.AppendLine("## ⚙️ Настройки");
            sb.AppendLine();

            GenerateAllTrackersPermissionsSection(sb, role);
            GenerateTrackerPermissionsSettings(sb, role);
        }
    }
    
    private void GenerateAllTrackersPermissionsSection(StringBuilder sb, Role role)
    {
        if (role.ParsedSettings!.PermissionsAllTrackers?.Any() == true)
        {
            sb.AppendLine("### Разрешения для всех трекеров");
            sb.AppendLine();
            sb.AppendLine("| *Разрешение* | *Значение* |");
            sb.AppendLine("|--------------|------------|");

            foreach (var setting in role.ParsedSettings.PermissionsAllTrackers)
            {
                if (string.Equals(setting.Key, "delete_issues"))
                    continue;
                var translatedName = Translations.Get(":" + setting.Key);
                var settingValue = setting.Value == "1" ? "Да" : "Нет";
                sb.AppendLine($"| {translatedName} | {settingValue} |");
            }
        }
    }

    private void GenerateTrackerPermissionsSettings(StringBuilder sb, Role role)
    {
        sb.AppendLine("### Разрешения для отдельных трекеров");
        sb.AppendLine();
        
        if (role.ParsedSettings!.PermissionsTrackerIds?.Any() == true)
        {
            foreach (var setting in role.ParsedSettings.PermissionsTrackerIds)
            {
                if (string.Equals(setting.Key, "delete_issues"))
                    continue;
                var translatedName = Translations.Get(":" + setting.Key);
                sb.AppendLine($"### Разрешение _{translatedName}_");
                sb.AppendLine();

                if (setting.Value.Count > 0)
                {
                    sb.AppendLine("| *Трекер* |");
                    sb.AppendLine("|----------|");

                    foreach (var trackerId in setting.Value)
                    {
                        var tracker = Trackers.FirstOrDefault(t => t.Id == int.Parse(trackerId));
                        if (tracker == null)
                        {
                            Log.Debug("Не найден, либо больше не используется трекер с ID={Id}. Почисти настройки роли {RoleId}, {RoleName}",
                                trackerId, role.Id, role.Name);
                            continue;
                        }
                        sb.AppendLine($"| {GetMarkdownLink("трекер", tracker.Name, tracker.Id)} |)");
                    }

                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("*Нет связанных трекеров*");
                    sb.AppendLine();
                }
            }
        }
    }
    
    private void GenerateButtonsSection(StringBuilder sb, Role role, List<Button> buttons)
    {
        if (buttons?.Any() == true)
        {
            var buttonsForRole = buttons
                .Where(button =>
                {
                    // Проверяем ограничение current_user_not
                    var currentUserNotOption =
                        button.Options?.FirstOrDefault(o => string.Equals(o.FieldName, "current_user_not"));
                    
                    // Если кнопка для всех ролей и при этом параметра current_user_not не существует, вернуть истину
                    if (button.IsUniversalForRoles && currentUserNotOption == null)
                        return true;
                    bool standardRoleCheck = button.IsRoleInverted
                        ? !button.AssociatedRoleIds.Contains(role.Id)
                        : button.AssociatedRoleIds.Contains(role.Id);
                    
                    if (currentUserNotOption != null && currentUserNotOption.Values.Any())
                    {
                        var excludeRoleIds = currentUserNotOption.Values
                            .Where(v => !string.IsNullOrEmpty(v.Id))
                            .Select(v => int.TryParse(v.Id, out int roleId) ? roleId : 0)
                            .Where(id => id > 0)
                            .ToList();

                        if (excludeRoleIds.Contains(role.Id))
                            return false;
                        return true;
                    }
                    return standardRoleCheck;
                })
                .OrderBy(b => b.Name)
                .ToList();

            if (buttonsForRole.Any())
            {
                sb.AppendLine("## ▶️ Доступные кнопки");
                sb.AppendLine();
                sb.AppendLine("| *Кнопка* | *Доступна в проектах* | *Доступна в трекерах* |");
                sb.AppendLine("|----------|-----------------------|-----------------------|");

                foreach (var button in buttonsForRole)
                {
                    string projectNames;
                    string trackerNames;
                    var projectLinks = button.AssociatedProjectIds
                        .Where(id => Projects.Any(p => p.Id == id))
                        .Select(GetProjectName)
                        .Where(name => name != null)
                        .Select(name =>
                        {
                            var project = Projects.FirstOrDefault(p => p.Name == name);
                            return project != null ? GetMarkdownLink("project", name, project.Id) : name;
                        })
                        .ToList();
                    
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

                    if (button.IsUniversalForProjects)
                    {
                        projectNames = "*Все проекты*";
                    }
                    else if (button.IsProjectInverted)
                    {
                        projectNames = projectLinks.Any()
                            ? $"*Кроме* {string.Join(", ", projectLinks)}"
                            : "*Все проекты*";
                    }
                    else
                    {
                        projectNames = projectLinks.Any()
                            ? string.Join(", ", projectLinks)
                            : "*Нет доступных проектов*";
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
                        $"| {GetMarkdownLink("button", button.Name, button.Id)} | {projectNames} | {trackerNames}");
                }
                sb.AppendLine();
            }
        }
    }
}
using System.Text;
using RedmineDocs.Resources;

namespace RedmineDocs.Services.Implementation;

public class ButtonMarkdownGenerator : MarkdownGeneratorBase
{
    public ButtonMarkdownGenerator(
        string outputPath,
        List<Project> projects,
        List<Tracker> trackers,
        List<Role> roles,
        List<Group> groups,
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }

    public async Task GenerateButtonPage(Button button)
    {
        var sb = new StringBuilder();
        
        AppendHeader(sb, new List<string>{"Кнопка", button.Type}, button.Name);

        sb.AppendLine("## 📝 Описание");
        sb.AppendLine();
        sb.AppendLine($"**Тип кнопки:** {button.Type}");
        sb.AppendLine();
        if(!string.IsNullOrWhiteSpace(button.Description))
            sb.AppendLine(button.Description);
        sb.AppendLine();

        bool invertedRoles = button.IsRoleInverted;

        var currentUserNotOption = button.Options!.FirstOrDefault(o => string.Equals(o.FieldName, "current_user_not"));
        if (currentUserNotOption != null)
        {
            button.Options!.Add(new ButtonOption{ FieldName = "current_user", Invert = 1, Values = currentUserNotOption.Values });
            invertedRoles = false;
        }

        GenerateProjectAvailabilitySection(sb, button);

        GenerateTrackerAvailabilitySection(sb, button);

        // Учитываем инвертированные роли.
        GenerateRoleAvailabilitySection(sb, button, invertedRoles);

        GenerateButtonActionsSection(sb, button);
        
        await SaveToFile(sb, "button", button.Name, button.Id);
    }

    private void GenerateProjectAvailabilitySection(StringBuilder sb, Button button)
    {
        sb.AppendLine("## 🗃️ Доступность в проектах");
        sb.AppendLine();

        if (button.IsUniversalForProjects)
        {
            sb.AppendLine("*Кнопка доступна во всех проектах*");
        }
        else
        {
            var validProjectIds = button.AssociatedProjectIds
                .Where(id => Projects.Any(p => p.Id == id))
                .ToList();

            if (button.IsProjectInverted)
            {
                sb.AppendLine("*Кнопка доступна во всех проектах, кроме перечисленных:*");
            }
            else
            {
                sb.AppendLine("*Кнопка доступна только в перечисленных проектах:*");
            }
            
            sb.AppendLine();
            sb.AppendLine("| *ID* | *Проект* | *Описание* |");
            sb.AppendLine("|------|----------|------------|");

            foreach (var projectId in validProjectIds)
            {
                var project = Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    sb.AppendLine(
                        $"| {project.Id} | {GetMarkdownLink("project", project.Name, project.Id)} | {project.Description ?? "---"} |");
                }
            }
        }
        sb.AppendLine();
    }

    private void GenerateTrackerAvailabilitySection(StringBuilder sb, Button button)
    {
        sb.AppendLine("## 👁️ Доступность в трекерах");
        sb.AppendLine();

        if (button.IsUniversalForTrackers)
            sb.AppendLine("*Кнопка доступна во всех трекерах*");
        else
        {
            if (button.IsTrackerInverted)
                sb.AppendLine("*Кнопка доступна во всех трекерах, кроме перечисленных:*");
            else
                sb.AppendLine("*Кнопка доступна только в перечисленных трекерах:*");
            
            sb.AppendLine();
            sb.AppendLine("| *ID* | *Трекер* |");
            sb.AppendLine("|------|-----------|");
            
            var validTrackerIds = button.AssociatedTrackerIds
                .Where(id => Trackers.Any(p => p.Id == id))
                .ToList();

            foreach (var trackerId in validTrackerIds)
            {
                var tracker = Trackers.FirstOrDefault(t => t.Id == trackerId);
                if (tracker != null)
                    sb.AppendLine($"| {tracker.Id} | {GetMarkdownLink("tracker", tracker.Name, tracker.Id)} |");
            }
        }
        
        sb.AppendLine();
    }

    private void GenerateRoleAvailabilitySection(StringBuilder sb, Button button, bool invertedRoles)
    {
        sb.AppendLine("## 👷‍♂️ Доступность для ролей");
        sb.AppendLine();

        if (button.IsUniversalForRoles)
            sb.AppendLine("*Кнопка доступна для всех ролей*");
        else
        {
            if (invertedRoles)
                sb.AppendLine("*Кнопка доступна для всех ролей, кроме перечисленных:*");
            else
                sb.AppendLine("*Кнопка доступна только для перечисленных ролей:*");

            sb.AppendLine();
            sb.AppendLine("| *ID* | *Роль* |");
            sb.AppendLine("|------|--------|");

            foreach (var roleId in button.AssociatedRoleIds)
            {
                var role = Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                    sb.AppendLine($"| {role.Id} | {GetMarkdownLink("role", role.Name, role.Id)} |");
            }
        }
        
        sb.AppendLine();
    }

    private void GenerateButtonActionsSection(StringBuilder sb, Button button)
    {
        sb.AppendLine("## ⚡ Действия");
        sb.AppendLine();

        if (button.Actions?.Any() == true)
        {
            sb.AppendLine("| *Действие* | *Поле*| *Значение* |");
            sb.AppendLine("|------------|-------|------------|");

            foreach (var action in button.Actions)
            {
                string actionTranslation = Translations.Get(action.Action);

                string fieldName = action.FieldName.Replace("_id", "");
                string fieldTranslation = Translations.Get(fieldName);

                if (action.Values?.Any() == true)
                {
                    var isFirstValue = true;
                    foreach (var value in action.Values)
                    {
                        if (isFirstValue)
                        {
                            sb.AppendLine($"| **{actionTranslation}** | **{fieldTranslation}** | {value.Value} |");
                            isFirstValue = false;
                        }
                        else
                            sb.AppendLine($"| | | {value.Value} |");
                    }
                }
            }
        }
        else
            sb.AppendLine("*У кнопки нет настроенных действий*");
    }
}
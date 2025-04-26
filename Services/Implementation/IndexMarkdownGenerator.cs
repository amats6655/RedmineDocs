using System.Text;

namespace RedmineDocs.Services.Implementation;

public class IndexMarkdownGenerator : MarkdownGeneratorBase
{
    public IndexMarkdownGenerator(
        string outputPath, 
        List<Project> projects, 
        List<Tracker> trackers, 
        List<Role> roles, 
        List<Group> groups, 
        List<Button> buttons)
        : base(outputPath, projects, trackers, roles, groups, buttons)
    {
    }

    public async Task GenerateIndexPage()
    {
        var sb = new StringBuilder();
        
        AppendHeader(sb, new List<string>(), "Welcome to RedmineDocs");

        sb.AppendLine(
            ">[!summary] Добро пожаловать в документацию системы **[ServiceDesk](https://sd.talantiuspeh.ru)** на базе **Redmine 3.4.10.**");
        sb.AppendLine();

        GenerateBody(sb, Projects, Trackers, Roles, Groups, Buttons);

        await SaveToFile(sb, "index");
    }

    private void GenerateBody(StringBuilder sb, List<Project> projects, List<Tracker> trackers, List<Role> roles, List<Group> groups, List<Button> buttons)
    {
        sb.AppendLine("## Основные компоненты");
        sb.AppendLine();
        sb.AppendLine($"#### **[Проекты](Projects/index)**: ({Projects.Count})");
        sb.AppendLine();
        sb.AppendLine($"#### **[Роли](Roles/index)**: Определяют права доступа пользователей. ({Roles.Count})");
        sb.AppendLine();
        sb.AppendLine($"#### **[Трекеры](Trackers/index)**: Категоризируют типы задач и заявок в проектах. ({Trackers.Count})");
        sb.AppendLine($"#### **[Группы](Groups/index)**: Собирательные единицы для управления пользователями. ({Groups.Count})");
        sb.AppendLine("Группа предоставляет своим участникам определенные роли в проектах.");
        sb.AppendLine("Роль определяет список доступных трекеров для просмотра или изменения");
        sb.AppendLine($"#### **[Кнопки](Buttons/index)**: Расширяют возможности настройки рабочих процессов с помощью настраиваемых кнопок. ({Buttons.Count})");
    }
}
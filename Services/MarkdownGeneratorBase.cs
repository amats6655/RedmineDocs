using System.Text;
using RedmineDocs.Resources;

namespace RedmineDocs.Services;

public abstract class MarkdownGeneratorBase
{
    public readonly string OutputPath;
    protected readonly List<Project> Projects;
    protected readonly List<Tracker> Trackers;
    protected readonly List<Role> Roles;
    protected readonly List<Group> Groups;
    protected readonly List<Button> Buttons;

    protected readonly string ProjectPath;
    protected readonly string TrackersPath;
    protected readonly string RolesPath;
    protected readonly string GroupsPath;
    protected readonly string ButtonsPath;

    public MarkdownGeneratorBase(
        string outputPath, 
        List<Project> projects, 
        List<Tracker> trackers,
        List<Role> roles,
        List<Group> groups,
        List<Button> buttons)
    {
        OutputPath = outputPath;
        Directory.CreateDirectory(OutputPath);
        
        ProjectPath = Path.Combine(outputPath, "Projects");
        TrackersPath = Path.Combine(OutputPath, "Trackers");
        RolesPath = Path.Combine(OutputPath, "Roles");
        GroupsPath = Path.Combine(OutputPath, "Groups");
        ButtonsPath = Path.Combine(OutputPath, "Buttons");
        
        Directory.CreateDirectory(ProjectPath);
        Directory.CreateDirectory(TrackersPath);
        Directory.CreateDirectory(RolesPath);
        Directory.CreateDirectory(GroupsPath);
        Directory.CreateDirectory(ButtonsPath);
        
        Projects = projects;
        Trackers = trackers;
        Roles = roles;
        Groups = groups;
        Buttons = buttons;
    }

    protected string GetTrackerName(int trackerId) =>
        Trackers.FirstOrDefault(t => t.Id == trackerId)?.Name ?? string.Empty;
    
    protected string GetRoleName(int roleId) =>
        Roles.FirstOrDefault(r => r.Id == roleId)?.Name ?? string.Empty;
    
    protected string GetProjectName(int projectId) =>
        Projects.FirstOrDefault(p => p.Id == projectId)?.Name ?? string.Empty;


    /// <summary>
    /// Получает имя файла для сущности
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="entityId"></param>
    /// <param name="name"></param>
    protected string GetEntityFileName(string entityType, int entityId, string name = null)
    {
        string typePrefix = entityType.ToLower() switch
        {
            "project" or "проект" => "project",
            "tracker" or "трекер" => "tracker",
            "role" or "роль" => "role",
            "group" or "группа" => "group",
            "button" or "кнопка" => "button",
            _ => entityType.ToLower()
        };

        string fileSuffix = $"{typePrefix}_{entityId}";
        return fileSuffix;
    }

    /// <summary>
    /// Получает относительный путь к файлу для создания ссылки Markdown
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="name"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    protected string GetEntityPath(string entityType, string name, int entityId)
    {
        var fileName = GetEntityFileName(entityType, entityId, name);

        switch (entityType.ToLower())
        {
            case "project":
            case "проект":
                return Path.Combine("Projects", fileName).Replace("\\", "/");
            case "tracker":
            case "трекер":
                return Path.Combine("Trackers", fileName).Replace("\\", "/");
            case "role":
            case "роль":
                return Path.Combine("Roles", fileName).Replace("\\", "/");
            case "group":
            case "группа":
                return Path.Combine("Groups", fileName).Replace("\\", "/");
            case "button":
            case "кнопки":
                return Path.Combine("Buttons", fileName).Replace("\\", "/");
            default:
                return fileName;
            
        }
    }

    /// <summary>
    /// Получает полный путь к файлу для сохранения
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="name"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    protected string GetSafeFilePath(string entityType, string name, int entityId)
    {
        var fileName = GetEntityFileName(entityType, entityId, name);
        
        switch (entityType.ToLower())
        {
            case "project":
            case "проект":
                return Path.Combine(ProjectPath, fileName + ".md");
            case "tracker":
            case "трекер":
                return Path.Combine(TrackersPath, fileName + ".md");
            case "role":
            case "роль":
                return Path.Combine(RolesPath, fileName + ".md");
            case "group":
            case "группа":
                return Path.Combine(GroupsPath, fileName + ".md");
            case "button":
            case "кнопки":
                return Path.Combine(ButtonsPath, fileName + ".md");
            default:
                return Path.Combine(OutputPath, fileName + ".md");
            
        }
    }

    /// <summary>
    /// Получает markdown-ссылку на сущность 
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="name"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    protected string GetMarkdownLink(string entityType, string name, int entityId)
    {
        var relPath = GetEntityPath(entityType, name, entityId);
        var escapedPath = relPath.Replace(" ", "%20");
        
        return $"[{name}]({escapedPath})";
    }

    protected async Task SaveToFile(StringBuilder content, string entityType, string name, int entityId)
    {
        var fileName = GetSafeFilePath(entityType, name, entityId);
        await File.WriteAllTextAsync(fileName, content.ToString());
    }
    
    protected async Task SaveToFile(StringBuilder content, string name)
    {
        var fileName = Path.Combine(OutputPath, name + ".md");
        await File.WriteAllTextAsync(fileName, content.ToString());
    }

    /// <summary>
    /// Добавляем заголовок markdown страницы
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="tags"></param>
    /// <param name="title"></param>
    protected void AppendHeader(StringBuilder sb, List<string> tags, string title)
    {
        sb.AppendLine("---");
        sb.AppendLine("tags:");
        foreach(var tag in tags)
            sb.AppendLine($"  - {tag}");
        sb.AppendLine($"title: {title}");
        sb.AppendLine("draft: false");
        sb.AppendLine("---");
        sb.AppendLine();
    }
}

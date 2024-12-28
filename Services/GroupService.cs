using System.Data;
using Dapper;
using RedmineDocs.Models;
using RedmineDocs.Services;

public class GroupService
{
    private readonly IDbConnection _dbConnection;
    private readonly PageGeneratorService _pageGenerator;

    public GroupService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
        _pageGenerator = new PageGeneratorService();
    }

    /// <summary>
    /// Получает список групп, включая их роли и проекты.
    /// Если передан projectId, возвращает только группы, связанные с этим проектом.
    /// </summary>
    /// <param name="projectId">ID проекта (опционально).</param>
    /// <returns>Список групп.</returns>
public async Task<List<Group>> GetGroupsAsync(int? projectId = null)
{
    string groupsQuery = @"
        SELECT 
            g.id AS GroupId,
            g.firstname AS Name,
            g.lastname AS Description,
            p.id AS ProjectId,
            p.name AS ProjectName,
            r.id AS RoleId,
            r.name AS RoleName,
            r.settings AS RoleSettings
        FROM users g
        JOIN members m ON m.user_id = g.id
        JOIN member_roles mr ON mr.member_id = m.id
        JOIN roles r ON r.id = mr.role_id
        JOIN projects p ON p.id = m.project_id
        WHERE g.type = 'Group'";

    // Добавляем условие для фильтрации по projectId
    if (projectId.HasValue)
    {
        groupsQuery += " AND p.id = @ProjectId";
    }

    groupsQuery += " ORDER BY g.id, p.id, r.id";

    var result = await _dbConnection.QueryAsync<GroupProjectRoleDto>(groupsQuery, new { ProjectId = projectId });

    // Группировка данных
    var groups = result
        .GroupBy(g => new { g.GroupId, g.Name, g.Description })
        .Select(group => new Group
        {
            Id = group.Key.GroupId,
            Name = group.Key.Name,
            Description = group.Key.Description,
            Projects = group
                .Where(p => p.ProjectId != 0) // Убедитесь, что проект связан
                .GroupBy(p => new { p.ProjectId, p.ProjectName })
                .Select(project => new Project
                {
                    Id = project.Key.ProjectId,
                    Name = project.Key.ProjectName,
                    Roles = project
                        .Where(r => r.RoleId != 0) // Убедитесь, что роль связана
                        .GroupBy(r => new { r.RoleId, r.RoleName, r.RoleSettings })
                        .Select(role => new Role
                        {
                            Id = role.Key.RoleId,
                            Name = role.Key.RoleName,
                            Settings = role.Key.RoleSettings,
                        }).ToList()
                }).ToList()
        }).ToList();
    
    // Получаем все трекеры из базы данных
    string trackersQuery = @"
		        SELECT id, name
		        FROM trackers";

    var trackerIdNamePairs = await _dbConnection.QueryAsync<(int Id, string Name)>(trackersQuery);

    // Создаем словарь ID трекера -> Название трекера
    var trackerIdToName = trackerIdNamePairs.ToDictionary(pair => pair.Id.ToString(), pair => pair.Name);

    // Парсим настройки ролей
    foreach (var group in groups)
    {
        foreach (var project in group.Projects)
        {
            foreach (var role in project.Roles)
            {
                role.ParseSettings(trackerIdToName);
            }
        }
    }

    return groups;
}

    /// <summary>
    /// Генерирует страницу документации для группы.
    /// </summary>
    /// <param name="group">Группа.</param>
    public void GenerateGroupPage(Group group)
    {
        string obsTemplatePath = "Templates/ObsGroupTemplate.sbn";
        string templatePath = "Templates/GroupTemplate.sbn";
        string obsOutputDir = Path.Combine("Obsidian", "Groups");
        string outputDir = Path.Combine("Output", "Groups");
        string fileName = $"Группа {group.Description}";

        _pageGenerator.GeneratePage(templatePath, new { group }, outputDir, fileName);
        _pageGenerator.GeneratePage(obsTemplatePath, new { group }, obsOutputDir, fileName);
    }
    
    /// <summary>
    /// DTO для обработки данных групп, проектов и ролей.
    /// </summary>
    public class GroupProjectRoleDto
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleSettings { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySqlConnector;
using RedmineDocs.Models;

namespace RedmineDocs.Services
{
    public class ProjectService
    {
        private readonly string _connectionString;
        private readonly List<Button> _allButtons;
        private readonly List<Role> _allRoles;
        private readonly GroupService _groupService;
        private readonly TrackerService _trackerService;
        private readonly PageGeneratorService _pageGenerator;
        

        private const string ProjectsQuery = "SELECT id, name, description FROM projects WHERE status = 1";
        private const string TrackersQuery = @"
            SELECT t.id, t.name, t.description
            FROM trackers t
            JOIN projects_trackers pt ON t.id = pt.tracker_id
            WHERE pt.project_id = @ProjectId";

        private const string RolesQuery = @"
                SELECT DISTINCT r.id, r.name
                FROM roles r
                JOIN member_roles mr ON r.id = mr.role_id
                JOIN members m ON mr.member_id = m.id
                WHERE m.project_id = @ProjectId";

        public ProjectService(string connectionString, List<Button> allButtons, List<Role> allRoles)
        {
            _connectionString = connectionString;
            _pageGenerator = new PageGeneratorService();
            var dbConnection = new MySqlConnection(_connectionString);
            _allRoles = allRoles;
            _allButtons = allButtons;
            _groupService = new GroupService(dbConnection);
            _trackerService = new TrackerService(dbConnection, allButtons, allRoles);
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            var projects = new List<Project>();

            try
            {
                using IDbConnection dbConnection = new MySqlConnection(_connectionString);
                dbConnection.Open();

                projects = (await dbConnection.QueryAsync<Project>(ProjectsQuery)).ToList();

                foreach (var project in projects)
                {
                    Console.WriteLine($"Обработка проекта: {project.Name}");
                    
                    // Загрузка трекеров
                    project.Trackers = (await _trackerService.GetTrackersAsync(project.Id)).ToList();
                    
                    
                    // Загрузка групп через GroupService (только группы с назначенными ролями)
                    project.Groups = (await _groupService.GetGroupsAsync(project.Id)).ToList();

                    // Загрузка кнопок через ButtonService
                    project.Buttons = GetButtonsForProject(_allButtons, project.Name, project.Trackers, project.Groups);


                }

                dbConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникла ошибка при подключении к базе данных: {ex.Message}");
            }

            return projects;
        }

        public void PageGenerate(List<Project> projects)
        {
            foreach (var project in projects)
            {
                Console.WriteLine($"Генерация страницы для проекта: {project.Name}");
                _pageGenerator.GeneratePage("Templates/ProjectTemplate.sbn", new { project }, "Output/Projects", project.Name);
                _pageGenerator.GeneratePage("Templates/ObsProjectTemplate.sbn", new { project }, "Obsidian/Projects", $"Проект {project.Name}");
            }

            Console.WriteLine("Генерация завершена.");
        }
        
        private static List<Button> GetButtonsForProject(List<Button> allButtons, string projectName, List<Tracker> trackers, List<Group> groups)
        {
            var buttonsForRole = new List<Button>();

            foreach (var button in allButtons)
            {
                // Проверяем, есть ли опция с field_name == "project_id" и значением projectName
                bool hasProjectOption = button.Options.Any(option =>
                                            option.FieldName == "project_id" &&
                                            ((option.IsInverted == 0 && option.Values.Contains(projectName)) ||
                                             (option.IsInverted == 1 && !option.Values.Contains(projectName)))) 
                                        || button.Options.FindIndex(option => option.FieldName == "project_name") == -1
                                        ;
                
                
                bool hasTrackerOption = false;
                foreach (var tracker in trackers)
                {
                    if (button.Options.Any(option =>
                            option.FieldName == "tracker_id" &&
                            ((option.IsInverted == 0 && option.Values.Contains(tracker.Name)) ||
                             (option.IsInverted == 1 && !option.Values.Contains(tracker.Name))))
                        || button.Options.FindIndex(f => f.FieldName == "tracker_id") == -1
                       )
                    {
                        hasTrackerOption = true;
                        break;
                    }
                }

                bool hasRoleOption = false;
                foreach (var group in groups)
                {
                    foreach (var role in group.Projects[0].Roles)
                    {
                        if(role.Name.Equals("Специалист call-центра")) continue;
                        if (button.Options.Any(option =>
                                option.FieldName == "current_user" &&
                                ((option.IsInverted == 0 && option.Values.Contains(role.Name)) ||
                                 (option.IsInverted == 1 && !option.Values.Contains(role.Name))))
                            || button.Options.FindIndex(f => f.FieldName == "current_user") == -1
                           )
                        {
                            hasRoleOption = true;
                            break;
                        }
                    }
                    if (hasRoleOption) break;
                }

                if (hasProjectOption && hasTrackerOption && hasRoleOption)
                {
                    buttonsForRole.Add(button);
                }
            }

            return buttonsForRole;
        }

    }
}
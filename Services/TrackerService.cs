using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using RedmineDocs.Models;

namespace RedmineDocs.Services
{
    public class TrackerService
    {
        private readonly IDbConnection _dbConnection;
        private readonly PageGeneratorService _pageGenerator;
        private readonly List<Button> _allButtons;
        private readonly List<Role> _allRoles;

        public TrackerService(IDbConnection dbConnection, List<Button> allButtons, List<Role> allRoles)
        {
            _dbConnection = dbConnection;
            _pageGenerator = new PageGeneratorService();
            _allButtons = allButtons;
            _allRoles = allRoles;
        }
        
        public async Task<List<Tracker>> GetTrackersAsync(int projectId)
        {
            string trackersQuery = @"
                SELECT t.id, t.name
                FROM trackers t
                JOIN projects_trackers pt ON t.id = pt.tracker_id
                WHERE pt.project_id = @ProjectId";

            var trackers = (await _dbConnection.QueryAsync<Tracker>(trackersQuery, new { ProjectId = projectId })).AsList();

            // При необходимости загружаем кнопки для трекеров
            foreach (var tracker in trackers)
            {
                tracker.Name = CleanTrackerName(tracker.Name);
                tracker.Roles = GetRolesForTracker(_allRoles, tracker.Name);
                tracker.Buttons = GetButtonsForTrackerAsync(_allButtons, tracker.Name);
            }

            return trackers;
        }

        private List<Button> GetButtonsForTrackerAsync(List<Button> allButtons, string trackerName)
        {
            // Фильтрация кнопок по роли
            var buttonsForRole = new List<Button>();

            foreach (var button in allButtons)
            {
                // Проверяем, есть ли опция с field_name == "current_user" и значением roleId
                var hasTrackerOption = button.Options.Any(option =>
                    option.FieldName == "tracker_id" &&
                    ((option.IsInverted == 0 && option.Values.Contains(trackerName)) ||
                     (option.IsInverted == 1 && !option.Values.Contains(trackerName)))
                );

                if (hasTrackerOption)
                {
                    buttonsForRole.Add(button);
                }
            }

            return buttonsForRole;
        }

        private List<Role> GetRolesForTracker(List<Role> allRoles, string trackerName)
        {
            var rolesForTracker = new List<Role>();
            
            foreach (var role in allRoles)
            {
                var hasPermissionTrackerIds = role.PermissionsTrackerIds.Any(permission =>
                    permission.Value.Contains(trackerName));
                var hasPermissionAllTracker = role.PermissionsAllTrackers.Any(permission =>
                    permission.Value == "1" && permission.Key != "delete_issues");

                if (hasPermissionTrackerIds || hasPermissionAllTracker)
                {
                    rolesForTracker.Add(role);
                }
            }
            
            
            return rolesForTracker;
        }

        
        /// <summary>
        /// Очищает название кнопки от лишних символов и приводит его к нормальному виду.
        /// </summary>
        /// <param name="name">Исходное название кнопки.</param>
        /// <returns>Очищенное название кнопки.</returns>
        private string CleanTrackerName(string name)
        {
            // Убираем переносы строк и лишние пробелы
            name = name.Replace(".ru", " ru").Trim();

            // Если название начинается с "---", убираем его
            if (name.EndsWith("---"))
            {
                name = name.Substring(3).Trim();
            }

            // Проверяем на структуру вроде `!ruby/hash-with-ivars` и ищем значение после "default:"
            if (name.Contains("!ruby") || name.Contains("elements:"))
            {
                var defaultIndex = name.IndexOf("default:");
                if (defaultIndex != -1)
                {
                    // Извлекаем текст после "default:" до конца строки или до следующего разделителя
                    var defaultValue = name.Substring(defaultIndex + 8).Trim(); // 8 — длина строки "default:"
                    var endIndex = defaultValue.IndexOfAny(new[] { '\n', '\r' }); // Ищем конец строки
                    if (endIndex != -1)
                    {
                        defaultValue = defaultValue.Substring(0, endIndex).Trim();
                    }

                    return string.IsNullOrWhiteSpace(defaultValue) ? "Неизвестный трекер" : defaultValue;
                }

                return "Неизвестный трекер";
            }

            // Если имя пустое после обработки, возвращаем заглушку
            return string.IsNullOrWhiteSpace(name) ? "Неизвестный трекер" : name;
        }
        public void GenerateTrackerPage(Tracker tracker, Project project)
        {
            string templatePath = "Templates/TrackerTemplate.sbn";
            string obsTemplatePath = "Templates/ObsTrackerTemplate.sbn";
            string outputDir = Path.Combine("Output", "Trackers");
            string obsOutputDir = Path.Combine("Obsidian", "Trackers");
            string fileName = $"Трекер {tracker.Name}";

            _pageGenerator.GeneratePage(templatePath, new { tracker, project }, outputDir, fileName);
            _pageGenerator.GeneratePage(obsTemplatePath, new { tracker, project }, obsOutputDir, fileName);
        }
    }
}
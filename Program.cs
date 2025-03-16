using Microsoft.Extensions.Configuration;
using RedmineDocs.Services;
using RedmineDocs.Models;
using System.Threading.Tasks;
using System.IO;
using MySqlConnector;
using System.Data;
using System.Collections.Generic;
using Serilog;

namespace RedmineDocs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: configuration["logPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "logs/log-.log"),
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    fileSizeLimitBytes: 100_000_000,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 12,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            try
            {
                Log.Information("Начало генерации документации.");

                // Настройка конфигурации
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                var configuration = builder.Build();

                string connectionString = configuration.GetConnectionString("RedmineDatabase");

                using IDbConnection dbConnection = new MySqlConnection(connectionString);
                dbConnection.Open();
                
                var buttonService = new ButtonService(dbConnection);
                var allButtons = await buttonService.GetAllButtonsAsync();
                var roleService = new RoleService(dbConnection, allButtons);
                var allRoles = await roleService.GetAllRolesAsync();
                var groupService = new GroupService(dbConnection);
                var trackerService = new TrackerService(dbConnection, allButtons, allRoles);
                var projectService = new ProjectService(connectionString, allButtons, allRoles);

                // Получение всех групп и проектов
                var groups = await groupService.GetGroupsAsync();
                var projects = await projectService.GetProjectsAsync();

                // Генерация страниц для проектов
                projectService.PageGenerate(projects);

                // Генерация страниц для групп
                foreach (var group in groups)
                {
                    Log.Information("Генерация страницы для группы: {GroupName}", group.Name);
                    groupService.GenerateGroupPage(group);
                }

                // Генерация страниц для ролей
                foreach (var role in allRoles){
                    Log.Information("Генерация страницы для роли: {RoleName}", role.Name);
                    roleService.GenerateRolePage(role);
                }

                // Генерация страниц для трекеров
                foreach (var project in projects)
                {
                    foreach (var tracker in project.Trackers)
                    {
                        Log.Information("Генерация страницы для трекера: {TrackerName}", tracker.Name);
                        trackerService.GenerateTrackerPage(tracker, project); // Передаём проект
                    }
                }

                // Генерация страниц для кнопок
                foreach (var button in allButtons)
                {
                        Log.Information("Генерация страницы для кнопки: {ButtonName}", button.Name);
                        buttonService.GenerateButtonPage(button);
                }

                dbConnection.Close();

                Log.Information("Генерация документации завершена успешно.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Произошла критическая ошибка во время генерации документации.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
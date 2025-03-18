using System.Data;
using DotEnv.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using RedmineDocs.Services.Implementation;
using Serilog.Events;

namespace RedmineDocs;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var envVars = new EnvLoader().Load();
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

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
            
            string outputPath = configuration["OutputPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "docs");
            
            services.AddScoped<IDbConnection>(_ =>
            {
                var connectionString = "";
                if (!string.IsNullOrEmpty(envVars[AppSettings.ConnectionString]))
                    connectionString = envVars[AppSettings.ConnectionString];
                else
                    connectionString = configuration.GetConnectionString("DefaultConnection");

                return new MySqlConnection(connectionString);
            });
            
            services.AddScoped<IDataService, DataService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IButtonService, ButtonService>();
            services.AddScoped<ITrackerService, TrackerService>();
            services.AddScoped<IRoleService, RoleService>();
            
            var serviceProvider = services.BuildServiceProvider();
            var dataService = serviceProvider.GetRequiredService<IDataService>();
            var projectService = serviceProvider.GetRequiredService<IProjectService>();
            var buttonService = serviceProvider.GetRequiredService<IButtonService>();
            var groupService = serviceProvider.GetRequiredService<IGroupService>();
            var trackerService = serviceProvider.GetRequiredService<ITrackerService>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();

            if (args.Contains("--debug-groups"))
            {
                var projectData = await dataService.GetProjectsAsync();
                Log.Information("=== Отладка данных групп ===");
                foreach (var project in projectData)
                {
                    Log.Information("Проект {ProjectId} : {ProjectName}", project.Id, project.Name);
                    Log.Information("Данные групп: {GroupsJson}", project.GroupsJson);
                }

                return;
            }
            
            var projects = await projectService.GetProjectsAsync();
            var buttons = await buttonService.GetButtonsAsync();
            var roles = await roleService.GetRolesAsync();
            var trackers = trackerService.GetTrackersAsync(projects);
            var groups = groupService.GetGroups(projects);
            
            Log.Information("Все сущности успешно загружены");

            var markdownGenerator =
                new MarkdownGeneratorService(outputPath, projects, roles, trackers, groups, buttons);
            
            Log.Information("Генерация документов для проектов");
            foreach (var project in projects)
            {
                Log.Debug("Генерация документации для проекта Id={Id}, Name={Name}", project.Id, project.Name);
                await markdownGenerator.GenerateProjectPage(project);
            }
            
            Log.Information("Генерация документов для групп");
            foreach (var group in groups)
            {
                Log.Debug("Генерация документации для группы Id={Id}, Name={Name}", group.Id, group.Name);
                await markdownGenerator.GenerateGroupPage(group);
            }
            
            Log.Information("Генерация документации для трекеров");
            foreach (var tracker in trackers)
            {
                Log.Debug("Генерация документации для трекера Id={Id}, Name={Name}", tracker.Id, tracker.Name);
                await markdownGenerator.GenerateTrackerPage(tracker);
            }
            
            Log.Information("Генерация документов для ролей");
            foreach (var role in roles)
            {
                Log.Debug("Генерация документации для роли Id={Id}, Name={Name}", role.Id, role.Name);
                await markdownGenerator.GenerateRolePage(role);
            }
            
            Log.Information("Генерация документов для кнопок");
            foreach (var button in buttons)
            {
                Log.Debug("Генерация документации для кнопки Id={Id}, Name={Name}", button.Id, button.Name);
                await markdownGenerator.GenerateButtonPage(button);
            }

        }
        catch (Exception ex)
        {
            Log.Error(ex, "Критическая ошибка при выполнении программы");
            throw;
        }
    }
}
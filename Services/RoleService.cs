using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using RedmineDocs.Models;

namespace RedmineDocs.Services
{
	public class RoleService
	{
		private readonly IDbConnection _dbConnection;
		private readonly PageGeneratorService _pageGenerator;
		private readonly List<Button> _allButtons;

		public RoleService(IDbConnection dbConnection, List<Button> allButtons)
		{
			_dbConnection = dbConnection;
			_pageGenerator = new PageGeneratorService();
			_allButtons = allButtons;
		}

        public async Task<List<Role>> GetAllRolesAsync()
        {
            string rolesQuery = @"
		        SELECT DISTINCT r.id, r.name, r.settings
		        FROM roles r";

            var roles = (await _dbConnection.QueryAsync<Role>(rolesQuery)).AsList();

            // Получаем все трекеры из базы данных
            string trackersQuery = @"
		        SELECT id, name
		        FROM trackers";

            var trackerIdNamePairs = await _dbConnection.QueryAsync<(int Id, string Name)>(trackersQuery);

            // Создаем словарь ID трекера -> Название трекера
            var trackerIdToName = trackerIdNamePairs.ToDictionary(pair => pair.Id.ToString(), pair => pair.Name);

            foreach (var role in roles)
            {
                // Передаем словарь трекеров в метод ParseSettings
                role.ParseSettings(trackerIdToName);
                role.Buttons = GetButtonsForRole(_allButtons, role.Name);
            }
            return roles;
        }

		public void GenerateRolePage(Role role)
		{
			string templatePath = "Templates/RoleTemplate.sbn";
			string obsTemplatePath = "Templates/ObsRoleTemplate.sbn";
			string outputDir = Path.Combine("Output", "Roles");
			string obsOutputDir = Path.Combine("Obsidian", "Roles");
			string fileName = $"Роль {role.Name}";

			_pageGenerator.GeneratePage(templatePath, new { role }, outputDir, fileName);
			_pageGenerator.GeneratePage(obsTemplatePath, new { role }, obsOutputDir, fileName);
		}
		private List<Button> GetButtonsForRole(List<Button> allButtons, string roleName)
		{
			// Фильтрация кнопок по роли
			var buttonsForRole = new List<Button>();

			foreach (var button in allButtons)
			{
				// Проверяем, есть ли опция с field_name == "current_user" и значением roleId
				var hasRoleOption = button.Options.Any(option =>
					option.FieldName == "current_user" &&
					((option.IsInverted == 0 && option.Values.Contains(roleName)) ||
					 (option.IsInverted == 1 && !option.Values.Contains(roleName)))
				);

				if (hasRoleOption)
				{
					buttonsForRole.Add(button);
				}
			}

			return buttonsForRole;
		}
	}
}
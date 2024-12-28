using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using RedmineDocs.Models;
using RedmineDocs.Helpers;

namespace RedmineDocs.Services
{
    public class ButtonService
    {
        private readonly IDbConnection _dbConnection;
        private readonly PageGeneratorService _pageGenerator;

        public ButtonService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _pageGenerator = new PageGeneratorService();
        }

        public async Task<List<Button>> GetAllButtonsAsync()
        {
            string buttonsQuery = @"
                        SELECT DISTINCT lb.id, lb.name, lb.description
                        FROM lu_buttons lb";

            var buttons = (await _dbConnection.QueryAsync<Button>(buttonsQuery)).ToList();

            foreach (var button in buttons)
            {
                // Обработка названия кнопки
                button.Name = CleanButtonName(button.Name);
                Console.WriteLine($"Обработка кнопки {button.Name}");
                button.Options = await GetOptionsWithNamesAsync(button.Id);
                button.Actions = await GetActionsWithNamesAsync(button.Id);
            }

            return buttons;
        }


        private async Task<List<Option>> GetOptionsWithNamesAsync(int buttonId)
        {
            string optionsQuery = @"
                SELECT lo.field_name AS FieldName, lo.invert AS IsInverted, 
                       lov.value AS Value
                FROM lu_button_options lo
                JOIN lu_button_option_values lov ON lo.id = lov.lu_button_option_id
                WHERE lo.lu_button_id = @ButtonId";

            var optionsRaw = await _dbConnection.QueryAsync<(string FieldName, int IsInverted, string Value)>(optionsQuery, new { ButtonId = buttonId });

            var options = optionsRaw
                .GroupBy(o => new { o.FieldName, o.IsInverted })
                .Select(g => new Option
                {
                    FieldName = g.Key.FieldName,
                    IsInverted = g.Key.IsInverted,
                    Values = g.Select(o => o.Value).ToList()
                })
                .ToList();

            foreach (var option in options)
            {
                try
                {
                    var tableName = FieldNames.GetTableName(option.FieldName);

                    if (tableName == "custom_fields")
                    {
                        if (int.TryParse(option.FieldName.Split('-').Last(), out int customFieldId))
                        {
                            string customFieldQuery = @"
                                SELECT name 
                                FROM custom_fields 
                                WHERE id = @CustomFieldId";

                            var customFieldName = await _dbConnection.QueryFirstOrDefaultAsync<string>(customFieldQuery, new { CustomFieldId = customFieldId });

                            option.FieldName = customFieldName ?? option.FieldName;
                        }
                    }
                    else if (tableName != null)
                    {
                        string relatedTableQuery = $@"
                            SELECT name 
                            FROM {tableName}
                            WHERE id IN @Ids";

                        var relatedNames = await _dbConnection.QueryAsync<string>(relatedTableQuery, new { Ids = option.Values.Select(int.Parse).ToArray() });

                        option.Values = relatedNames.ToList();
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"Ошибка при обработке опции: {ex.Message}. Используется исходное значение Value.");
                    // Оставляем текущее значение Value без изменений
                }
            }

            return options;
        }

        private async Task<List<Models.Action>> GetActionsWithNamesAsync(int buttonId)
        {
            string actionsQuery = @"
                SELECT lba.field_name AS FieldName, lba.action AS ActionType, lba.overwrite AS IsInverted,
                       lbav.value AS Value
                FROM lu_button_actions lba
                LEFT JOIN lu_button_action_values lbav ON lba.id = lbav.lu_button_action_id
                WHERE lba.lu_button_id = @ButtonId";

            var actionsRaw = await _dbConnection.QueryAsync<(string FieldName, string ActionType, int IsInverted, string Value)>(actionsQuery, new { ButtonId = buttonId });

            var actions = actionsRaw
                .GroupBy(a => new { a.FieldName, a.ActionType, a.IsInverted })
                .Select(g => new Models.Action
                {
                    FieldName = g.Key.FieldName,
                    ActionType = g.Key.ActionType,
                    IsInverted = g.Key.IsInverted,
                    Values = g.Select(a => a.Value).ToList()
                })
                .ToList();

            foreach (var action in actions)
            {
                try
                {
                    var tableName = FieldNames.GetTableName(action.FieldName);

                    if (tableName == "custom_fields")
                    {
                        if (int.TryParse(action.FieldName.Split('-').Last(), out int customFieldId))
                        {
                            string customFieldQuery = @"
                                SELECT name 
                                FROM custom_fields 
                                WHERE id = @CustomFieldId";

                            var customFieldName = await _dbConnection.QueryFirstOrDefaultAsync<string>(customFieldQuery, new { CustomFieldId = customFieldId });

                            action.FieldName = customFieldName ?? action.FieldName;
                        }
                    }
                    else if (tableName != null)
                    {
                        string relatedTableQuery = $@"
                            SELECT name 
                            FROM {tableName}
                            WHERE id IN @Ids";

                        var relatedNames = await _dbConnection.QueryAsync<string>(relatedTableQuery, new { Ids = action.Values.Select(int.Parse).ToArray() });

                        action.Values = relatedNames.ToList();
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"Ошибка при обработке действия: {ex.Message}. Используется исходное значение Value.");
                    // Оставляем текущее значение Value без изменений
                }
            }

            return actions;
        }

        /// <summary>
        /// Очищает название кнопки от лишних символов и приводит его к нормальному виду.
        /// </summary>
        /// <param name="name">Исходное название кнопки.</param>
        /// <returns>Очищенное название кнопки.</returns>
        private string CleanButtonName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Неизвестная кнопка";
            }

            // Убираем переносы строк и лишние пробелы
            name = name.Replace("\n", "").Replace("\r", "").Replace("\"", "").Replace("...", "").Trim();

            // Если название начинается с "---", убираем его
            if (name.StartsWith("---"))
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

                    return string.IsNullOrWhiteSpace(defaultValue) ? "Неизвестная кнопка" : defaultValue;
                }

                return "Неизвестная кнопка";
            }

            // Если имя пустое после обработки, возвращаем заглушку
            return string.IsNullOrWhiteSpace(name) ? "Неизвестная кнопка" : name;
        }

        public void GenerateButtonPage(Button button)
        {
            string templatePath = "Templates/ButtonTemplate.sbn";
            string obsTemplatePath = "Templates/ObsButtonTemplate.sbn";
            string outputDir = Path.Combine("Output", "Buttons");
            string obsOutputDir = Path.Combine("Obsidian", "Buttons");
            string fileName = $"Кнопка {button.Name}";

            _pageGenerator.GeneratePage(templatePath, new { button }, outputDir, fileName);
            _pageGenerator.GeneratePage(obsTemplatePath, new { button }, obsOutputDir, fileName);
        }
    }
}
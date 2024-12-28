using System;
using System.IO;
using System.Text;
using Markdig;
using Scriban;

namespace RedmineDocs.Services
{
    /// <summary>
    /// Сервис для генерации страниц документации на основе шаблонов Scriban.
    /// </summary>
    public class PageGeneratorService
    {
        /// <summary>
        /// Генерирует страницы документации (Markdown и HTML) на основе шаблона и модели данных.
        /// </summary>
        /// <param name="templatePath">Путь к файлу шаблона Scriban (.sbn).</param>
        /// <param name="model">Модель данных для рендеринга шаблона.</param>
        /// <param name="outputDir">Путь к директории для сохранения сгенерированных файлов.</param>
        /// <param name="fileName">Имя файла (без расширения).</param>
        public void GeneratePage(string templatePath, object model, string outputDir, string fileName)
        {
            try
            {
                // Проверка существования шаблона
                if (!File.Exists(templatePath))
                {
                    Console.WriteLine($"Шаблон не найден: {templatePath}");
                    return;
                }

                // Чтение шаблона
                string templateText = File.ReadAllText(templatePath);
                var template = Template.Parse(templateText);

                // Проверка на ошибки парсинга шаблона
                if (template.HasErrors)
                {
                    Console.WriteLine($"Ошибки в шаблоне {templatePath}:");
                    foreach (var message in template.Messages)
                    {
                        Console.WriteLine($"- {message}");
                    }
                    return;
                }

                // Рендеринг шаблона с моделью данных
                var renderedContent = template.Render(model, memberRenamer: member => member.Name);

                // Создание директории, если она не существует
                Directory.CreateDirectory(outputDir);

                // Полный путь для файла Markdown
                string markdownPath = GetUniqueFilePath(Path.Combine(outputDir, $"{SanitizeFileName(fileName)}.md"));

                // Запись Markdown файла
                File.WriteAllText(markdownPath, renderedContent, Encoding.UTF8);
                Console.WriteLine($"Markdown файл создан: {markdownPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при генерации страницы: {ex.Message}");
            }
        }

        /// <summary>
        /// Очищает имя файла, заменяя недопустимые символы на подчёркивания.
        /// </summary>
        /// <param name="fileName">Имя файла для очистки.</param>
        /// <returns>Очищенное имя файла.</returns>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Имя файла не может быть пустым или содержать только пробелы.", nameof(fileName));

            // Ограничение длины имени файла (например, до 100 символов)
            fileName = fileName.Length > 100 ? fileName.Substring(0, 100) : fileName;

            // Заменяем недопустимые символы на подчёркивания
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, ' ');
            }

            return fileName;
        }

        /// <summary>
        /// Получает уникальный путь к файлу, добавляя числовой индекс, если файл уже существует.
        /// </summary>
        /// <param name="filePath">Исходный путь к файлу.</param>
        /// <returns>Уникальный путь к файлу.</returns>
        private static string GetUniqueFilePath(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            // Если имя файла начинается с "Трекер ", сразу возвращаем оригинальный путь
            if (fileNameWithoutExtension.StartsWith("Трекер "))
            {
                return filePath;
            }

            int counter = 1;
            string uniqueFilePath = filePath;

            while (File.Exists(uniqueFilePath))
            {
                uniqueFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}({counter}){extension}");
                counter++;
            }

            return uniqueFilePath;
        }
    }
}

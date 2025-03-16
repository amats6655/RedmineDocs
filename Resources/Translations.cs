namespace RedmineDocs.Resources;

public static class Translations
{
    private static Dictionary<string, string> _translations = new();
    private static Dictionary<string, string> _defaultTranslations = new();
    private static Dictionary<string, string> _fileTranslations = new();

    static Translations()
    {
        InitializeDefaultTranslations();
        LoadTranslationsFromFile();
    }

    /// <summary>
    /// Получает перевод термина. Если перевод не найден, возвращает сам термин
    /// </summary>
    public static string Get(string term)
    {
        if (string.IsNullOrEmpty(term)) return term;

        if (_translations.TryGetValue(term, out var translation))
        {
            return translation;
        }
        return term;
    }

    private static void InitializeDefaultTranslations()
    {
        _defaultTranslations = new Dictionary<string, string>()
        {
            // Тут встроенные переводы
        };
        foreach (var kvp in _defaultTranslations)
        {
            _translations[kvp.Key] = kvp.Value;
        }
    }

    private static void LoadTranslationsFromFile()
    {
        Log.Debug("Поиск файла переводов в возможных местах:");
        var possiblePaths = GetPossibleTranslationsPaths();

        foreach (var path in possiblePaths)
        {
            Log.Debug("Проверка пути: {Path} -> Существует: {Exists}", path, File.Exists(path));
            if (File.Exists(path))
            {
                Log.Information("Файл переводов найден: {Path}", path);
                try
                {
                    var json = File.ReadAllText(path);
                    Log.Debug("Содержимое файла переводов (первые 100 символов): {Content}...",
                        json.Substring(0, Math.Min(100, json.Length)));

                    var additionalTranslations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (additionalTranslations != null)
                    {
                        Log.Information("Успешно десериализовано {Count} переводов", additionalTranslations.Count);
                        _fileTranslations = additionalTranslations;

                        foreach (var kvp in additionalTranslations)
                        {
                            Log.Debug("Добавление/обновление перевода: '{Key}', -> '{Value}'",
                                kvp.Key, kvp.Value);
                            _translations[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        Log.Error("Ошибка: десериализованные переводы равны null");
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при загрузке файла переводов");
                }
            }

            if (!_fileTranslations.Any())
            {
                Log.Warning("Файл переводов не найден ни в одном из проверенных мест");
            }
        }
    }
    
    private static IEnumerable<string> GetPossibleTranslationsPaths()
    {
        var currentDir = Directory.GetCurrentDirectory();
        return new[]
        {
            Path.Combine(currentDir, "translations.json"),
            Path.Combine(currentDir, "docs", "translations.json"),
            Path.Combine(currentDir, "RedmineDocs", "translations.json")
        };
    }
    
    /// <summary>
    /// Получает перевод термина с информацией об источнике. Используется для отладки
    /// </summary>
    public static string GetWithSource(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return term;

        if (_translations.TryGetValue(term, out var translation))
        {
            var source = _fileTranslations.ContainsKey(term) ? "Файл" : "Встроенный";
            return $"{translation} ({source})";
        }

        return term;
    }
}
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RedmineDocs.Models;
public class Role : BaseModel
{
    public string Settings { get; set; } // Сырые данные из поля settings

    // Парсенные права доступа
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, string> PermissionsAllTrackers { get; set; } = new();
    public Dictionary<string, List<string>> PermissionsTrackerIds { get; set; } = new();

    // Связи
    public List<Tracker> Trackers { get; set; } = new();
    public List<Button> Buttons { get; set; } = new();

    // Метод для парсинга settings
public void ParseSettings(Dictionary<string, string> trackerIdToName)
 {
    if (string.IsNullOrEmpty(Settings))
        return;

    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        // Маппируем пользовательский тег на Dictionary<string, object>
        .WithTagMapping("!ruby/hash:ActiveSupport::HashWithIndifferentAccess", typeof(Dictionary<string, object>))
        .Build();

    Dictionary<string, object> yamlObject;
    try
    {
        yamlObject = deserializer.Deserialize<Dictionary<string, object>>(Settings);
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
        Console.WriteLine($"Ошибка при десериализации settings для роли '{Name}': {ex.Message}");
        return;
    }

    // Парсинг списка permissions
    if (yamlObject.ContainsKey("permissions"))
    {
        var permissionsList = yamlObject["permissions"] as IEnumerable<object>;
        if (permissionsList != null)
        {
            Permissions = permissionsList.Select(p => p.ToString()).ToList();
        }
    }

    // Парсинг permissions_all_trackers
    if (yamlObject.ContainsKey("permissions_all_trackers"))
    {
        var permissionsAllTrackers = yamlObject["permissions_all_trackers"] as Dictionary<string, object>;
        if (permissionsAllTrackers != null)
        {
            PermissionsAllTrackers = permissionsAllTrackers.ToDictionary(
                kv => kv.Key.ToString(),
                kv => kv.Value.ToString()
            );
        }
    }

    // Парсинг permissions_tracker_ids
    if (yamlObject.ContainsKey("permissions_tracker_ids"))
    {
        var permissionsTrackerIds = yamlObject["permissions_tracker_ids"] as Dictionary<string, object>;
        if (permissionsTrackerIds != null)
        {
            PermissionsTrackerIds = new Dictionary<string, List<string>>();

            foreach (var kvp in permissionsTrackerIds)
            {
                var permission = kvp.Key.ToString();
                var trackerIdsList = kvp.Value as IEnumerable<object>;
                if (trackerIdsList != null)
                {
                    var trackerNames = new List<string>();
                    foreach (var trackerIdObj in trackerIdsList)
                    {
                        var trackerIdStr = trackerIdObj.ToString();
                        if (trackerIdToName.TryGetValue(trackerIdStr, out var trackerName))
                        {
                            trackerNames.Add(trackerName);
                        }
                        else
                        {
                            trackerNames.Add($"ID {trackerIdStr} (название не найдено)");
                        }
                    }
                    PermissionsTrackerIds[permission] = trackerNames;
                }
            }
        }
    }
}
}
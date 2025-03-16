using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RedmineDocs.Models;
public class Role
{
    [JsonProperty ("id")] public int Id { get; set; }
    [JsonProperty ("name")] public required string Name { get; set; }
    public string? Permissions { get; set; }
    public string? Settings { get; set; }
    
    [JsonIgnore] public List<string>? ParsedPermissions { get; set; }
    [JsonIgnore] public RoleSettings? ParsedSettings { get; set; }

    public void ParseYamlFields()
    {
        var fixedSettings = Settings;
        if(!string.IsNullOrEmpty(Settings) && Settings.Contains("!ruby/hash:ActiveSupport::HashWithIndifferentAccess"))
            fixedSettings = Settings.Replace("!ruby/hash:ActiveSupport::HashWithIndifferentAccess", "");

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new UnderscoredNamingConvention())
            .Build();

        if (!string.IsNullOrEmpty(Permissions))
        {
            try
            {
                ParsedPermissions = deserializer.Deserialize<List<string>>(Permissions);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при десериализации YAML-поля роли {RoleId}", Id);
                Log.Debug("Permissions: {Permissions}", Permissions);
            }
        }

        if (!string.IsNullOrEmpty(fixedSettings))
        {
            try
            {
                ParsedSettings = deserializer.Deserialize<RoleSettings>(fixedSettings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при десериализации YAML-поля роли {RoleId}", Id);
                Log.Debug("Settings: {Settings}", fixedSettings);
            }
        }
    }
}
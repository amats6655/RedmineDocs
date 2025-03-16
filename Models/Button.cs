using Newtonsoft.Json;
using Serilog;

namespace RedmineDocs.Models;

public class Button
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Description { get; set; }
    [JsonProperty("actions_json")] public string? ActionsJson { get; set; }
    [JsonProperty("options_json")] public string? OptionsJson { get; set; }
    [JsonIgnore] public List<ButtonAction>? Actions { get; set; }
    [JsonIgnore] public List<ButtonOption>? Options { get; set; }
    
    [JsonIgnore] public List<int> AssociatedProjectIds { get; set; } = new List<int>();
    [JsonIgnore] public List<int> AssociatedRoleIds { get; set; } = new List<int>();
    [JsonIgnore] public List<int> AssociatedTrackerIds { get; set; } = new List<int>();
    
    [JsonIgnore] public bool IsProjectInverted { get; private set; }
    [JsonIgnore] public bool IsRoleInverted { get; private set; }
    [JsonIgnore] public bool IsTrackerInverted { get; private set; }
    
    [JsonIgnore] public bool IsUniversalForProjects => !AssociatedProjectIds.Any();
    [JsonIgnore] public bool IsUniversalForRoles => !AssociatedRoleIds.Any();
    [JsonIgnore] public bool IsUniversalForTrackers => !AssociatedTrackerIds.Any();

    public void DeserializeJsonFields()
    {
        try
        {
            if (!string.IsNullOrEmpty(ActionsJson))
            {
                Actions = JsonConvert.DeserializeObject<List<ButtonAction>>(ActionsJson) ?? new List<ButtonAction>();
            }

            if (!string.IsNullOrEmpty(OptionsJson))
            {
                Options = JsonConvert.DeserializeObject<List<ButtonOption>>(OptionsJson) ?? new List<ButtonOption>();
            }

            ComputeAssociations();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при десериализации JSON для кнопки {ButtonName}", Name);
            Log.Debug("ActionsJson: {ActionsJson}", ActionsJson);
            Log.Debug("OptionsJson: {OptionsJson}", OptionsJson);
        }
    }

    private void ComputeAssociations()
    {
        AssociatedProjectIds.Clear();
        AssociatedRoleIds.Clear();
        AssociatedTrackerIds.Clear();

        if (Options != null)
        {
            foreach (var option in Options)
            {
                var assocType = GetAssociationType(option.FieldName!);
                if (assocType != AssociationType.None)
                {
                    if (assocType == AssociationType.Project)
                        IsProjectInverted = option.Invert == 1;
                    else if (assocType == AssociationType.Role)
                        IsRoleInverted = option.Invert == 1;
                    else if (assocType == AssociationType.Tracker)
                        IsTrackerInverted = option.Invert == 1;
                }

                foreach (var value in option.Values)
                {
                    if (!string.IsNullOrEmpty(value.Id) && int.TryParse(value.Id, out int parsedId))
                    {
                        switch (assocType)
                        {
                            case AssociationType.Project:
                                if (!AssociatedProjectIds.Contains(parsedId))
                                    AssociatedProjectIds.Add(parsedId);
                                break;
                            case AssociationType.Role:
                                if (!AssociatedRoleIds.Contains(parsedId))
                                    AssociatedRoleIds.Add(parsedId);
                                break;
                            case AssociationType.Tracker:
                                if (!AssociatedTrackerIds.Contains(parsedId))
                                    AssociatedTrackerIds.Add(parsedId);
                                break;
                        }
                    }
                }
            }
        }
    }

    private AssociationType GetAssociationType(string fieldName)
    {
        return fieldName switch
        {
            "project_id" => AssociationType.Project,
            "current_user" => AssociationType.Role,
            "tracker_id" => AssociationType.Tracker,
            _ => AssociationType.None
        };
    }
}
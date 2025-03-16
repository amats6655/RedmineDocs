namespace RedmineDocs.Services.Implementation;

public class ButtonService : IButtonService
{
    private readonly IDataService _dataService;
    public ButtonService(IDataService dataService)
    {
        _dataService = dataService;
    }
    
    public async Task<List<Button>> GetButtonsAsync()
    {
        var buttons = await _dataService.GetButtonsAsync();
        Log.Information("Начинаем обработку {Count} кнопок", buttons?.Count() ?? 0);

        if (buttons == null || !buttons.Any())
        {
            Log.Warning("Список кнопок пуст");
            return new List<Button>();
        }

        foreach (var button in buttons)
        {
            try
            {
                button.DeserializeJsonFields();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработки кнопки Id={Id}, Name={Name}",
                    button.Id, button.Name);
            }
        }
        
        Log.Information("Обработка кнопок завершена. Обработано {Count} кнопок", buttons.Count());
        return buttons;
    }
}
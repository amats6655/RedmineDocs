using RedmineDocs.Models;

namespace RedmineDocs.Services.Interfaces;

public interface IButtonService
{
    Task<List<Button>> GetButtonsAsync();
}
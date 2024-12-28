using ToDoSampleApp.Models;

namespace ToDoSampleApp.Services;

public interface IDatabaseService
{
    Task<List<TodoItem>> GetItemsAsync();
    Task<int> SaveItemAsync(TodoItem item);
    Task<int> DeleteItemAsync(TodoItem item);
}

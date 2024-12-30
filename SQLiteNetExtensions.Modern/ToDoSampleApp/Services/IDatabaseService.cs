using System.Diagnostics.Metrics;
using ToDoSampleApp.Models;

namespace ToDoSampleApp.Services;

public interface IDatabaseService
{
    Task<List<TodoItem>> GetItemsAsync();
    Task SaveItemAsync(TodoItem item);
    Task UpdateItemAsync(TodoItem item);
    Task<int> DeleteItemAsync(TodoItem item);
    Task Init();
}

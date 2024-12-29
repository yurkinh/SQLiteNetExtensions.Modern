using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToDoSampleApp.Models;
using ToDoSampleApp.Services;

namespace ToDoSampleApp.ViewModels;

public partial class TodoItemViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDatabaseService databaseService;

    private TodoItem? receivedTodoItem;

    [ObservableProperty]
    private string? name;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private bool done;

    public TodoItemViewModel(IDatabaseService databaseService, TodoItem? todoItem = null)
    {
        this.databaseService = databaseService;

        if (todoItem != null)
        {
            Name = todoItem.Name;
            Notes = todoItem.Notes;
            Done = todoItem.Done;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("TodoItem", out var todoItemObj) && todoItemObj is TodoItem todoItem)
        {
            receivedTodoItem = todoItem;
            Name = todoItem.Name;
            Notes = todoItem.Notes;
            Done = todoItem.Done;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var todoItem = new TodoItem
        {
            Name = Name,
            Notes = Notes,
            Done = Done
        };

        //Edit mode
        if (receivedTodoItem != null)
        {
            todoItem.ID = receivedTodoItem.ID;
            await DeleteAsync(); //deleting old item
        }
 
        await databaseService.SaveItemAsync(todoItem);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await databaseService.DeleteItemAsync(receivedTodoItem);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
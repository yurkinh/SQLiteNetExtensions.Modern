using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToDoSampleApp.Models;
using ToDoSampleApp.Services;

namespace ToDoSampleApp.ViewModels;

public partial class TodoItemViewModel : ObservableObject
{
    private readonly IDatabaseService databaseService;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string notes;

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

    [RelayCommand]
    private async Task SaveAsync()
    {
        var todoItem = new TodoItem
        {
            Name = Name,
            Notes = Notes,
            Done = Done
        };

        await databaseService.SaveItemAsync(todoItem);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var todoItem = new TodoItem
        {
            Name = Name,
            Notes = Notes,
            Done = Done
        };

        await databaseService.DeleteItemAsync(todoItem);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
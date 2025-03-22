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
    private string? notesTitle;

    [ObservableProperty]
    private string? notesBody;

    [ObservableProperty]
    private bool done;

    [ObservableProperty]
    private DateTime eventDateTime = DateTime.Now;

    partial void OnEventDateTimeChanged(DateTime value)
    {
        OnPropertyChanged(nameof(EventTime));
    }

    public TimeSpan EventTime
    {
        get => EventDateTime.TimeOfDay;
        set
        {
            if (EventDateTime.TimeOfDay != value)
            {
                EventDateTime = EventDateTime.Date + value;
                OnPropertyChanged(nameof(EventTime));
            }
        }
    }

    public TodoItemViewModel(IDatabaseService databaseService, TodoItem? todoItem = null)
    {
        this.databaseService = databaseService;

        if (todoItem != null)
        {
            Name = todoItem.Name;
            Done = todoItem.Done;
            NotesTitle = todoItem.Notes?.Title;
            NotesBody = todoItem.Notes?.Body;

            EventDateTime = todoItem.Notes?.EventDateTime ?? DateTime.Now;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("TodoItem", out var todoItemObj) && todoItemObj is TodoItem todoItem)
        {
            receivedTodoItem = todoItem;
            Name = todoItem.Name;
            Done = todoItem.Done;
            NotesTitle = todoItem.Notes?.Title;
            NotesBody = todoItem.Notes?.Body;
            EventDateTime = todoItem.Notes?.EventDateTime ?? DateTime.Now;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (receivedTodoItem != null)
        {
            receivedTodoItem.Name = Name;
            receivedTodoItem.Done = Done;

            receivedTodoItem.Notes ??= new Notes();
            receivedTodoItem.Notes.Title = NotesTitle;
            receivedTodoItem.Notes.Body = NotesBody;
            receivedTodoItem.Notes.EventDateTime = EventDateTime;

            await databaseService.UpdateItemAsync(receivedTodoItem);
        }
        else
        {
            var todoItem = new TodoItem
            {
                Name = Name,
                Done = Done,
                Notes = new Notes
                {
                    Title = NotesTitle,
                    Body = NotesBody,
                    EventDateTime = EventDateTime
                }
            };
            await databaseService.SaveItemAsync(todoItem);
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (receivedTodoItem == null)
            return;
        await databaseService.DeleteItemAsync(receivedTodoItem);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
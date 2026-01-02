using ToDoSampleApp.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ToDoSampleApp.Services;
using ToDoSampleApp.Views;

namespace ToDoSampleApp.ViewModels;

public partial class TodoListViewModel(IDatabaseService databaseService) : ObservableObject
{
    private readonly IDatabaseService databaseService = databaseService;

    public ObservableCollection<TodoItem> Items { get; private set; } = [];

    [ObservableProperty]
    private TodoItem? selectedItem;

    partial void OnSelectedItemChanged(TodoItem? value)
    {
        if (value != null)
        {
            SelectItemAsync(value).ConfigureAwait(false);
            SelectedItem = null; // Reset selection
        }
    }

    public async Task LoadItemsAsync()
    {
        var items = await databaseService.GetItemsAsync();
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddItemAsync() => await Shell.Current.GoToAsync(nameof(TodoItemPage));

    [RelayCommand]
    private async Task SelectItemAsync(TodoItem selectedItem)
    {
        if (selectedItem == null)
            return;

        await Shell.Current.GoToAsync(nameof(TodoItemPage), new Dictionary<string, object>
        {
            { "TodoItem", selectedItem }
        });
    }

    [RelayCommand]
    private async Task AppearingAsync() => await LoadItemsAsync();
}
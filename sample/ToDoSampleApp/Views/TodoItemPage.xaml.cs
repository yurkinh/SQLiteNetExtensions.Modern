using ToDoSampleApp.ViewModels;

namespace ToDoSampleApp.Views;

public partial class TodoItemPage : ContentPage
{
    public TodoItemPage(TodoItemViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
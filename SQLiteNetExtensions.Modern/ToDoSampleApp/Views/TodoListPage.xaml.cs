using ToDoSampleApp.ViewModels;

namespace ToDoSampleApp.Views;

public partial class TodoListPage : ContentPage
{
	public TodoListPage(TodoListViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
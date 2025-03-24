using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ToDoSampleApp.Services;
using ToDoSampleApp.ViewModels;
using ToDoSampleApp.Views;

namespace ToDoSampleApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiCommunityToolkit()
			.RegisterViewModelsAndViews()
			.RegisterServices()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
				fonts.AddFont("RobotoFlex.ttf", "RobotoFlex");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.UseMauiApp<App>().Build();
	}
	public static MauiAppBuilder RegisterViewModelsAndViews(this MauiAppBuilder builder)
	{
		builder.Services.AddTransientWithShellRoute<TodoItemPage, TodoItemViewModel>("TodoItemPage");
		builder.Services.AddTransientWithShellRoute<TodoListPage, TodoListViewModel>("TodoListPage");
		return builder;
	}
	public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();

		return builder;
	}
}

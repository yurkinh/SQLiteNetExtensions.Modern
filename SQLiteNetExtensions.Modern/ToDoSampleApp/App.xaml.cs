using ToDoSampleApp.Services;

namespace ToDoSampleApp;

public partial class App : Application
{
	IDatabaseService databaseService;
	public App(IDatabaseService databaseService)
	{
		InitializeComponent();

		MainPage = new AppShell();

		this.databaseService = databaseService;
	}

	protected override async void OnStart()
	{
		await databaseService.Init();
	}
}

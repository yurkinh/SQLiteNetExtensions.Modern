using ToDoSampleApp.Services;

namespace ToDoSampleApp;

public partial class App : Application
{
	IDatabaseService databaseService;
	public App(IDatabaseService databaseService)
	{
		InitializeComponent();
		this.databaseService = databaseService;
	}
	protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());

	protected override async void OnStart()
	{
		await databaseService.Init();
	}
}

using VIO.App.Constants;
using VIO.App.Interfaces;

namespace VIO.App;

public partial class App : Application
{
    private readonly ISessionService _sessionService;

    public App(
        ISessionService sessionService)
    {
        InitializeComponent();

        _sessionService = sessionService;
    }

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        var shell = new AppShell();

        shell.Loaded += async (_, _) =>
        {
            var initialRoute =
                _sessionService.IsAuthenticated
                    ? AppRoutes.Home
                    : AppRoutes.Login;

            await shell.GoToAsync(initialRoute);
        };

        return new Window(shell);
    }
}

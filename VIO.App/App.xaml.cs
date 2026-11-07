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
            // Durante el desarrollo y la exposición,
            // VIO siempre inicia mostrando Login.
            // Las cuentas guardadas en SQLite NO se eliminan.
            _sessionService.End();

            await shell.GoToAsync(
                AppRoutes.Login);
        };

        return new Window(shell);
    }
}

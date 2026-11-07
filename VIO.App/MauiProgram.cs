using Microsoft.Extensions.Logging;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });

        builder.Services.AddSingleton<VioDatabase>();

        builder.Services.AddSingleton<
            ISessionService,
            SessionService>();

        builder.Services.AddSingleton<
            IAuthService,
            AuthService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

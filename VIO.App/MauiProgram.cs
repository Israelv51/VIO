using Microsoft.Extensions.Logging;
using VIO.App.Interfaces;
using VIO.App.Services;
using VIO.App.ViewModels;
using VIO.App.Views;

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

        // Base de datos y servicios
        builder.Services.AddSingleton<VioDatabase>();

        builder.Services.AddSingleton<
            ISessionService,
            SessionService>();

        builder.Services.AddSingleton<
            IAuthService,
            AuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Páginas con ViewModel
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ProfilePage>();

        // Páginas simples
        builder.Services.AddTransient<CvPage>();
        builder.Services.AddTransient<DocumentsPage>();
        builder.Services.AddTransient<CertificationsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

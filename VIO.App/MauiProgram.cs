using CommunityToolkit.Maui;
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
            .UseMauiCommunityToolkitCamera()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });

        // =====================
        // SERVICIOS
        // =====================

        builder.Services.AddSingleton<VioDatabase>();

        builder.Services.AddSingleton<
            ISessionService,
            SessionService>();

        builder.Services.AddSingleton<
            IAuthService,
            AuthService>();

        builder.Services.AddSingleton<DocumentStorageService>();
        builder.Services.AddSingleton<PdfDocumentService>();
        builder.Services.AddSingleton<CvPdfService>();

        // =====================
        // VIEWMODELS
        // =====================

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<ProfilePhotoEditorViewModel>();

        builder.Services.AddTransient<CvViewModel>();
        builder.Services.AddTransient<CvPreviewViewModel>();

        builder.Services.AddTransient<DocumentsViewModel>();
        builder.Services.AddTransient<DocumentDetailsViewModel>();
        builder.Services.AddTransient<CertificationsViewModel>();

        builder.Services.AddTransient<ProfessionalProfileViewModel>();
        builder.Services.AddTransient<EducationViewModel>();
        builder.Services.AddTransient<ExperienceViewModel>();
        builder.Services.AddTransient<SkillsViewModel>();

        // =====================
        // PÁGINAS
        // =====================

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();

        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<ProfilePhotoEditorPage>();

        builder.Services.AddTransient<CvPage>();
        builder.Services.AddTransient<CvPreviewPage>();

        builder.Services.AddTransient<DocumentsPage>();
        builder.Services.AddTransient<DocumentDetailsPage>();

        builder.Services.AddTransient<CertificationsPage>();

        builder.Services.AddTransient<ProfessionalProfilePage>();
        builder.Services.AddTransient<EducationPage>();
        builder.Services.AddTransient<ExperiencePage>();
        builder.Services.AddTransient<SkillsPage>();

        builder.Services.AddTransient<DocumentScannerPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}


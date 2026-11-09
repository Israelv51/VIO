using VIO.App.Constants;
using VIO.App.Views;

namespace VIO.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            AppRoutes.Register,
            typeof(RegisterPage));

        Routing.RegisterRoute(
            AppRoutes.Certifications,
            typeof(CertificationsPage));

        Routing.RegisterRoute(
            AppRoutes.ProfessionalProfile,
            typeof(ProfessionalProfilePage));

        Routing.RegisterRoute(
            AppRoutes.Education,
            typeof(EducationPage));

        Routing.RegisterRoute(
            AppRoutes.Experience,
            typeof(ExperiencePage));

        Routing.RegisterRoute(
            AppRoutes.Skills,
            typeof(SkillsPage));

        Routing.RegisterRoute(
            AppRoutes.DocumentScanner,
            typeof(DocumentScannerPage));

        Routing.RegisterRoute(
            AppRoutes.CvPreview,
            typeof(CvPreviewPage));

        Routing.RegisterRoute(
            AppRoutes.ProfilePhotoEditor,
            typeof(ProfilePhotoEditorPage));

        Routing.RegisterRoute(
            AppRoutes.DocumentDetails,
            typeof(DocumentDetailsPage));
    }
}

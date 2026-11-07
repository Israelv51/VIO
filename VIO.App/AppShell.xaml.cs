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
    }
}

using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(
        LoginViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}

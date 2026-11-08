using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class CertificationsPage : ContentPage
{
    private readonly CertificationsViewModel _viewModel;

    public CertificationsPage(
        CertificationsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel =
            viewModel;

        BindingContext =
            viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}

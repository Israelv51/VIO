using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class ProfessionalProfilePage : ContentPage
{
    private readonly ProfessionalProfileViewModel _viewModel;

    public ProfessionalProfilePage(
        ProfessionalProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}

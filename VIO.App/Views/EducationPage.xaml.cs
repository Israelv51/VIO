using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class EducationPage : ContentPage
{
    private readonly EducationViewModel _viewModel;

    public EducationPage(EducationViewModel viewModel)
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

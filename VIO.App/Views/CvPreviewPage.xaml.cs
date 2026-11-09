using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class CvPreviewPage : ContentPage
{
    private readonly CvPreviewViewModel _viewModel;

    public CvPreviewPage(
        CvPreviewViewModel viewModel)
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

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

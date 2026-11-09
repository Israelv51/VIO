using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class ProfilePhotoEditorPage : ContentPage
{
    private readonly ProfilePhotoEditorViewModel _viewModel;

    public ProfilePhotoEditorPage(
        ProfilePhotoEditorViewModel viewModel)
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

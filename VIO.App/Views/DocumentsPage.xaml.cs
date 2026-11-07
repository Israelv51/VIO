using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class DocumentsPage : ContentPage
{
    private readonly DocumentsViewModel _viewModel;

    public DocumentsPage(
        DocumentsViewModel viewModel)
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

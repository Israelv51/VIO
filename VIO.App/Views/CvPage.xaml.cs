using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class CvPage : ContentPage
{
    private readonly CvViewModel _viewModel;

    public CvPage(CvViewModel viewModel)
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

using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class ExperiencePage : ContentPage
{
    private readonly ExperienceViewModel _viewModel;

    public ExperiencePage(ExperienceViewModel viewModel)
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

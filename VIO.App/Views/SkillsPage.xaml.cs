using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class SkillsPage : ContentPage
{
    private readonly SkillsViewModel _viewModel;

    public SkillsPage(SkillsViewModel viewModel)
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

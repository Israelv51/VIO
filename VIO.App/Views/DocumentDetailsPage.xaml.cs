using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class DocumentDetailsPage :
    ContentPage,
    IQueryAttributable
{
    private readonly DocumentDetailsViewModel _viewModel;

    public DocumentDetailsPage(
        DocumentDetailsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (!query.TryGetValue(
                "id",
                out var value))
        {
            return;
        }

        if (!int.TryParse(
                value?.ToString(),
                out int id))
        {
            return;
        }

        Dispatcher.Dispatch(
            async () =>
                await _viewModel.LoadAsync(id));
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

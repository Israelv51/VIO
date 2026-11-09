using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Models;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel
{
    [RelayCommand]
    private async Task DetailsAsync(
        VioDocument document)
    {
        if (document is null)
            return;

        await Shell.Current.GoToAsync(
            $"{AppRoutes.DocumentDetails}?id={document.Id}");
    }
}

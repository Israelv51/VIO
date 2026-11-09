using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Models;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel
{
    [RelayCommand]
    private async Task PreviewAsync(
        VioDocument document)
    {
        if (document is null)
            return;

        await Shell.Current.GoToAsync(
            $"{AppRoutes.DocumentPreview}?id={document.Id}");
    }
}

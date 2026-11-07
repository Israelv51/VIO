using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel
{
    [RelayCommand]
    private async Task OpenScannerAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.DocumentScanner);
    }
}

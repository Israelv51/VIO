using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;

namespace VIO.App.ViewModels;

public partial class CvViewModel
{
    [RelayCommand]
    private async Task PreviewAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.CvPreview);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;

namespace VIO.App.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;

    public ProfileViewModel(
        ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [ObservableProperty]
    public partial string Name { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        var user =
            await _sessionService.GetCurrentUserAsync();

        if (user is null)
        {
            _sessionService.End();

            await Shell.Current.GoToAsync(
                AppRoutes.Login);

            return;
        }

        Name = user.Name;
        Email = user.Email;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _sessionService.End();

        await Shell.Current.GoToAsync(
            AppRoutes.Login);
    }
}

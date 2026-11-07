using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;

namespace VIO.App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;

    public HomeViewModel(
        ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [ObservableProperty]
    public partial string Name { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Bienvenido a VIO";

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

        Greeting =
            $"Hola, {GetFirstName(user.Name)}";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _sessionService.End();

        Name = "Usuario";
        Email = string.Empty;

        await Shell.Current.GoToAsync(
            AppRoutes.Login);
    }

    private static string GetFirstName(
        string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Usuario";

        return fullName
            .Trim()
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?? "Usuario";
    }
}

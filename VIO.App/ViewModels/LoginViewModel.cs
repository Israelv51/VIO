using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;

namespace VIO.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var result = await _authService.LoginAsync(
                Email,
                Password);

            if (!result.Success)
            {
                ErrorMessage = result.Message;
                return;
            }

            Password = string.Empty;

            await Shell.Current.GoToAsync(
                AppRoutes.Home);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        ErrorMessage = string.Empty;

        await Shell.Current.GoToAsync(
            AppRoutes.Register);
    }
}

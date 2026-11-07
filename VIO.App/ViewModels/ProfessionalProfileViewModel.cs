using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class ProfessionalProfileViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public ProfessionalProfileViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string ProfessionalTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string City { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LinkedIn { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GitHub { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        var profile =
            await _database.GetProfessionalProfileAsync(userId);

        if (profile is null)
            return;

        ProfessionalTitle = profile.ProfessionalTitle;
        Summary = profile.Summary;
        Phone = profile.Phone;
        City = profile.City;
        LinkedIn = profile.LinkedIn;
        GitHub = profile.GitHub;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        await _database.SaveProfessionalProfileAsync(
            new ProfessionalProfile
            {
                UserId = userId,
                ProfessionalTitle = ProfessionalTitle.Trim(),
                Summary = Summary.Trim(),
                Phone = Phone.Trim(),
                City = City.Trim(),
                LinkedIn = LinkedIn.Trim(),
                GitHub = GitHub.Trim()
            });

        Message = "Perfil guardado correctamente.";
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class CvViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public CvViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string ProfessionalTitle { get; set; }
        = "Completa tu perfil profesional";

    [ObservableProperty]
    public partial string Summary { get; set; }
        = "Agrega información para construir tu CV.";

    [ObservableProperty]
    public partial int EducationCount { get; set; }

    [ObservableProperty]
    public partial int ExperienceCount { get; set; }

    [ObservableProperty]
    public partial int SkillCount { get; set; }

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
        {
            await Shell.Current.GoToAsync(AppRoutes.Login);
            return;
        }

        var profile =
            await _database.GetProfessionalProfileAsync(userId);

        ProfessionalTitle =
            string.IsNullOrWhiteSpace(profile?.ProfessionalTitle)
                ? "Completa tu perfil profesional"
                : profile.ProfessionalTitle;

        Summary =
            string.IsNullOrWhiteSpace(profile?.Summary)
                ? "Agrega información para construir tu CV."
                : profile.Summary;

        EducationCount =
            (await _database.GetEducationAsync(userId)).Count;

        ExperienceCount =
            (await _database.GetExperiencesAsync(userId)).Count;

        SkillCount =
            (await _database.GetSkillsAsync(userId)).Count;
    }

    [RelayCommand]
    private async Task EditProfileAsync() =>
        await Shell.Current.GoToAsync(AppRoutes.ProfessionalProfile);

    [RelayCommand]
    private async Task EducationAsync() =>
        await Shell.Current.GoToAsync(AppRoutes.Education);

    [RelayCommand]
    private async Task ExperienceAsync() =>
        await Shell.Current.GoToAsync(AppRoutes.Experience);

    [RelayCommand]
    private async Task SkillsAsync() =>
        await Shell.Current.GoToAsync(AppRoutes.Skills);
}

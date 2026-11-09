using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public HomeViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string Name { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string FirstName { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProfessionalTitle { get; set; }
        = "Construye tu identidad profesional";

    [ObservableProperty]
    public partial string ProfilePhotoPath { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial int CompletionPercentage { get; set; }

    [ObservableProperty]
    public partial double CompletionProgress { get; set; }

    [ObservableProperty]
    public partial string CompletionText { get; set; }
        = "0% completado";

    [ObservableProperty]
    public partial int DocumentsCount { get; set; }

    [ObservableProperty]
    public partial int CertificationsCount { get; set; }

    [ObservableProperty]
    public partial int SkillsCount { get; set; }

    [ObservableProperty]
    public partial int ExperienceCount { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; }
        = "Completa tu perfil para comenzar.";

    public async Task LoadAsync()
    {
        var user =
            await _session.GetCurrentUserAsync();

        if (user is null)
        {
            _session.End();

            await Shell.Current.GoToAsync(
                AppRoutes.Login);

            return;
        }

        Name = user.Name;

        FirstName =
            GetFirstName(user.Name);

        Email =
            user.Email;

        ProfilePhotoPath =
            user.ProfilePhotoPath ?? string.Empty;

        int userId =
            user.Id;

        var profile =
            await _database.GetProfessionalProfileAsync(
                userId);

        var education =
            await _database.GetEducationAsync(
                userId);

        var experience =
            await _database.GetExperiencesAsync(
                userId);

        var skills =
            await _database.GetSkillsAsync(
                userId);

        var certifications =
            await _database.GetCertificationsAsync(
                userId);

        var documents =
            await _database.GetDocumentsAsync(
                userId);

        ProfessionalTitle =
            string.IsNullOrWhiteSpace(
                profile?.ProfessionalTitle)
                ? "Construye tu identidad profesional"
                : profile.ProfessionalTitle;

        DocumentsCount =
            documents.Count;

        CertificationsCount =
            certifications.Count;

        SkillsCount =
            skills.Count;

        ExperienceCount =
            experience.Count;

        int score = 0;

        if (profile is not null &&
            !string.IsNullOrWhiteSpace(
                profile.ProfessionalTitle) &&
            !string.IsNullOrWhiteSpace(
                profile.Summary))
        {
            score += 20;
        }

        if (education.Count > 0)
            score += 20;

        if (experience.Count > 0)
            score += 20;

        if (skills.Count > 0)
            score += 20;

        if (certifications.Count > 0)
            score += 20;

        CompletionPercentage =
            score;

        CompletionProgress =
            score / 100.0;

        CompletionText =
            $"{score}% completado";

        StatusMessage =
            score switch
            {
                100 =>
                    "Tu perfil VIO está completo y listo para compartir.",

                >= 80 =>
                    "Ya casi terminas tu identidad profesional.",

                >= 40 =>
                    "Vas construyendo un perfil profesional sólido.",

                _ =>
                    "Agrega información para fortalecer tu perfil."
            };
    }

    [RelayCommand]
    private async Task GoToCvAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.Cv);
    }

    [RelayCommand]
    private async Task GoToDocumentsAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.Documents);
    }

    [RelayCommand]
    private async Task GoToProfileAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.Profile);
    }

    [RelayCommand]
    private async Task GoToCertificationsAsync()
    {
        await Shell.Current.GoToAsync(
            AppRoutes.Certifications);
    }

    private static string GetFirstName(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Usuario";

        return name
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?? "Usuario";
    }
}

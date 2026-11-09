using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class CvViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly CvPdfService _cvPdfService;
    private readonly DocumentStorageService _storage;

    public CvViewModel(
        ISessionService session,
        VioDatabase database,
        CvPdfService cvPdfService,
        DocumentStorageService storage)
    {
        _session = session;
        _database = database;
        _cvPdfService = cvPdfService;
        _storage = storage;
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

    [ObservableProperty]
    public partial int CertificationCount { get; set; }

    [ObservableProperty]
    public partial int CompletionPercentage { get; set; }

    [ObservableProperty]
    public partial double CompletionProgress { get; set; }

    [ObservableProperty]
    public partial string CompletionText { get; set; }
        = "0% completado";

    [ObservableProperty]
    public partial string Message { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial bool IsGenerating { get; set; }

    [ObservableProperty]
    public partial VioDocument? LastGeneratedCv { get; set; }

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
        {
            await Shell.Current.GoToAsync(
                AppRoutes.Login);

            return;
        }

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

        ProfessionalTitle =
            string.IsNullOrWhiteSpace(
                profile?.ProfessionalTitle)
                ? "Completa tu perfil profesional"
                : profile.ProfessionalTitle;

        Summary =
            string.IsNullOrWhiteSpace(
                profile?.Summary)
                ? "Agrega información para construir tu CV."
                : profile.Summary;

        EducationCount =
            education.Count;

        ExperienceCount =
            experience.Count;

        SkillCount =
            skills.Count;

        CertificationCount =
            certifications.Count;

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
    }

    [RelayCommand]
    private async Task GeneratePdfAsync()
    {
        if (IsGenerating)
            return;

        try
        {
            IsGenerating = true;

            Message =
                "Generando tu CV...";

            var document =
                await _cvPdfService.GenerateAsync();

            await _database.AddDocumentAsync(
                document);

            LastGeneratedCv =
                document;

            Message =
                "CV generado y guardado en Documentos.";
        }
        catch (Exception ex)
        {
            Message =
                ex.Message;
        }
        finally
        {
            IsGenerating =
                false;
        }
    }

    [RelayCommand]
    private async Task GenerateAndSharePdfAsync()
    {
        if (IsGenerating)
            return;

        try
        {
            IsGenerating = true;

            Message =
                "Preparando CV...";

            var document =
                await _cvPdfService.GenerateAsync();

            await _database.AddDocumentAsync(
                document);

            LastGeneratedCv =
                document;

            await _storage.ShareAsync(
                document);

            Message =
                "CV listo para compartir.";
        }
        catch (Exception ex)
        {
            Message =
                ex.Message;
        }
        finally
        {
            IsGenerating =
                false;
        }
    }

    [RelayCommand]
    private async Task OpenLastCvAsync()
    {
        if (LastGeneratedCv is null)
        {
            Message =
                "Primero genera tu CV.";

            return;
        }

        await _storage.OpenAsync(
            LastGeneratedCv);
    }

    [RelayCommand]
    private async Task EditProfileAsync() =>
        await Shell.Current.GoToAsync(
            AppRoutes.ProfessionalProfile);

    [RelayCommand]
    private async Task EducationAsync() =>
        await Shell.Current.GoToAsync(
            AppRoutes.Education);

    [RelayCommand]
    private async Task ExperienceAsync() =>
        await Shell.Current.GoToAsync(
            AppRoutes.Experience);

    [RelayCommand]
    private async Task SkillsAsync() =>
        await Shell.Current.GoToAsync(
            AppRoutes.Skills);
}

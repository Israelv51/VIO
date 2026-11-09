using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class CvPreviewViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public ObservableCollection<Education> Education { get; } = [];
    public ObservableCollection<WorkExperience> Experiences { get; } = [];
    public ObservableCollection<Skill> Skills { get; } = [];
    public ObservableCollection<Certification> Certifications { get; } = [];

    public CvPreviewViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string Name { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string Initials { get; set; } = "VIO";

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProfessionalTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Summary { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string City { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string GitHub { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LinkedIn { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhotoPath { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        var user =
            await _session.GetCurrentUserAsync();

        if (user is null)
            return;

        var profile =
            await _database.GetProfessionalProfileAsync(
                user.Id);

        Name =
            user.Name;

        Initials =
            GetInitials(user.Name);

        Email =
            user.Email;

        PhotoPath =
            user.ProfilePhotoPath ?? string.Empty;

        ProfessionalTitle =
            profile?.ProfessionalTitle
            ?? "Perfil profesional";

        Summary =
            profile?.Summary
            ?? string.Empty;

        Phone =
            profile?.Phone
            ?? string.Empty;

        City =
            profile?.City
            ?? string.Empty;

        GitHub =
            profile?.GitHub
            ?? string.Empty;

        LinkedIn =
            profile?.LinkedIn
            ?? string.Empty;

        Education.Clear();

        foreach (var item in
                 await _database.GetEducationAsync(user.Id))
        {
            Education.Add(item);
        }

        Experiences.Clear();

        foreach (var item in
                 await _database.GetExperiencesAsync(user.Id))
        {
            Experiences.Add(item);
        }

        Skills.Clear();

        foreach (var item in
                 await _database.GetSkillsAsync(user.Id))
        {
            Skills.Add(item);
        }

        Certifications.Clear();

        foreach (var item in
                 await _database.GetCertificationsAsync(user.Id))
        {
            Certifications.Add(item);
        }
    }

    private static string GetInitials(string name)
    {
        var parts =
            name.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return "VIO";

        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpperInvariant();

        return $"{parts[0][0]}{parts[1][0]}"
            .ToUpperInvariant();
    }
}

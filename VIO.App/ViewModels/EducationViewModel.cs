using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class EducationViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public ObservableCollection<Education> Items { get; } = [];

    public EducationViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string Institution { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Degree { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime StartDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime EndDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Education? SelectedEducation { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        Items.Clear();

        foreach (var item in await _database.GetEducationAsync(userId))
            Items.Add(item);
    }

    partial void OnSelectedEducationChanged(Education? value)
    {
        if (value is null)
            return;

        Institution = value.Institution;
        Degree = value.Degree;
        StartDate = value.StartDate;
        EndDate = value.EndDate;
        IsCurrent = value.IsCurrent;
        Description = value.Description;
        IsEditing = true;
        Message = "Editando registro seleccionado.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (string.IsNullOrWhiteSpace(Institution) ||
            string.IsNullOrWhiteSpace(Degree))
        {
            Message = "Completa institución y carrera.";
            return;
        }

        var item = new Education
        {
            Id = SelectedEducation?.Id ?? 0,
            UserId = userId,
            Institution = Institution.Trim(),
            Degree = Degree.Trim(),
            StartDate = StartDate,
            EndDate = EndDate,
            IsCurrent = IsCurrent,
            Description = Description.Trim(),
            CreatedAt = SelectedEducation?.CreatedAt ?? DateTime.UtcNow
        };

        await _database.SaveEducationAsync(item);

        ResetForm();
        await LoadAsync();

        Message = "Formación guardada.";
    }

    [RelayCommand]
    private void NewEducation()
    {
        ResetForm();
        Message = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteEducationAsync()
    {
        if (SelectedEducation is null)
            return;

        await _database.DeleteEducationAsync(SelectedEducation);

        ResetForm();
        await LoadAsync();

        Message = "Registro eliminado.";
    }

    private void ResetForm()
    {
        SelectedEducation = null;
        Institution = string.Empty;
        Degree = string.Empty;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        IsCurrent = false;
        Description = string.Empty;
        IsEditing = false;
    }
}

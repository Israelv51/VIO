using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class ExperienceViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public ObservableCollection<WorkExperience> Items { get; } = [];

    public ExperienceViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string Company { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Position { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime StartDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime EndDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial bool IsCurrent { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial WorkExperience? SelectedExperience { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        Items.Clear();

        foreach (var item in await _database.GetExperiencesAsync(userId))
            Items.Add(item);
    }

    partial void OnSelectedExperienceChanged(WorkExperience? value)
    {
        if (value is null)
            return;

        Company = value.Company;
        Position = value.Position;
        StartDate = value.StartDate;
        EndDate = value.EndDate;
        IsCurrent = value.IsCurrent;
        Description = value.Description;
        IsEditing = true;
        Message = "Editando experiencia seleccionada.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (string.IsNullOrWhiteSpace(Company) ||
            string.IsNullOrWhiteSpace(Position))
        {
            Message = "Completa empresa y puesto.";
            return;
        }

        var item = new WorkExperience
        {
            Id = SelectedExperience?.Id ?? 0,
            UserId = userId,
            Company = Company.Trim(),
            Position = Position.Trim(),
            StartDate = StartDate,
            EndDate = EndDate,
            IsCurrent = IsCurrent,
            Description = Description.Trim(),
            CreatedAt = SelectedExperience?.CreatedAt ?? DateTime.UtcNow
        };

        await _database.SaveExperienceAsync(item);

        ResetForm();
        await LoadAsync();

        Message = "Experiencia guardada.";
    }

    [RelayCommand]
    private void NewExperience()
    {
        ResetForm();
        Message = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteExperienceAsync()
    {
        if (SelectedExperience is null)
            return;

        await _database.DeleteExperienceAsync(SelectedExperience);

        ResetForm();
        await LoadAsync();

        Message = "Experiencia eliminada.";
    }

    private void ResetForm()
    {
        SelectedExperience = null;
        Company = string.Empty;
        Position = string.Empty;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        IsCurrent = false;
        Description = string.Empty;
        IsEditing = false;
    }
}

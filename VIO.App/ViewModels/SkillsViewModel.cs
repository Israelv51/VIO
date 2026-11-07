using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class SkillsViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    public ObservableCollection<Skill> Items { get; } = [];

    public IReadOnlyList<string> Levels { get; } =
        ["Básico", "Intermedio", "Avanzado", "Experto"];

    public SkillsViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string SkillName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedLevel { get; set; } = "Intermedio";

    [ObservableProperty]
    public partial Skill? SelectedSkill { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        Items.Clear();

        foreach (var item in await _database.GetSkillsAsync(userId))
            Items.Add(item);
    }

    partial void OnSelectedSkillChanged(Skill? value)
    {
        if (value is null)
            return;

        SkillName = value.Name;
        SelectedLevel = value.Level;
        IsEditing = true;
        Message = "Editando habilidad seleccionada.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (string.IsNullOrWhiteSpace(SkillName))
        {
            Message = "Escribe una habilidad.";
            return;
        }

        var item = new Skill
        {
            Id = SelectedSkill?.Id ?? 0,
            UserId = userId,
            Name = SkillName.Trim(),
            Level = SelectedLevel,
            CreatedAt = SelectedSkill?.CreatedAt ?? DateTime.UtcNow
        };

        await _database.SaveSkillAsync(item);

        ResetForm();
        await LoadAsync();

        Message = "Habilidad guardada.";
    }

    [RelayCommand]
    private void NewSkill()
    {
        ResetForm();
        Message = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteSkillAsync()
    {
        if (SelectedSkill is null)
            return;

        await _database.DeleteSkillAsync(SelectedSkill);

        ResetForm();
        await LoadAsync();

        Message = "Habilidad eliminada.";
    }

    private void ResetForm()
    {
        SelectedSkill = null;
        SkillName = string.Empty;
        SelectedLevel = "Intermedio";
        IsEditing = false;
    }
}

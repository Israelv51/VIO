using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class DocumentDetailsViewModel :
    ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly DocumentStorageService _storage;

    private VioDocument? _document;

    public IReadOnlyList<string> Categories { get; } =
    [
        "Documento oficial",
        "Identificación",
        "Escolar",
        "Certificación",
        "Laboral",
        "Curriculum Vitae",
        "Comprobante",
        "Personal",
        "Otro"
    ];

    public DocumentDetailsViewModel(
        ISessionService session,
        VioDatabase database,
        DocumentStorageService storage)
    {
        _session = session;
        _database = database;
        _storage = storage;
    }

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Category { get; set; } = "General";

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPrivate { get; set; } = true;

    [ObservableProperty]
    public partial bool IsOfficialDocument { get; set; }

    [ObservableProperty]
    public partial string FileInfo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync(int documentId)
    {
        if (_session.CurrentUserId is not int userId)
            return;

        _document =
            await _database.GetDocumentByIdAsync(
                userId,
                documentId);

        if (_document is null)
        {
            Message =
                "No se encontró el documento.";

            return;
        }

        DisplayName =
            _document.DisplayName;

        Category =
            _document.Category;

        Description =
            _document.Description;

        IsPrivate =
            _document.IsPrivate;

        IsOfficialDocument =
            _document.IsOfficialDocument;

        FileInfo =
            $"{_document.TypeLabel} · {_document.SizeText} · {_document.DateText}";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_document is null)
            return;

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            Message =
                "El documento necesita un nombre.";

            return;
        }

        _document.DisplayName =
            DisplayName.Trim();

        _document.Category =
            Category;

        _document.Description =
            Description.Trim();

        _document.IsPrivate =
            IsPrivate;

        _document.IsOfficialDocument =
            IsOfficialDocument;

        await _database.UpdateDocumentAsync(
            _document);

        Message =
            "Documento actualizado.";
    }

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (_document is null)
            return;

        await _storage.OpenAsync(
            _document);
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        if (_document is null)
            return;

        await _storage.ShareAsync(
            _document);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_document is null)
            return;

        await _storage.DeleteFileAsync(
            _document);

        await _database.DeleteDocumentAsync(
            _document);

        await Shell.Current.GoToAsync("..");
    }
}

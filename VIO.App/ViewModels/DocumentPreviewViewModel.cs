using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class DocumentPreviewViewModel :
    ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly DocumentStorageService _storage;
    private readonly DocumentPreviewService _preview;

    private VioDocument? _document;

    public DocumentPreviewViewModel(
        ISessionService session,
        VioDatabase database,
        DocumentStorageService storage,
        DocumentPreviewService preview)
    {
        _session = session;
        _database = database;
        _storage = storage;
        _preview = preview;
    }

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PreviewPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FileInfo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Category { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PrivacyText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    public async Task LoadAsync(
        int documentId)
    {
        if (_session.CurrentUserId is not int userId)
            return;

        try
        {
            IsLoading = true;

            _document =
                await _database.GetDocumentByIdAsync(
                    userId,
                    documentId);

            if (_document is null)
            {
                Message =
                    "Documento no encontrado.";

                return;
            }

            DisplayName =
                _document.DisplayName;

            Category =
                _document.Category;

            PrivacyText =
                $"{_document.PrivacyIcon} {_document.PrivacyText}";

            FileInfo =
                $"{_document.TypeLabel} · {_document.SizeText} · {_document.DateText}";

            PreviewPath =
                await _preview.GetPreviewAsync(
                    _document)
                ?? string.Empty;
        }
        catch (Exception ex)
        {
            Message =
                $"No fue posible generar la vista previa: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
}

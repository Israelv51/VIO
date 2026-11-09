using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Constants;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly DocumentStorageService _storage;
    private readonly PdfDocumentService _pdfService;

    public ObservableCollection<VioDocument> Items { get; }
        = [];

    public DocumentsViewModel(
        ISessionService session,
        VioDatabase database,
        DocumentStorageService storage,
        PdfDocumentService pdfService)
    {
        _session = session;
        _database = database;
        _storage = storage;
        _pdfService = pdfService;
    }

    [ObservableProperty]
    public partial string SearchText { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; }
        = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool HasDocuments { get; set; }

    partial void OnSearchTextChanged(
        string value)
    {
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        var documents =
            await _database.GetDocumentsAsync(
                userId,
                SearchText);

        Items.Clear();

        foreach (var item in documents)
            Items.Add(item);

        HasDocuments =
            Items.Count > 0;
    }

    [RelayCommand]
    private async Task AddDocumentAsync()
    {
        if (IsBusy ||
            _session.CurrentUserId is not int userId)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var document =
                await _storage.PickAndImportAsync(
                    userId);

            if (document is null)
                return;

            await _database.AddDocumentAsync(
                document);

            await LoadAsync();

            Message =
                "Documento agregado correctamente.";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreatePdfFromPhotosAsync()
    {
        if (IsBusy ||
            _session.CurrentUserId is not int userId)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message =
                "Selecciona las imágenes en el orden de las páginas.";

            var document =
                await _pdfService.CreateFromPhotosAsync(
                    userId);

            if (document is null)
            {
                Message =
                    "Operación cancelada.";
                return;
            }

            await _database.AddDocumentAsync(
                document);

            await LoadAsync();
            await RefreshHubAsync();

            Message =
                "PDF creado correctamente.";

            await Shell.Current.GoToAsync(
                $"{AppRoutes.DocumentDetails}?id={document.Id}");
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ScanTwoSidedAsync()
    {
        if (IsBusy ||
            _session.CurrentUserId is not int userId)
        {
            return;
        }

        try
        {
            IsBusy = true;

            Message =
                "Primero fotografía el frente y después el reverso.";

            var document =
                await _pdfService.CaptureTwoSidedAsync(
                    userId);

            if (document is null)
            {
                Message =
                    "Captura cancelada.";
                return;
            }

            await _database.AddDocumentAsync(
                document);

            await LoadAsync();

            Message =
                "Documento doble cara creado correctamente.";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDocumentAsync(
        VioDocument document)
    {
        if (document is null)
            return;

        try
        {
            await _storage.OpenAsync(
                document);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private async Task ShareDocumentAsync(
        VioDocument document)
    {
        if (document is null)
            return;

        try
        {
            await _storage.ShareAsync(
                document);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeleteDocumentAsync(
        VioDocument document)
    {
        if (document is null)
            return;

        await _storage.DeleteFileAsync(
            document);

        await _database.DeleteDocumentAsync(
            document);

        await LoadAsync();

        Message =
            "Documento eliminado.";
    }
}



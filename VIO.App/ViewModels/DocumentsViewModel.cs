using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel : ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly DocumentStorageService _storage;

    public ObservableCollection<VioDocument> Items { get; }
        = [];

    public DocumentsViewModel(
        ISessionService session,
        VioDatabase database,
        DocumentStorageService storage)
    {
        _session = session;
        _database = database;
        _storage = storage;
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
            Message =
                ex.Message;
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

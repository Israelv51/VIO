using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Interfaces;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class CertificationsViewModel :
    ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;
    private readonly DocumentStorageService _storage;

    public ObservableCollection<Certification> Items { get; } = [];

    public ObservableCollection<VioDocument> AvailableDocuments { get; } = [];

    public CertificationsViewModel(
        ISessionService session,
        VioDatabase database,
        DocumentStorageService storage)
    {
        _session = session;
        _database = database;
        _storage = storage;
    }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Issuer { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime IssueDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime ExpirationDate { get; set; } =
        DateTime.Today.AddYears(1);

    [ObservableProperty]
    public partial bool DoesNotExpire { get; set; }

    [ObservableProperty]
    public partial string CredentialId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CredentialUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Certification? SelectedCertification { get; set; }

    [ObservableProperty]
    public partial VioDocument? SelectedDocument { get; set; }

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadAsync();
    }

    partial void OnSelectedCertificationChanged(
        Certification? value)
    {
        if (value is null)
            return;

        Name = value.Name;
        Issuer = value.Issuer;
        IssueDate = value.IssueDate;
        ExpirationDate = value.ExpirationDate;
        DoesNotExpire = value.DoesNotExpire;
        CredentialId = value.CredentialId;
        CredentialUrl = value.CredentialUrl;
        Notes = value.Notes;

        SelectedDocument =
            value.RelatedDocumentId.HasValue
                ? AvailableDocuments.FirstOrDefault(
                    x => x.Id ==
                         value.RelatedDocumentId.Value)
                : null;

        IsEditing = true;

        Message =
            "Editando certificación.";
    }

    public async Task LoadAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        await LoadDocumentsAsync(userId);

        var certifications =
            await _database.GetCertificationsAsync(
                userId,
                SearchText);

        foreach (var certification in certifications)
        {
            if (certification.RelatedDocumentId is int documentId)
            {
                var document =
                    AvailableDocuments.FirstOrDefault(
                        x => x.Id == documentId);

                certification.RelatedDocumentName =
                    document?.DisplayName
                    ?? "Documento no disponible";
            }
            else
            {
                certification.RelatedDocumentName =
                    "Sin evidencia vinculada";
            }
        }

        Items.Clear();

        foreach (var item in certifications)
            Items.Add(item);
    }

    private async Task LoadDocumentsAsync(int userId)
    {
        var documents =
            await _database.GetDocumentsAsync(userId);

        AvailableDocuments.Clear();

        foreach (var item in documents)
            AvailableDocuments.Add(item);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            Message =
                "Escribe el nombre de la certificación.";

            return;
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            Message =
                "Escribe la institución emisora.";

            return;
        }

        var item =
            new Certification
            {
                Id =
                    SelectedCertification?.Id ?? 0,

                UserId =
                    userId,

                Name =
                    Name.Trim(),

                Issuer =
                    Issuer.Trim(),

                IssueDate =
                    IssueDate,

                ExpirationDate =
                    ExpirationDate,

                DoesNotExpire =
                    DoesNotExpire,

                CredentialId =
                    CredentialId.Trim(),

                CredentialUrl =
                    CredentialUrl.Trim(),

                Notes =
                    Notes.Trim(),

                RelatedDocumentId =
                    SelectedDocument?.Id,

                CreatedAt =
                    SelectedCertification?.CreatedAt
                    ?? DateTime.UtcNow
            };

        await _database.SaveCertificationAsync(item);

        Reset();

        await LoadAsync();

        Message =
            "Certificación guardada correctamente.";
    }

    [RelayCommand]
    private void NewCertification()
    {
        Reset();
        Message = string.Empty;
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedCertification is null)
            return;

        await _database.DeleteCertificationAsync(
            SelectedCertification);

        Reset();

        await LoadAsync();

        Message =
            "Certificación eliminada.";
    }

    [RelayCommand]
    private async Task OpenEvidenceAsync(
        Certification certification)
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (certification.RelatedDocumentId is not int documentId)
        {
            Message =
                "Esta certificación todavía no tiene evidencia vinculada.";

            return;
        }

        try
        {
            var document =
                await _database.GetDocumentByIdAsync(
                    userId,
                    documentId);

            if (document is null)
            {
                Message =
                    "El documento vinculado ya no existe.";

                return;
            }

            await _storage.OpenAsync(document);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private void Reset()
    {
        SelectedCertification = null;
        SelectedDocument = null;

        Name = string.Empty;
        Issuer = string.Empty;

        IssueDate = DateTime.Today;

        ExpirationDate =
            DateTime.Today.AddYears(1);

        DoesNotExpire = false;

        CredentialId = string.Empty;
        CredentialUrl = string.Empty;
        Notes = string.Empty;

        IsEditing = false;
    }
}

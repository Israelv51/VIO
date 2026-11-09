using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VIO.App.Constants;
using VIO.App.Models;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class DocumentsViewModel
{
    public ObservableCollection<DocumentSuggestion>
        BaseSuggestions { get; } = [];

    public ObservableCollection<DocumentSuggestion>
        OptionalSuggestions { get; } = [];

    [ObservableProperty]
    public partial int EmploymentReadyCount { get; set; }

    [ObservableProperty]
    public partial int EmploymentTotalCount { get; set; }

    [ObservableProperty]
    public partial int MissingEmploymentCount { get; set; }

    [ObservableProperty]
    public partial double EmploymentProgress { get; set; }

    [ObservableProperty]
    public partial string EmploymentProgressText { get; set; }
        = "0 de 0";

    [ObservableProperty]
    public partial string EmploymentSuggestionText { get; set; }
        = "Comienza a construir tu expediente.";

    public async Task RefreshHubAsync()
    {
        if (_session.CurrentUserId is not int userId)
            return;

        var documents =
            await _database.GetDocumentsAsync(userId);

        var suggestions =
            EmploymentDocumentCatalog.Create();

        foreach (var suggestion in suggestions)
        {
            var match =
                documents.FirstOrDefault(
                    document =>
                        Matches(
                            document,
                            suggestion));

            suggestion.IsAvailable =
                match is not null;

            suggestion.MatchedDocumentId =
                match?.Id;
        }

        BaseSuggestions.Clear();
        OptionalSuggestions.Clear();

        foreach (var item in
                 suggestions.Where(
                     x => x.Group == "Base"))
        {
            BaseSuggestions.Add(item);
        }

        foreach (var item in
                 suggestions.Where(
                     x => x.Group != "Base"))
        {
            OptionalSuggestions.Add(item);
        }

        EmploymentTotalCount =
            BaseSuggestions.Count;

        EmploymentReadyCount =
            BaseSuggestions.Count(
                x => x.IsAvailable);

        MissingEmploymentCount =
            EmploymentTotalCount -
            EmploymentReadyCount;

        EmploymentProgress =
            EmploymentTotalCount == 0
                ? 0
                : EmploymentReadyCount /
                  (double)EmploymentTotalCount;

        EmploymentProgressText =
            $"{EmploymentReadyCount} de {EmploymentTotalCount}";

        EmploymentSuggestionText =
            MissingEmploymentCount switch
            {
                0 =>
                    "Tu expediente laboral base está completo.",

                1 =>
                    "Te falta 1 documento para completar tu expediente base.",

                _ =>
                    $"Te faltan {MissingEmploymentCount} documentos del expediente base."
            };
    }

    [RelayCommand]
    private async Task AddNamedDocumentAsync()
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

            string initialName =
                document.DisplayName;

            string? customName =
                await Shell.Current.DisplayPromptAsync(
                    "Guardar documento",
                    "¿Cómo quieres llamar este documento?",
                    accept: "Guardar",
                    cancel: "Cancelar",
                    placeholder: "Nombre del documento",
                    maxLength: 80,
                    initialValue: initialName);

            if (string.IsNullOrWhiteSpace(customName))
            {
                await _storage.DeleteFileAsync(
                    document);

                return;
            }

            document.DisplayName =
                customName.Trim();

            document.IsPrivate =
                true;

            await _database.AddDocumentAsync(
                document);

            await LoadAsync();
            await RefreshHubAsync();

            Message =
                "Documento guardado correctamente.";

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
    private async Task SuggestionAsync(
        DocumentSuggestion suggestion)
    {
        if (suggestion is null)
            return;

        if (suggestion.IsAvailable &&
            suggestion.MatchedDocumentId is int existingId)
        {
            await Shell.Current.GoToAsync(
                $"{AppRoutes.DocumentDetails}?id={existingId}");

            return;
        }

        if (_session.CurrentUserId is not int userId)
            return;

        try
        {
            IsBusy = true;

            var document =
                await _storage.PickAndImportAsync(
                    userId);

            if (document is null)
                return;

            string? customName =
                await Shell.Current.DisplayPromptAsync(
                    "Guardar documento",
                    "Puedes ponerle el nombre que quieras.",
                    accept: "Guardar",
                    cancel: "Cancelar",
                    placeholder: suggestion.Title,
                    maxLength: 80,
                    initialValue: suggestion.Title);

            if (string.IsNullOrWhiteSpace(customName))
            {
                await _storage.DeleteFileAsync(
                    document);

                return;
            }

            document.DisplayName =
                customName.Trim();

            document.Category =
                suggestion.Category;

            document.Description =
                suggestion.Description;

            document.IsPrivate =
                true;

            document.IsOfficialDocument =
                suggestion.IsOfficialDocument;

            await _database.AddDocumentAsync(
                document);

            await LoadAsync();
            await RefreshHubAsync();

            Message =
                $"{suggestion.Title} agregado.";

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

    private static bool Matches(
        VioDocument document,
        DocumentSuggestion suggestion)
    {
        string haystack =
            $"{document.DisplayName} " +
            $"{document.OriginalFileName} " +
            $"{document.Category} " +
            $"{document.Description}"
                .ToLowerInvariant();

        return suggestion.MatchTokens.Any(
            token =>
                haystack.Contains(
                    token.ToLowerInvariant(),
                    StringComparison.Ordinal));
    }
}


using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using VIO.App.Models;

namespace VIO.App.Services;

public class DocumentStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".heic"
        };

    public async Task<VioDocument?> PickAndImportAsync(
        int userId)
    {
        var result = await FilePicker.Default.PickAsync(
            new PickOptions
            {
                PickerTitle = "Selecciona un documento"
            });

        if (result is null)
            return null;

        string extension =
            Path.GetExtension(result.FileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "VIO acepta PDF, JPG, JPEG, PNG, WEBP y HEIC.");
        }

        string storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        string userDirectory =
            GetUserDirectory(userId);

        Directory.CreateDirectory(userDirectory);

        string destinationPath =
            Path.Combine(
                userDirectory,
                storedFileName);

        await using var source =
            await result.OpenReadAsync();

        await using var destination =
            File.Create(destinationPath);

        await source.CopyToAsync(destination);

        var info =
            new FileInfo(destinationPath);

        return new VioDocument
        {
            UserId = userId,
            DisplayName =
                Path.GetFileNameWithoutExtension(
                    result.FileName),

            OriginalFileName =
                result.FileName,

            StoredFileName =
                storedFileName,

            Extension =
                extension,

            Category =
                GetCategory(extension),

            SizeBytes =
                info.Length,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    public async Task OpenAsync(
        VioDocument document)
    {
        string sharePath =
            await PrepareSharingCopyAsync(document);

        await Launcher.Default.OpenAsync(
            new OpenFileRequest(
                document.DisplayName,
                new ReadOnlyFile(sharePath)));
    }

    public async Task ShareAsync(
        VioDocument document)
    {
        string sharePath =
            await PrepareSharingCopyAsync(document);

        await Share.Default.RequestAsync(
            new ShareFileRequest
            {
                Title =
                    $"Compartir {document.DisplayName}",

                File =
                    new ShareFile(sharePath)
            });
    }

    public Task DeleteFileAsync(
        VioDocument document)
    {
        string path =
            GetDocumentPath(document);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public string GetDocumentPath(
        VioDocument document)
    {
        return Path.Combine(
            GetUserDirectory(document.UserId),
            document.StoredFileName);
    }

    private static string GetUserDirectory(
        int userId)
    {
        return Path.Combine(
            FileSystem.Current.AppDataDirectory,
            "VioDocuments",
            userId.ToString());
    }

    private static string GetCategory(
        string extension)
    {
        return extension.Equals(
            ".pdf",
            StringComparison.OrdinalIgnoreCase)
                ? "PDF"
                : "Imagen";
    }

    private async Task<string> PrepareSharingCopyAsync(
        VioDocument document)
    {
        string source =
            GetDocumentPath(document);

        if (!File.Exists(source))
        {
            throw new FileNotFoundException(
                "El archivo ya no existe.",
                source);
        }

        string sharingDirectory =
            Path.Combine(
                FileSystem.Current.CacheDirectory,
                "sharing-root");

        Directory.CreateDirectory(
            sharingDirectory);

        string safeName =
            $"{document.Id}_{document.OriginalFileName}";

        string destination =
            Path.Combine(
                sharingDirectory,
                safeName);

        await using var input =
            File.OpenRead(source);

        await using var output =
            File.Create(destination);

        await input.CopyToAsync(output);

        return destination;
    }
}

using PDFtoImage;
using VIO.App.Models;

namespace VIO.App.Services;

public class DocumentPreviewService
{
    private readonly DocumentStorageService _storage;

    public DocumentPreviewService(
        DocumentStorageService storage)
    {
        _storage = storage;
    }

    public async Task<string?> GetPreviewAsync(
        VioDocument document)
    {
        string sourcePath =
            _storage.GetDocumentPath(document);

        if (!File.Exists(sourcePath))
            return null;

        string extension =
            document.Extension.ToLowerInvariant();

        // Las imágenes se muestran directamente.
        if (extension is
            ".jpg" or
            ".jpeg" or
            ".png" or
            ".webp")
        {
            return sourcePath;
        }

        if (extension != ".pdf")
            return null;

        // PDFtoImage tiene requisitos mínimos de plataforma.
#if ANDROID
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return null;
#elif IOS
        if (!OperatingSystem.IsIOSVersionAtLeast(13, 6))
            return null;
#elif MACCATALYST
        if (!OperatingSystem.IsMacCatalystVersionAtLeast(13, 5))
            return null;
#endif

        string directory =
            Path.Combine(
                FileSystem.Current.CacheDirectory,
                "DocumentPreviews");

        Directory.CreateDirectory(directory);

        string previewPath =
            Path.Combine(
                directory,
                $"document_{document.Id}.png");

        if (File.Exists(previewPath))
            return previewPath;

        await Task.Run(() =>
        {
            using var pdf =
                File.OpenRead(sourcePath);

            // La plataforma ya fue validada arriba.
#pragma warning disable CA1416
            Conversion.SavePng(
                imageFilename: previewPath,
                pdfStream: pdf,
                page: 0);
#pragma warning restore CA1416
        });

        return previewPath;
    }
}

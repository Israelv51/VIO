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

#if ANDROID
        // PDFtoImage requiere Android 12 / API 31+
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return null;
        }
#endif

#if IOS
        // Evitamos depender del renderer nativo en iOS
        // durante VIO v1.
        return null;
#else

        string directory =
            Path.Combine(
                FileSystem.Current.CacheDirectory,
                "DocumentPreviews");

        Directory.CreateDirectory(
            directory);

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

            Conversion.SavePng(
                imageFilename: previewPath,
                pdfStream: pdf,
                page: 0);
        });

        return previewPath;

#endif
    }
}

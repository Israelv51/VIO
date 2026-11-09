using Microsoft.Maui.Storage;
using SkiaSharp;
using VIO.App.Models;

namespace VIO.App.Services;

public partial class PdfDocumentService
{
    public Task<VioDocument> CreateFromImageFilesAsync(
        int userId,
        IReadOnlyList<string> imagePaths,
        string displayName)
    {
        string directory =
            Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "VioDocuments",
                userId.ToString());

        Directory.CreateDirectory(
            directory);

        string storedFileName =
            $"{Guid.NewGuid():N}.pdf";

        string pdfPath =
            Path.Combine(
                directory,
                storedFileName);

        int pagesCreated = 0;

        using (var document =
               SKDocument.CreatePdf(pdfPath))
        {
            foreach (string imagePath in imagePaths)
            {
                if (!File.Exists(imagePath))
                    continue;

                using var stream =
                    File.OpenRead(imagePath);

                using var bitmap =
                    SKBitmap.Decode(stream);

                if (bitmap is null)
                    continue;

                var canvas =
                    document.BeginPage(
                        PageWidth,
                        PageHeight);

                canvas.Clear(
                    SKColors.White);

                float availableWidth =
                    PageWidth - (Margin * 2);

                float availableHeight =
                    PageHeight - (Margin * 2);

                float widthScale =
                    availableWidth /
                    bitmap.Width;

                float heightScale =
                    availableHeight /
                    bitmap.Height;

                float scale =
                    Math.Min(
                        widthScale,
                        heightScale);

                float drawWidth =
                    bitmap.Width * scale;

                float drawHeight =
                    bitmap.Height * scale;

                float x =
                    (PageWidth - drawWidth) / 2;

                float y =
                    (PageHeight - drawHeight) / 2;

                var destination =
                    new SKRect(
                        x,
                        y,
                        x + drawWidth,
                        y + drawHeight);

                var sampling =
                    new SKSamplingOptions(
                        SKFilterMode.Linear);

                canvas.DrawBitmap(
                    bitmap,
                    destination,
                    sampling);

                document.EndPage();

                pagesCreated++;
            }

            document.Close();
        }

        if (pagesCreated == 0)
        {
            if (File.Exists(pdfPath))
                File.Delete(pdfPath);

            throw new InvalidOperationException(
                "VIO no pudo procesar las imágenes.");
        }

        var info =
            new FileInfo(pdfPath);

        var result =
            new VioDocument
            {
                UserId = userId,

                DisplayName =
                    displayName,

                OriginalFileName =
                    $"{displayName}.pdf",

                StoredFileName =
                    storedFileName,

                Extension =
                    ".pdf",

                Category =
                    "Documento oficial",

                SizeBytes =
                    info.Length,

                CreatedAt =
                    DateTime.UtcNow
            };

        return Task.FromResult(result);
    }
}


using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using SkiaSharp;
using VIO.App.Models;

namespace VIO.App.Services;

public partial class PdfDocumentService
{
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float Margin = 28f;

    public async Task<VioDocument?> CreateFromPhotosAsync(
        int userId)
    {
        var photos =
            await MediaPicker.PickPhotosAsync(
                new MediaPickerOptions
                {
                    Title = "Selecciona las fotos del documento",
                    SelectionLimit = 10,
                    MaximumWidth = 2400,
                    MaximumHeight = 2400,
                    CompressionQuality = 90,
                    RotateImage = true,
                    PreserveMetaData = false
                });

        if (photos.Count == 0)
            return null;

        return await CreatePdfAsync(
            userId,
            photos,
            $"Documento {DateTime.Now:dd-MM-yyyy HH-mm}");
    }

    public async Task<VioDocument?> CaptureTwoSidedAsync(
        int userId)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            throw new InvalidOperationException(
                "La cámara no está disponible en este dispositivo.");
        }

        var front =
            await MediaPicker.Default.CapturePhotoAsync(
                new MediaPickerOptions
                {
                    Title = "Fotografía del frente",
                    MaximumWidth = 2400,
                    MaximumHeight = 2400,
                    CompressionQuality = 90,
                    RotateImage = true,
                    PreserveMetaData = false,
                });

        if (front is null)
            return null;

        var back =
            await MediaPicker.Default.CapturePhotoAsync(
                new MediaPickerOptions
                {
                    Title = "Fotografía del reverso",
                    MaximumWidth = 2400,
                    MaximumHeight = 2400,
                    CompressionQuality = 90,
                    RotateImage = true,
                    PreserveMetaData = false,
                });

        if (back is null)
            return null;

        return await CreatePdfAsync(
            userId,
            new List<FileResult>
            {
                front,
                back
            },
            $"Documento doble cara {DateTime.Now:dd-MM-yyyy HH-mm}");
    }

    private async Task<VioDocument> CreatePdfAsync(
        int userId,
        IReadOnlyList<FileResult> photos,
        string displayName)
    {
        string directory =
            Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "VioDocuments",
                userId.ToString());

        Directory.CreateDirectory(directory);

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
            foreach (var photo in photos)
            {
                await using var stream =
                    await photo.OpenReadAsync();

                using var bitmap =
                    SKBitmap.Decode(stream);

                if (bitmap is null)
                    continue;

                var canvas =
                    document.BeginPage(
                        PageWidth,
                        PageHeight);

                canvas.Clear(SKColors.White);

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
                "No fue posible procesar las imágenes.");
        }

        var info =
            new FileInfo(pdfPath);

        return new VioDocument
        {
            UserId = userId,
            DisplayName = displayName,
            OriginalFileName =
                $"{displayName}.pdf",
            StoredFileName = storedFileName,
            Extension = ".pdf",
            Category = "Documento",
            SizeBytes = info.Length,
            CreatedAt = DateTime.UtcNow
        };
    }
}



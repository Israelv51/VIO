using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class ProfilePhotoEditorViewModel :
    ObservableObject
{
    private readonly ISessionService _session;
    private readonly VioDatabase _database;

    private int _userId;

    public ProfilePhotoEditorViewModel(
        ISessionService session,
        VioDatabase database)
    {
        _session = session;
        _database = database;
    }

    [ObservableProperty]
    public partial string SourcePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Zoom { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewX))]
    public partial double OffsetX { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PreviewY))]
    public partial double OffsetY { get; set; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    public double PreviewX =>
        OffsetX * 0.7;

    public double PreviewY =>
        OffsetY * 0.7;

    public async Task LoadAsync()
    {
        var user =
            await _session.GetCurrentUserAsync();

        if (user is null)
            return;

        _userId =
            user.Id;

        string directory =
            Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "ProfilePhotos");

        string original =
            Path.Combine(
                directory,
                $"profile_{user.Id}_original.jpg");

        SourcePath =
            File.Exists(original)
                ? original
                : user.ProfilePhotoPath ?? string.Empty;
    }

    [RelayCommand]
    private void Reset()
    {
        Zoom = 1;
        OffsetX = 0;
        OffsetY = 0;

        Message =
            "Encuadre restablecido.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsSaving ||
            string.IsNullOrWhiteSpace(SourcePath) ||
            !File.Exists(SourcePath))
        {
            return;
        }

        try
        {
            IsSaving = true;

            var user =
                await _session.GetCurrentUserAsync();

            if (user is null)
                return;

            using var stream =
                File.OpenRead(SourcePath);

            using var bitmap =
                SKBitmap.Decode(stream);

            if (bitmap is null)
            {
                Message =
                    "No fue posible procesar la imagen.";

                return;
            }

            float zoom =
                (float)Math.Clamp(
                    Zoom,
                    1,
                    3);

            float baseSize =
                Math.Min(
                    bitmap.Width,
                    bitmap.Height);

            float cropSize =
                baseSize / zoom;

            float normalizedX =
                (float)Math.Clamp(
                    OffsetX / 100.0,
                    -1,
                    1);

            float normalizedY =
                (float)Math.Clamp(
                    OffsetY / 100.0,
                    -1,
                    1);

            float maxShiftX =
                Math.Max(
                    0,
                    (bitmap.Width - cropSize) / 2);

            float maxShiftY =
                Math.Max(
                    0,
                    (bitmap.Height - cropSize) / 2);

            float centerX =
                bitmap.Width / 2f +
                normalizedX * maxShiftX;

            float centerY =
                bitmap.Height / 2f +
                normalizedY * maxShiftY;

            float left =
                centerX - cropSize / 2;

            float top =
                centerY - cropSize / 2;

            left =
                Math.Clamp(
                    left,
                    0,
                    bitmap.Width - cropSize);

            top =
                Math.Clamp(
                    top,
                    0,
                    bitmap.Height - cropSize);

            var sourceRect =
                new SKRect(
                    left,
                    top,
                    left + cropSize,
                    top + cropSize);

            const int targetSize =
                1000;

            using var surface =
                SKSurface.Create(
                    new SKImageInfo(
                        targetSize,
                        targetSize));

            var canvas =
                surface.Canvas;

            canvas.Clear(
                SKColors.White);

            var targetRect =
                new SKRect(
                    0,
                    0,
                    targetSize,
                    targetSize);

            var sampling =
                new SKSamplingOptions(
                    SKFilterMode.Linear);

            canvas.DrawBitmap(
                bitmap,
                sourceRect,
                targetRect,
                sampling);

            using var image =
                surface.Snapshot();

            using var encoded =
                image.Encode(
                    SKEncodedImageFormat.Jpeg,
                    95);

            string destination =
                Path.Combine(
                    FileSystem.Current.AppDataDirectory,
                    "ProfilePhotos",
                    $"profile_{_userId}.jpg");

            await using var output =
                File.Create(destination);

            encoded.SaveTo(output);

            user.ProfilePhotoPath =
                destination;

            await _database.UpdateUserAsync(user);

            Message =
                "Foto ajustada correctamente.";

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }
}

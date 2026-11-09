using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using SkiaSharp;
using VIO.App.Constants;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;
    private readonly VioDatabase _database;

    public ProfileViewModel(
        ISessionService sessionService,
        VioDatabase database)
    {
        _sessionService = sessionService;
        _database = database;
    }

    [ObservableProperty]
    public partial string Name { get; set; } = "Usuario";

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProfilePhotoPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhotoMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasPhoto { get; set; }

    public async Task LoadAsync()
    {
        var user =
            await _sessionService.GetCurrentUserAsync();

        if (user is null)
        {
            _sessionService.End();

            await Shell.Current.GoToAsync(
                AppRoutes.Login);

            return;
        }

        Name = user.Name;
        Email = user.Email;

        ProfilePhotoPath =
            user.ProfilePhotoPath ?? string.Empty;

        HasPhoto =
            !string.IsNullOrWhiteSpace(ProfilePhotoPath) &&
            File.Exists(ProfilePhotoPath);
    }

    [RelayCommand]
    private async Task SelectPhotoAsync()
    {
        var user =
            await _sessionService.GetCurrentUserAsync();

        if (user is null)
            return;

        try
        {
            var photos =
                await MediaPicker.PickPhotosAsync(
                    new MediaPickerOptions
                    {
                        Title = "Selecciona tu foto",
                        SelectionLimit = 1,
                        MaximumWidth = 2400,
                        MaximumHeight = 2400,
                        CompressionQuality = 95,
                        RotateImage = true,
                        PreserveMetaData = false
                    });

            var photo =
                photos.FirstOrDefault();

            if (photo is null)
                return;

            string directory =
                Path.Combine(
                    FileSystem.Current.AppDataDirectory,
                    "ProfilePhotos");

            Directory.CreateDirectory(directory);

            string originalPath =
                Path.Combine(
                    directory,
                    $"profile_{user.Id}_original.jpg");

            string finalPath =
                Path.Combine(
                    directory,
                    $"profile_{user.Id}.jpg");

            await using var source =
                await photo.OpenReadAsync();

            using var bitmap =
                SKBitmap.Decode(source);

            if (bitmap is null)
            {
                PhotoMessage =
                    "No fue posible procesar la foto.";

                return;
            }

            using (var image =
                   SKImage.FromBitmap(bitmap))
            using (var encoded =
                   image.Encode(
                       SKEncodedImageFormat.Jpeg,
                       95))
            await using (var output =
                         File.Create(originalPath))
            {
                encoded.SaveTo(output);
            }

            CreateSquareCrop(
                bitmap,
                finalPath);

            user.ProfilePhotoPath =
                finalPath;

            await _database.UpdateUserAsync(user);

            ProfilePhotoPath =
                finalPath;

            HasPhoto = true;

            PhotoMessage =
                "Foto cargada. Ahora puedes ajustarla.";

            await Shell.Current.GoToAsync(
                AppRoutes.ProfilePhotoEditor);
        }
        catch (Exception ex)
        {
            PhotoMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task AdjustPhotoAsync()
    {
        if (!HasPhoto)
        {
            PhotoMessage =
                "Primero selecciona una foto.";

            return;
        }

        await Shell.Current.GoToAsync(
            AppRoutes.ProfilePhotoEditor);
    }

    [RelayCommand]
    private async Task RemovePhotoAsync()
    {
        var user =
            await _sessionService.GetCurrentUserAsync();

        if (user is null)
            return;

        string directory =
            Path.Combine(
                FileSystem.Current.AppDataDirectory,
                "ProfilePhotos");

        string originalPath =
            Path.Combine(
                directory,
                $"profile_{user.Id}_original.jpg");

        if (File.Exists(originalPath))
            File.Delete(originalPath);

        if (!string.IsNullOrWhiteSpace(
                user.ProfilePhotoPath) &&
            File.Exists(user.ProfilePhotoPath))
        {
            File.Delete(user.ProfilePhotoPath);
        }

        user.ProfilePhotoPath = null;

        await _database.UpdateUserAsync(user);

        ProfilePhotoPath = string.Empty;
        HasPhoto = false;

        PhotoMessage =
            "Foto eliminada.";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _sessionService.End();

        await Shell.Current.GoToAsync(
            AppRoutes.Login);
    }

    private static void CreateSquareCrop(
        SKBitmap bitmap,
        string destination)
    {
        int cropSize =
            Math.Min(
                bitmap.Width,
                bitmap.Height);

        int x =
            (bitmap.Width - cropSize) / 2;

        int y =
            (bitmap.Height - cropSize) / 2;

        var sourceRect =
            new SKRect(
                x,
                y,
                x + cropSize,
                y + cropSize);

        const int targetSize = 1000;

        using var surface =
            SKSurface.Create(
                new SKImageInfo(
                    targetSize,
                    targetSize));

        var canvas =
            surface.Canvas;

        canvas.Clear(SKColors.White);

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
                94);

        using var file =
            File.Create(destination);

        encoded.SaveTo(file);
    }
}

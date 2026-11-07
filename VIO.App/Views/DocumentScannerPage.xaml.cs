using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using VIO.App.Interfaces;
using VIO.App.Services;

namespace VIO.App.Views;

public partial class DocumentScannerPage : ContentPage
{
    private readonly ISessionService _session;
    private readonly PdfDocumentService _pdfService;
    private readonly VioDatabase _database;

    private string? _frontImagePath;
    private string? _backImagePath;

    private int _step = 1;
    private bool _isCapturing;

    public DocumentScannerPage(
        ISessionService session,
        PdfDocumentService pdfService,
        VioDatabase database)
    {
        InitializeComponent();

        _session = session;
        _pdfService = pdfService;
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await StartCameraAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            Camera.IsTorchOn = false;
            Camera.StopCameraPreview();
        }
        catch
        {
        }
    }

    private async Task StartCameraAsync()
    {
        var permission =
            await Permissions.RequestAsync<
                Permissions.Camera>();

        if (permission != PermissionStatus.Granted)
        {
            TitleLabel.Text =
                "Permiso de cámara";

            InstructionLabel.Text =
                "VIO necesita acceso a la cámara para escanear documentos.";

            CaptureButton.IsEnabled = false;

            return;
        }

        try
        {
            using var timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(8));

            var cameras =
                await Camera.GetAvailableCameras(
                    timeout.Token);

            Camera.SelectedCamera =
                cameras.FirstOrDefault(
                    camera =>
                        camera.Position ==
                        CameraPosition.Rear)
                ?? cameras.FirstOrDefault();

            if (Camera.SelectedCamera is null)
            {
                TitleLabel.Text =
                    "Cámara no disponible";

                InstructionLabel.Text =
                    "No se encontró una cámara compatible.";

                CaptureButton.IsEnabled = false;

                return;
            }

            await Camera.StartCameraPreview(
                timeout.Token);

            FlashButton.IsEnabled =
                Camera.SelectedCamera.IsFlashSupported;

            if (!FlashButton.IsEnabled)
            {
                FlashButton.Opacity = 0.35;
            }
        }
        catch (Exception ex)
        {
            TitleLabel.Text =
                "No pudimos iniciar la cámara";

            InstructionLabel.Text =
                ex.Message;

            CaptureButton.IsEnabled = false;
        }
    }

    private async void OnCaptureClicked(
        object? sender,
        EventArgs e)
    {
        if (_isCapturing)
            return;

        if (_session.CurrentUserId is not int userId)
            return;

        try
        {
            _isCapturing = true;
            CaptureButton.IsEnabled = false;

            using var timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(10));

            using var capturedStream =
                await Camera.CaptureImage(
                    timeout.Token);

            string imagePath =
                Path.Combine(
                    FileSystem.Current.CacheDirectory,
                    $"vio-scan-{Guid.NewGuid():N}.jpg");

            await using (
                var output =
                    File.Create(imagePath))
            {
                await capturedStream.CopyToAsync(
                    output);
            }

            HapticFeedback.Default.Perform(
                HapticFeedbackType.Click);

            await AnimateCaptureAsync();

            if (_step == 1)
            {
                DeleteFileIfExists(
                    _frontImagePath);

                _frontImagePath =
                    imagePath;

                await MoveToBackSideAsync();

                CaptureButton.IsEnabled =
                    true;

                return;
            }

            DeleteFileIfExists(
                _backImagePath);

            _backImagePath =
                imagePath;

            ProcessingOverlay.IsVisible =
                true;

            Camera.IsTorchOn =
                false;

            var pdf =
                await _pdfService
                    .CreateFromImageFilesAsync(
                        userId,
                        new[]
                        {
                            _frontImagePath!,
                            _backImagePath!
                        },
                        $"Documento escaneado {DateTime.Now:dd-MM-yyyy HH-mm}");

            await _database.AddDocumentAsync(
                pdf);

            CleanupTemporaryImages();

            HapticFeedback.Default.Perform(
                HapticFeedbackType.Click);

            Camera.StopCameraPreview();

            await Shell.Current.GoToAsync(
                "..");
        }
        catch (OperationCanceledException)
        {
            InstructionLabel.Text =
                "La captura tardó demasiado. Intenta nuevamente.";

            CaptureButton.IsEnabled =
                true;
        }
        catch (Exception ex)
        {
            ProcessingOverlay.IsVisible =
                false;

            TitleLabel.Text =
                "No pudimos completar el escaneo";

            InstructionLabel.Text =
                ex.Message;

            CaptureButton.IsEnabled =
                true;
        }
        finally
        {
            _isCapturing =
                false;
        }
    }

    private async Task AnimateCaptureAsync()
    {
        FlashOverlay.Opacity = 0;

        await FlashOverlay.FadeToAsync(
            0.82,
            60);

        await FlashOverlay.FadeToAsync(
            0,
            140);
    }

    private async Task MoveToBackSideAsync()
    {
        _step = 2;

        FlipCard.IsVisible =
            true;

        FlipCard.Opacity =
            0;

        await FlipCard.FadeToAsync(
            1,
            180,
            Easing.CubicOut);

        await Task.Delay(
            850);

        await FlipCard.FadeToAsync(
            0,
            180,
            Easing.CubicIn);

        FlipCard.IsVisible =
            false;

        StepLabel.Text =
            "REVERSO · 2 DE 2";

        FrontDot.WidthRequest =
            7;

        FrontDot.BackgroundColor =
            Color.FromArgb("#34C759");

        BackDot.WidthRequest =
            22;

        BackDot.BackgroundColor =
            Color.FromArgb("#0A84FF");

        SideLabel.Text =
            "REVERSO";

        TitleLabel.Text =
            "Escanea el reverso";

        InstructionLabel.Text =
            "Voltea el documento y vuelve a colocarlo dentro del marco.";

        ResetButton.IsVisible =
            true;
    }

    private void OnFlashClicked(
        object? sender,
        EventArgs e)
    {
        if (Camera.SelectedCamera?.IsFlashSupported != true)
            return;

        Camera.IsTorchOn =
            !Camera.IsTorchOn;

        FlashButton.BackgroundColor =
            Camera.IsTorchOn
                ? Color.FromArgb("#0A84FF")
                : Color.FromArgb("#551D1D1F");

        HapticFeedback.Default.Perform(
            HapticFeedbackType.Click);
    }

    private void OnResetClicked(
        object? sender,
        EventArgs e)
    {
        CleanupTemporaryImages();

        _step = 1;

        StepLabel.Text =
            "FRENTE · 1 DE 2";

        FrontDot.WidthRequest =
            22;

        FrontDot.BackgroundColor =
            Color.FromArgb("#0A84FF");

        BackDot.WidthRequest =
            7;

        BackDot.BackgroundColor =
            Color.FromArgb("#6E6E73");

        SideLabel.Text =
            "FRENTE";

        TitleLabel.Text =
            "Escanea el frente";

        InstructionLabel.Text =
            "Coloca todo el documento dentro del marco y evita reflejos.";

        ResetButton.IsVisible =
            false;

        CaptureButton.IsEnabled =
            true;

        HapticFeedback.Default.Perform(
            HapticFeedbackType.Click);
    }

    private async void OnCloseClicked(
        object? sender,
        EventArgs e)
    {
        CleanupTemporaryImages();

        try
        {
            Camera.IsTorchOn = false;
            Camera.StopCameraPreview();
        }
        catch
        {
        }

        await Shell.Current.GoToAsync(
            "..");
    }

    private void CleanupTemporaryImages()
    {
        DeleteFileIfExists(
            _frontImagePath);

        DeleteFileIfExists(
            _backImagePath);

        _frontImagePath =
            null;

        _backImagePath =
            null;
    }

    private static void DeleteFileIfExists(
        string? path)
    {
        if (!string.IsNullOrWhiteSpace(path) &&
            File.Exists(path))
        {
            File.Delete(path);
        }
    }
}

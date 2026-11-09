using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using VIO.App.Interfaces;
using VIO.App.Constants;
using VIO.App.Services;

namespace VIO.App.Views;

public partial class DocumentScannerPage :
    ContentPage,
    IQueryAttributable
{
    private readonly ISessionService _session;
    private readonly PdfDocumentService _pdfService;
    private readonly VioDatabase _database;

    private readonly List<string> _pages = [];

    private string _mode = "double";
    private int _doubleSideStep = 1;

    private bool _isCapturing;
    private bool _cameraStarted;

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

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue(
                "mode",
                out var value))
        {
            _mode =
                value?.ToString()?.ToLowerInvariant()
                ?? "double";
        }

        Dispatcher.Dispatch(
            ConfigureMode);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ConfigureMode();

        if (!_cameraStarted)
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

    private void ConfigureMode()
    {
        if (_mode == "multi")
        {
            StepLabel.Text =
                "PÁGINA 1";

            SideLabel.Text =
                "PÁGINA 1";

            TitleLabel.Text =
                "Escanea tu documento";

            InstructionLabel.Text =
                "Coloca la primera página dentro del marco.";

            FrontDot.WidthRequest =
                22;

            FrontDot.BackgroundColor =
                Color.FromArgb("#0A84FF");

            BackDot.IsVisible =
                false;

            FinishButton.IsVisible =
                false;

            ResetButton.IsVisible =
                false;
        }
        else
        {
            StepLabel.Text =
                "FRENTE · 1 DE 2";

            SideLabel.Text =
                "FRENTE";

            TitleLabel.Text =
                "Escanea el frente";

            InstructionLabel.Text =
                "Coloca todo el documento dentro del marco y evita reflejos.";

            FrontDot.WidthRequest =
                22;

            FrontDot.BackgroundColor =
                Color.FromArgb("#0A84FF");

            BackDot.IsVisible =
                true;

            BackDot.WidthRequest =
                7;

            BackDot.BackgroundColor =
                Color.FromArgb("#6E6E73");

            FinishButton.IsVisible =
                false;

            ResetButton.IsVisible =
                false;
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
                "VIO necesita acceso a la cámara.";

            CaptureButton.IsEnabled =
                false;

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

                CaptureButton.IsEnabled =
                    false;

                return;
            }

            await Camera.StartCameraPreview(
                timeout.Token);

            _cameraStarted =
                true;

            FlashButton.IsEnabled =
                Camera.SelectedCamera.IsFlashSupported;

            if (!FlashButton.IsEnabled)
                FlashButton.Opacity = 0.35;
        }
        catch (Exception ex)
        {
            TitleLabel.Text =
                "No pudimos iniciar la cámara";

            InstructionLabel.Text =
                ex.Message;

            CaptureButton.IsEnabled =
                false;
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
            _isCapturing =
                true;

            CaptureButton.IsEnabled =
                false;

            string imagePath =
                await CaptureTemporaryImageAsync();

            _pages.Add(
                imagePath);

            HapticFeedback.Default.Perform(
                HapticFeedbackType.Click);

            await AnimateCaptureAsync();

            if (_mode == "multi")
            {
                HandleMultiPageCapture();

                CaptureButton.IsEnabled =
                    true;

                return;
            }

            if (_doubleSideStep == 1)
            {
                await MoveToBackSideAsync();

                CaptureButton.IsEnabled =
                    true;

                return;
            }

            await FinishDocumentAsync(
                userId,
                $"Documento doble cara {DateTime.Now:dd-MM-yyyy HH-mm}");
        }
        catch (OperationCanceledException)
        {
            InstructionLabel.Text =
                "La captura tardó demasiado.";

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

    private async Task<string> CaptureTemporaryImageAsync()
    {
        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        using var capturedStream =
            await Camera.CaptureImage(
                timeout.Token);

        string path =
            Path.Combine(
                FileSystem.Current.CacheDirectory,
                $"vio-scan-{Guid.NewGuid():N}.jpg");

        await using var output =
            File.Create(path);

        await capturedStream.CopyToAsync(
            output);

        return path;
    }

    private void HandleMultiPageCapture()
    {
        int captured =
            _pages.Count;

        int next =
            captured + 1;

        StepLabel.Text =
            captured == 1
                ? "1 PÁGINA CAPTURADA"
                : $"{captured} PÁGINAS CAPTURADAS";

        SideLabel.Text =
            $"PÁGINA {next}";

        TitleLabel.Text =
            $"Página {captured} lista";

        InstructionLabel.Text =
            "Coloca la siguiente página y captura otra foto, o toca Finalizar PDF.";

        FinishButton.IsVisible =
            true;

        ResetButton.IsVisible =
            true;
    }

    private async Task MoveToBackSideAsync()
    {
        _doubleSideStep =
            2;

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

    private async void OnFinishClicked(
        object? sender,
        EventArgs e)
    {
        if (_session.CurrentUserId is not int userId)
            return;

        if (_pages.Count == 0)
            return;

        await FinishDocumentAsync(
            userId,
            $"Documento escaneado {DateTime.Now:dd-MM-yyyy HH-mm}");
    }

    private async Task FinishDocumentAsync(
        int userId,
        string name)
    {
        ProcessingOverlay.IsVisible =
            true;

        CaptureButton.IsEnabled =
            false;

        FinishButton.IsEnabled =
            false;

        Camera.IsTorchOn =
            false;

        var pdf =
            await _pdfService.CreateFromImageFilesAsync(
                userId,
                _pages,
                name);

        await _database.AddDocumentAsync(
            pdf);

        CleanupTemporaryImages();

        HapticFeedback.Default.Perform(
            HapticFeedbackType.Click);

        Camera.StopCameraPreview();

        await Shell.Current.GoToAsync(
            "..");

        await Shell.Current.GoToAsync(
            $"{AppRoutes.DocumentDetails}?id={pdf.Id}");
    }

    private async Task AnimateCaptureAsync()
    {
        FlashOverlay.Opacity =
            0;

        await FlashOverlay.FadeToAsync(
            0.82,
            60);

        await FlashOverlay.FadeToAsync(
            0,
            140);
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

        _doubleSideStep =
            1;

        ProcessingOverlay.IsVisible =
            false;

        FinishButton.IsEnabled =
            true;

        CaptureButton.IsEnabled =
            true;

        ConfigureMode();

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
            Camera.IsTorchOn =
                false;

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
        foreach (string path in _pages)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        _pages.Clear();
    }
}


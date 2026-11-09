using VIO.App.ViewModels;

namespace VIO.App.Views;

public partial class ProfilePhotoEditorPage : ContentPage
{
    private readonly ProfilePhotoEditorViewModel _viewModel;

    private double _panStartX;
    private double _panStartY;

    private double _pinchStartZoom = 1;

    public ProfilePhotoEditorPage(
        ProfilePhotoEditorViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }

    private void OnPanUpdated(
        object? sender,
        PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:

                _panStartX =
                    _viewModel.OffsetX;

                _panStartY =
                    _viewModel.OffsetY;

                break;

            case GestureStatus.Running:

                _viewModel.OffsetX =
                    Math.Clamp(
                        _panStartX +
                        e.TotalX / 2,
                        -100,
                        100);

                _viewModel.OffsetY =
                    Math.Clamp(
                        _panStartY +
                        e.TotalY / 2,
                        -100,
                        100);

                break;
        }
    }

    private void OnPinchUpdated(
        object? sender,
        PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _pinchStartZoom =
                _viewModel.Zoom;
        }

        if (e.Status == GestureStatus.Running)
        {
            _viewModel.Zoom =
                Math.Clamp(
                    _pinchStartZoom *
                    e.Scale,
                    1,
                    3);
        }
    }

    private void OnMoveUpClicked(
        object? sender,
        EventArgs e)
    {
        _viewModel.OffsetY =
            Math.Max(
                -100,
                _viewModel.OffsetY - 10);
    }

    private void OnMoveDownClicked(
        object? sender,
        EventArgs e)
    {
        _viewModel.OffsetY =
            Math.Min(
                100,
                _viewModel.OffsetY + 10);
    }

    private void OnMoveLeftClicked(
        object? sender,
        EventArgs e)
    {
        _viewModel.OffsetX =
            Math.Max(
                -100,
                _viewModel.OffsetX - 10);
    }

    private void OnMoveRightClicked(
        object? sender,
        EventArgs e)
    {
        _viewModel.OffsetX =
            Math.Min(
                100,
                _viewModel.OffsetX + 10);
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

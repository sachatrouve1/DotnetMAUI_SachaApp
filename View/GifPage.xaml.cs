using System;
using System.Threading;
using System.Threading.Tasks;

namespace SachaApp.View;

public partial class GifPage : ContentPage
{
    // Approximate one GIF cycle before enabling manual return.
    private const int GifPlaybackDurationMs = 5000;

    private CancellationTokenSource? _playbackCts;

    public GifPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Force restart to avoid platforms showing only the first frame.
        GifImage.IsAnimationPlaying = false;
        GifImage.IsAnimationPlaying = true;

        _playbackCts?.Cancel();
        _playbackCts = new CancellationTokenSource();

        BackButton.IsEnabled = false;
        BackButton.Text = "Playing...";

        try
        {
            await Task.Delay(GifPlaybackDurationMs, _playbackCts.Token);

            BackButton.Text = "Back to Main Page";
            BackButton.IsEnabled = true;
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation when leaving the page.
        }
    }

    protected override void OnDisappearing()
    {
        _playbackCts?.Cancel();
        base.OnDisappearing();
    }

    private async void OnBackButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}


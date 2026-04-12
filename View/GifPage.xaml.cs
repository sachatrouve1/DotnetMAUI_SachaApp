using System;
using System.Threading;
using System.Threading.Tasks;

namespace SachaApp.View;

public partial class GifPage : ContentPage
{
    private const int FallbackGifPlaybackDurationMs = 5000;
    private const int GifMinimumDurationMs = 500;

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
            var gifDurationMs = await GetGifDurationMsAsync(_playbackCts.Token);
            await Task.Delay(gifDurationMs, _playbackCts.Token);

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
        await Shell.Current.GoToAsync("//MainPage");
    }

    private static async Task<int> GetGifDurationMsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("beer_time.gif");
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            var durationMs = TryParseGifDurationMs(memory.ToArray());

            if (durationMs <= 0)
            {
                return FallbackGifPlaybackDurationMs;
            }

            return Math.Max(durationMs, GifMinimumDurationMs);
        }
        catch
        {
            return FallbackGifPlaybackDurationMs;
        }
    }

    private static int TryParseGifDurationMs(byte[] data)
    {
        if (data.Length < 13)
        {
            return 0;
        }

        // Header + logical screen descriptor
        var i = 6;
        i += 7;

        // Global color table
        var packed = data[10];
        var hasGlobalColorTable = (packed & 0x80) != 0;
        if (hasGlobalColorTable)
        {
            var gctSize = 3 * (1 << ((packed & 0x07) + 1));
            i += gctSize;
        }

        var totalDelayCentiseconds = 0;
        var pendingDelayCentiseconds = 10; // Common default when no GCE is present

        while (i < data.Length)
        {
            var blockId = data[i++];

            switch (blockId)
            {
                case 0x21: // Extension block
                    if (i >= data.Length)
                    {
                        return 0;
                    }

                    var extensionLabel = data[i++];
                    if (extensionLabel == 0xF9)
                    {
                        // Graphics Control Extension: 21 F9 04 [packed] [delay_lo] [delay_hi] [transparency] 00
                        if (i + 5 >= data.Length)
                        {
                            return 0;
                        }

                        var blockSize = data[i++];
                        if (blockSize != 4)
                        {
                            return 0;
                        }

                        i++; // packed fields
                        var delay = data[i++] | (data[i++] << 8);
                        pendingDelayCentiseconds = Math.Max(delay, 1);
                        i++; // transparency index

                        if (i >= data.Length || data[i++] != 0x00)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        i = SkipSubBlocks(data, i);
                        if (i < 0)
                        {
                            return 0;
                        }
                    }

                    break;

                case 0x2C: // Image descriptor (a frame)
                    if (i + 9 > data.Length)
                    {
                        return 0;
                    }

                    // Skip image descriptor fields
                    i += 8;
                    var imagePacked = data[i++];

                    // Local color table
                    var hasLocalColorTable = (imagePacked & 0x80) != 0;
                    if (hasLocalColorTable)
                    {
                        var lctSize = 3 * (1 << ((imagePacked & 0x07) + 1));
                        i += lctSize;
                        if (i > data.Length)
                        {
                            return 0;
                        }
                    }

                    // LZW minimum code size
                    if (i >= data.Length)
                    {
                        return 0;
                    }

                    i++;

                    // Image data sub-blocks
                    i = SkipSubBlocks(data, i);
                    if (i < 0)
                    {
                        return 0;
                    }

                    totalDelayCentiseconds += pendingDelayCentiseconds;
                    pendingDelayCentiseconds = 10;
                    break;

                case 0x3B: // Trailer
                    return totalDelayCentiseconds * 10;

                default:
                    return 0;
            }
        }

        return totalDelayCentiseconds * 10;
    }

    private static int SkipSubBlocks(byte[] data, int index)
    {
        var i = index;
        while (i < data.Length)
        {
            var size = data[i++];
            if (size == 0)
            {
                return i;
            }

            i += size;
            if (i > data.Length)
            {
                return -1;
            }
        }

        return -1;
    }
}


using System;
using System.Threading;
using System.Threading.Tasks;

namespace SachaApp;

public sealed class BubbleAnimator
{
    private readonly VisualElement[] _bubbles;
    private readonly Random _random = new();

    private CancellationTokenSource? _cts;

    public BubbleAnimator(params VisualElement[] bubbles)
    {
        _bubbles = bubbles;
    }

    public void Start(double width, double height)
    {
        Stop();
        _cts = new CancellationTokenSource();

        foreach (var bubble in _bubbles)
        {
            _ = AnimateBubbleAsync(bubble, width, height, _cts.Token);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task AnimateBubbleAsync(VisualElement bubble, double width, double height, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var startX = _random.NextDouble() * Math.Max(1, width - bubble.WidthRequest);
            var startY = height + _random.Next(20, 120);
            var endY = -_random.Next(40, 160);
            var duration = (uint)_random.Next(2600, 5200);

            bubble.TranslationX = startX;
            bubble.TranslationY = startY;
            bubble.Opacity = 0.8;

            try
            {
                await Task.WhenAll(
                    bubble.TranslateTo(startX + _random.Next(-12, 12), endY, duration, Easing.SinOut),
                    bubble.FadeTo(0.1, duration));

                await Task.Delay(_random.Next(150, 700), token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }
}


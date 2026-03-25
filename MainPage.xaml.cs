using System;
using System.Threading.Tasks;

namespace SachaApp;

public partial class MainPage : ContentPage
{
    private BubbleAnimator? _bubbleAnimator;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _bubbleAnimator ??= new BubbleAnimator(Bubble1, Bubble2, Bubble3, Bubble4, Bubble5, Bubble6);
        await WaitForBubbleLayerAsync();
        _bubbleAnimator.Start(BubbleLayer.Width, BubbleLayer.Height);
    }

    protected override void OnDisappearing()
    {
        _bubbleAnimator?.Stop();
        base.OnDisappearing();
    }

    private async void NavigateToGif_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.GifPageRoute);
    }

    private async Task WaitForBubbleLayerAsync()
    {
        var tries = 0;
        while ((BubbleLayer.Width <= 0 || BubbleLayer.Height <= 0) && tries < 20)
        {
            tries++;
            await Task.Delay(50);
        }
    }
}
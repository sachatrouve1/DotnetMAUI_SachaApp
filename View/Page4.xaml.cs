using System.Threading.Tasks;
using SachaApp.Animations;

namespace SachaApp.View;

public partial class Page4 : ContentPage
{
    private BubbleAnimator? _bubbleAnimator;

    public Page4()
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
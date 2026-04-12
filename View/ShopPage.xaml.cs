using SachaApp.Animations;
using SachaApp.ViewModel;

namespace SachaApp.View;

public partial class ShopPage
{
    private BubbleAnimator? _bubbleAnimator;
    private readonly Page2ViewModel _viewModel = new();

    public ShopPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _bubbleAnimator ??= new BubbleAnimator(Bubble1, Bubble2, Bubble3, Bubble4, Bubble5, Bubble6);
        await WaitForBubbleLayerAsync();
        _bubbleAnimator.Start(BubbleLayer.Width, BubbleLayer.Height);

        await _viewModel.LoadAsync();
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
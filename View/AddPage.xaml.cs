using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SachaApp.Animations;
using SachaApp.Services;

namespace SachaApp.View;

public partial class AddPage : ContentPage
{
    private BubbleAnimator? _bubbleAnimator;
    private readonly BeerCatalogService _beerCatalogService;

    public AddPage()
    {
        InitializeComponent();
        _beerCatalogService = IPlatformApplication.Current?.Services.GetService<BeerCatalogService>() ?? new BeerCatalogService();
    }

    private async void OnAddBeerClicked(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim() ?? string.Empty;
        var description = DescriptionEditor.Text?.Trim() ?? string.Empty;
        var image = ImageEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Required field", "Add a title before submitting.", "OK");
            return;
        }

        await _beerCatalogService.AddManualBeerAsync(title, description, image);

        TitleEntry.Text = string.Empty;
        DescriptionEditor.Text = string.Empty;
        ImageEntry.Text = string.Empty;
        PreviewImage.Source = null;
        PreviewImage.IsVisible = false;

        await Shell.Current.GoToAsync("//ShopPage");
    }

    private void OnImageEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        var imagePath = e.NewTextValue?.Trim();
        var hasImage = !string.IsNullOrWhiteSpace(imagePath);

        PreviewImage.IsVisible = hasImage;
        PreviewImage.Source = hasImage ? imagePath : null;
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
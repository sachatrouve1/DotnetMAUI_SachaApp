using Microsoft.Extensions.Logging;
using SachaApp.ViewModel;
using SachaApp.View;
using SachaApp.Services;

namespace SachaApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<BeerService>();
        builder.Services.AddSingleton<BeerCatalogService>();
        builder.Services.AddSingleton<Page2ViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
using Microsoft.Extensions.Logging;
using TreasureHunter.MauiAndroid.Pages;
using TreasureHunter.MauiAndroid.Services;

namespace TreasureHunter.MauiAndroid;

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

        // Register HttpClient
        builder.Services.AddHttpClient("TreasureHunterAPI", client =>
        {
            // Default base address - can be overridden
            client.BaseAddress = new Uri("https://your-api-url.com");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register Services
        builder.Services.AddSingleton<ITreasureApiService, TreasureApiService>();
        builder.Services.AddSingleton<IGeolocationService, GeolocationService>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

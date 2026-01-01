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

        // Determine base URL based on platform
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5174"  // Android emulator special IP
            : "http://localhost:5174"; // Other platforms

        // Register HttpClient
        builder.Services.AddHttpClient("TreasureHunterAPI", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register MAUI Essentials Services
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

        // Register Services
        builder.Services.AddSingleton<ITreasureApiService, TreasureApiService>();
        builder.Services.AddSingleton<IGeolocationService, GeolocationService>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();

        // Register MAUI Essentials Services
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

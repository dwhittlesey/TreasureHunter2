using System.Runtime.Versioning;
using Camera.MAUI;
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
            .UseMauiCameraView()
            .ConfigureFonts(fonts =>
            {
                // Use fonts that exist in your Resources/Fonts folder
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // For physical device testing - replace with your PC's IP
        var baseUrl = "https://10.0.0.33:7001";  
#else
        var baseUrl = "https://your-production-api.com";
#endif

        // Register HttpClient with better timeout handling
        builder.Services.AddHttpClient("TreasureHunterAPI", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(15); // Reduced from 30 to fail faster
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Disable SSL validation for local development (REMOVE IN PRODUCTION)
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        // Register MAUI Essentials Services
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);

        // Register Services
        builder.Services.AddSingleton<ITreasureApiService, TreasureApiService>();
        builder.Services.AddSingleton<IGeolocationService, GeolocationService>();
        builder.Services.AddSingleton<IARCameraService, ARCameraService>();
        builder.Services.AddSingleton<IARUpdateManager, ARUpdateManager>();

        // Register Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        return builder.Build();
    }
}

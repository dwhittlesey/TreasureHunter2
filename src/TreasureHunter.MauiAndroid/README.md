# TreasureHunter MAUI Android Application

A native Android application built with .NET 10 MAUI that provides GPS-based treasure hunting with AR capabilities.

## Project Overview

- **Framework**: .NET 10 MAUI (net10.0-android)
- **Min Android Version**: API 21 (Android 5.0)
- **Language**: C# with XAML
- **Architecture**: MVVM-inspired with services and dependency injection

## Features

### Authentication
- User registration with email, username, password, and display name
- User login with JWT token-based authentication
- Secure token storage using MAUI Preferences

### GPS Location Services
- Real-time GPS tracking using MAUI Geolocation APIs
- 5-second polling interval for location updates
- Location permission handling for Android
- Accurate distance and bearing calculations using Haversine formula

### Discovery Mode
- View nearby treasures within 500m radius
- Real-time proximity indicator with hot/cold feedback:
  - 🔥 BURNING HOT: Within discovery radius (can collect)
  - 🔥 Hot: Within 2x discovery radius
  - 😊 Warm: Within 5x discovery radius
  - ❄️ Cool: Within 10x discovery radius
  - 🧊 Cold: Beyond 10x discovery radius
- Dynamic treasure list sorted by distance
- Collect treasures when within range
- Points tracking and leaderboard

### Creation Mode
- Drop treasures at current GPS location
- Configure treasure properties:
  - Name and description
  - Type (Common 100pts, Rare 300pts, Epic 500pts, Legendary 1000pts)
  - Discovery radius (1-50 meters)
- Location validation and confirmation

## Project Structure

```
src/TreasureHunter.MauiAndroid/
├── App.xaml / App.xaml.cs              # Application entry point
├── AppShell.xaml / AppShell.xaml.cs    # Shell navigation
├── MauiProgram.cs                       # Dependency injection configuration
├── Services/
│   ├── Models/                          # Data transfer objects
│   │   ├── TreasureItemDto.cs          # Treasure item with calculated fields
│   │   ├── AuthModels.cs                # Login/Register/Auth response models
│   │   ├── PlaceItemRequest.cs         # Place treasure request/response
│   │   └── CollectItemRequest.cs       # Collect treasure request/response
│   ├── ITreasureApiService.cs          # API service interface
│   ├── TreasureApiService.cs           # HttpClient-based API implementation
│   ├── IGeolocationService.cs          # Location service interface
│   └── GeolocationService.cs           # GPS tracking implementation
├── Pages/
│   ├── LoginPage.xaml / .cs            # User authentication page
│   ├── RegisterPage.xaml / .cs         # User registration page
│   └── MainPage.xaml / .cs             # Main app with Discovery/Creation modes
├── Platforms/Android/
│   ├── AndroidManifest.xml             # Android permissions and settings
│   ├── MainActivity.cs                  # Main Android activity
│   └── MainApplication.cs               # MAUI application for Android
└── Resources/                           # Fonts, images, styles, icons
```

## API Integration

The app communicates with the TreasureHunter API backend:

### Endpoints Used
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Authenticate user
- `GET /api/treasure/nearby` - Get treasures near location
- `POST /api/treasure` - Place new treasure
- `POST /api/treasure/{id}/collect` - Collect treasure

### Configuration
Update the base URL in `MauiProgram.cs`:

```csharp
builder.Services.AddHttpClient("TreasureHunterAPI", client =>
{
    client.BaseAddress = new Uri("https://your-api-url.com");
});
```

**Note**: In a production environment, consider using app settings or environment variables:
- For development: `https://localhost:5001` or your local API endpoint
- For production: Your deployed API URL (e.g., `https://api.treasurehunter.com`)

## Building the Project

### Prerequisites
1. .NET 10 SDK installed
2. MAUI workload installed: `dotnet workload install maui-android`
3. Android SDK installed (via Visual Studio or Android Studio)

### Build Commands

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build src/TreasureHunter.MauiAndroid/TreasureHunter.MauiAndroid.csproj

# Build in Release mode
dotnet build src/TreasureHunter.MauiAndroid/TreasureHunter.MauiAndroid.csproj -c Release
```

### Running on Android Device/Emulator

```bash
# Run on connected device or emulator
dotnet build src/TreasureHunter.MauiAndroid/TreasureHunter.MauiAndroid.csproj -t:Run -f net10.0-android
```

## Permissions

The app requires the following Android permissions (configured in AndroidManifest.xml):

- `ACCESS_COARSE_LOCATION` - For approximate location
- `ACCESS_FINE_LOCATION` - For precise GPS location
- `INTERNET` - For API communication
- `ACCESS_NETWORK_STATE` - For network status

## Development Notes

### Known Warnings
The build produces warnings about obsolete `Frame` usage. These are non-critical:
- Frame is deprecated in .NET 9+ in favor of Border
- Frame still works and these warnings can be safely ignored
- Future updates may migrate to Border for better performance

### Services Architecture
- **TreasureApiService**: Handles all HTTP communication with bearer token authentication
- **GeolocationService**: Manages GPS tracking with permission handling and 5-second polling
- Both services are registered as singletons for app-wide state management

### UI Design
- Material Design-inspired with custom colors
- Responsive layouts with ScrollView for different screen sizes
- Observable collections for dynamic treasure list updates
- Toast-style status messages with auto-hide

## Testing

### Manual Testing Checklist
1. ✅ Registration with new user
2. ✅ Login with existing credentials
3. ✅ GPS permission request on first launch
4. ✅ Location tracking indicator updates
5. ✅ Nearby treasures load and display correctly
6. ✅ Proximity indicator changes based on distance
7. ✅ Treasure collection when in range
8. ✅ Points update after collection
9. ✅ Switch to Creation mode
10. ✅ Place treasure with current location
11. ✅ Validation for required fields

### API Testing
Refer to the main repository's `API_TESTING.md` for backend API testing.

## Troubleshooting

### Build Issues
- **Missing workload**: Run `dotnet workload install maui-android`
- **Android SDK not found**: Install via Visual Studio Installer or Android Studio
- **Font errors**: Ensure OpenSans fonts are in Resources/Fonts/

### Runtime Issues
- **Location not updating**: Check Android permissions in device settings
- **API calls failing**: Verify base URL in MauiProgram.cs and network connectivity
- **Login fails**: Confirm API backend is running and accessible

## Future Enhancements

Potential features for future development:
- AR visualization using ARCore
- Offline mode with local caching
- Push notifications for nearby treasures
- Social features (friends, leaderboards)
- Map view integration
- Treasure photos and custom icons
- Sound effects and haptic feedback

## License

Part of the TreasureHunter project. See main repository for license details.

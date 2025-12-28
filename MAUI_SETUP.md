# TreasureHunter MAUI Blazor Hybrid App

## Overview
The MAUI Blazor Hybrid mobile application provides the client interface for the Treasure Hunter AR experience.

## Project Setup

To create the MAUI project, run:
```bash
dotnet new maui-blazor -n TreasureHunter.MauiApp -f net8.0
cd TreasureHunter.MauiApp
dotnet add reference ../TreasureHunter.Application/TreasureHunter.Application.csproj
```

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.*" />
<PackageReference Include="Microsoft.Maui.Controls" Version="8.0.*" />
<PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.*" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.*" />
```

## Key Features to Implement

### 1. GPS Location Services
- Use `Microsoft.Maui.Devices.Sensors.Geolocation` for GPS tracking
- Request location permissions
- Continuous location updates every 5 seconds

### 2. Creation Mode Page (CreateTreasure.razor)
```razor
@page "/create"
@inject ILocationService LocationService
@inject HttpClient Http

<h3>Drop a Treasure</h3>

<EditForm Model="@model" OnValidSubmit="@HandleCreate">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label>Treasure Name</label>
        <InputText @bind-Value="model.Name" class="form-control" />
    </div>
    
    <div class="form-group">
        <label>Description</label>
        <InputTextArea @bind-Value="model.Description" class="form-control" />
    </div>
    
    <div class="form-group">
        <label>Item Type</label>
        <InputSelect @bind-Value="model.ItemTypeId" class="form-control">
            <option value="1">Common Coin</option>
            <option value="2">Rare Gem</option>
            <option value="3">Epic Treasure Chest</option>
        </InputSelect>
    </div>
    
    <div class="form-group">
        <label>Discovery Radius (meters)</label>
        <InputNumber @bind-Value="model.DiscoveryRadiusMeters" class="form-control" />
    </div>
    
    <button type="submit" class="btn btn-primary">Drop Treasure</button>
</EditForm>

@code {
    private CreateTreasureModel model = new();
    
    protected override async Task OnInitializedAsync()
    {
        var location = await Geolocation.GetLocationAsync();
        if (location != null)
        {
            model.Latitude = location.Latitude;
            model.Longitude = location.Longitude;
            model.Altitude = location.Altitude;
        }
    }
    
    private async Task HandleCreate()
    {
        // Call API to create treasure
        var response = await Http.PostAsJsonAsync("/api/treasure", model);
        if (response.IsSuccessStatusCode)
        {
            // Navigate to discovery mode
            NavigationManager.NavigateTo("/discover");
        }
    }
}
```

### 3. Discovery Mode Page (DiscoverTreasure.razor)
```razor
@page "/discover"
@inject NavigationManager NavigationManager
@inject ILocationService LocationService
@implements IAsyncDisposable

<h3>Treasure Discovery</h3>

<div class="proximity-indicator">
    @if (nearestTreasure != null)
    {
        <h4>@nearestTreasure.Name</h4>
        <p>Distance: @nearestTreasure.Distance.ToString("F2")m</p>
        <p>Direction: @nearestTreasure.Bearing.ToString("F0")°</p>
        <div class="hot-cold-indicator @GetProximityClass()">
            @GetProximityText()
        </div>
        
        @if (nearestTreasure.WithinDiscoveryRadius)
        {
            <button @onclick="CollectTreasure" class="btn btn-success">
                Collect Treasure! (+@nearestTreasure.PointValue points)
            </button>
        }
    }
    else
    {
        <p>Searching for nearby treasures...</p>
    }
</div>

<div id="ar-view">
    <!-- AR View using A-Frame or Three.js -->
</div>

@code {
    private HubConnection? hubConnection;
    private TreasureProximityInfo? nearestTreasure;
    
    protected override async Task OnInitializedAsync()
    {
        // Connect to SignalR hub
        hubConnection = new HubConnectionBuilder()
            .WithUrl("https://your-api.com/hubs/treasure")
            .Build();
            
        hubConnection.On<List<TreasureProximityInfo>>("ProximityUpdate", (treasures) =>
        {
            nearestTreasure = treasures.FirstOrDefault();
            StateHasChanged();
        });
        
        await hubConnection.StartAsync();
        
        // Start location tracking
        StartLocationUpdates();
    }
    
    private async void StartLocationUpdates()
    {
        while (true)
        {
            var location = await Geolocation.GetLocationAsync();
            if (location != null && hubConnection?.State == HubConnectionState.Connected)
            {
                await hubConnection.SendAsync("UpdateLocation", location.Latitude, location.Longitude);
            }
            await Task.Delay(5000); // Update every 5 seconds
        }
    }
    
    private async Task CollectTreasure()
    {
        var location = await Geolocation.GetLocationAsync();
        var response = await Http.PostAsJsonAsync($"/api/treasure/{nearestTreasure.Id}/collect", 
            new { Latitude = location.Latitude, Longitude = location.Longitude });
            
        if (response.IsSuccessStatusCode)
        {
            // Play success sound
            // Show animation
            nearestTreasure = null;
        }
    }
    
    private string GetProximityClass() => nearestTreasure?.ProximityLevel switch
    {
        "VERY_HOT" => "very-hot",
        "HOT" => "hot",
        "WARM" => "warm",
        "COOL" => "cool",
        _ => "cold"
    };
    
    private string GetProximityText() => nearestTreasure?.ProximityLevel switch
    {
        "VERY_HOT" => "🔥 BURNING HOT!",
        "HOT" => "🔥 Hot!",
        "WARM" => "😊 Warm",
        "COOL" => "❄️ Cool",
        _ => "🧊 Cold"
    };
    
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

### 4. AR Visualization

Use A-Frame in a WebView or MAUI's native camera overlay:

```html
<!-- wwwroot/ar-scene.html -->
<html>
<head>
    <script src="https://aframe.io/releases/1.4.0/aframe.min.js"></script>
    <script src="https://raw.githack.com/AR-js-org/AR.js/master/aframe/build/aframe-ar.js"></script>
</head>
<body style="margin: 0; overflow: hidden;">
    <a-scene embedded arjs="sourceType: webcam; debugUIEnabled: false;">
        <a-entity id="treasure-marker">
            <!-- Dynamically loaded treasure models -->
        </a-entity>
        <a-camera gps-camera rotation-reader></a-camera>
    </a-scene>
    
    <script>
        // JavaScript to update 3D model positions based on GPS coordinates
        window.updateTreasurePosition = function(lat, lon, alt, modelUrl) {
            // Calculate position relative to user
            // Update A-Frame entity
        };
    </script>
</body>
</html>
```

### 5. Authentication

```csharp
// Services/AuthService.cs
public class AuthService
{
    private readonly HttpClient _httpClient;
    
    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", 
            new { UserName = username, Password = password });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            // Store token in SecureStorage
            await SecureStorage.SetAsync("auth_token", result.Token);
            return result;
        }
        
        throw new Exception("Login failed");
    }
    
    public async Task<string> GetTokenAsync()
    {
        return await SecureStorage.GetAsync("auth_token") ?? string.Empty;
    }
}
```

## Platform-Specific Configuration

### Android (AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.CAMERA" />
```

### iOS (Info.plist)
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs your location to find nearby treasures</string>
<key>NSCameraUsageDescription</key>
<string>This app needs camera access for AR features</string>
```

## API Client Configuration

```csharp
// MauiProgram.cs
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://your-api-url.com") 
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TreasureService>();
```

## Build and Run

```bash
# Android
dotnet build -t:Run -f net8.0-android

# iOS
dotnet build -t:Run -f net8.0-ios

# Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

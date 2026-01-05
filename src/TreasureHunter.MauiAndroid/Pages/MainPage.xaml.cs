using Camera.MAUI;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.Devices.Sensors;
using TreasureHunter.MauiAndroid.Graphics;
using TreasureHunter.MauiAndroid.Models;
using TreasureHunter.MauiAndroid.Services;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Pages;

public partial class MainPage : ContentPage
{
    private readonly IARCameraService _arService;
    private readonly IARUpdateManager _updateManager;
    private readonly ITreasureApiService _apiService;
    private HubConnection? _hubConnection; // This should now work
    private readonly TreasureOverlayDrawable _overlayDrawable;
    private readonly List<TreasureOverlay> _nearbyTreasures = new();
    private TreasureOverlay? _selectedTreasure;
    private bool _isInitialized;
    private DateTime _lastSignalRUpdate = DateTime.MinValue;
    private const int SignalRUpdateIntervalMs = 2000; // Send to server every 2 seconds

    public MainPage(IARCameraService arService, IARUpdateManager updateManager, ITreasureApiService apiService)
    {
        InitializeComponent();
        _arService = arService;
        _updateManager = updateManager;
        _apiService = apiService;
        _overlayDrawable = new TreasureOverlayDrawable();
        TreasureOverlay.Drawable = _overlayDrawable;

        // Subscribe to update events
        _updateManager.LocationChanged += OnLocationChanged;
        _updateManager.OrientationChanged += OnOrientationChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isInitialized)
        {
            await InitializeAsync();
            _isInitialized = true;
        }
        else
        {
            await _updateManager.StartAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _updateManager.StopAsync().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        LoadingIndicator.IsRunning = true;
        StatusLabel.Text = "Initializing AR view...";

        try
        {
            // Request permissions
            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (cameraStatus != PermissionStatus.Granted || locationStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Permissions Required", 
                    "Camera and location permissions are required for AR treasure hunting.", 
                    "OK");
                return;
            }

            // Start camera
            await CameraView.StartCameraAsync();

            // Start sensors
            await _arService.StartSensors();

            // Initialize SignalR connection
            await InitializeSignalRAsync();

            // Start update manager
            await _updateManager.StartAsync();

            StatusLabel.Text = "Searching for treasures...";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Initialization error: {ex}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task InitializeSignalRAsync()
    {
        var token = _apiService.GetAuthToken();
        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("///LoginPage");
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://10.0.2.2:5174/hubs/treasure", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<List<ProximityData>>("ProximityUpdate", OnProximityUpdate);
        _hubConnection.On<List<DiscoverableData>>("TreasureDiscoverable", OnTreasureDiscoverable);

        _hubConnection.Reconnecting += error =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Reconnecting to server...";
            });
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Connected! Searching for treasures...";
            });
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StatusLabel.Text = "Connection lost. Reconnecting...";
            });
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            System.Diagnostics.Debug.WriteLine("SignalR connection established");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connection failed: {ex.Message}");
            StatusLabel.Text = "Failed to connect to server";
        }

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Camera permission is required", "OK");
            return;
        }

    }

    private async void OnLocationChanged(object? sender, Location location)
    {
        // Update compass display
        var heading = _arService.CurrentHeading;
        var direction = GetCompassDirection(heading);
        CompassLabel.Text = $"{direction} {heading:F0}°";

        // Send location to server (throttled)
        var now = DateTime.Now;
        if ((now - _lastSignalRUpdate).TotalMilliseconds >= SignalRUpdateIntervalMs)
        {
            _lastSignalRUpdate = now;
            
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("UpdateLocation", 
                        location.Latitude, 
                        location.Longitude);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR update error: {ex.Message}");
                }
            }
        }

        // Update overlay positions
        UpdateOverlayPositions();
    }

    private void OnOrientationChanged(object? sender, EventArgs e)
    {
        // Device orientation changed, update overlay positions
        UpdateOverlayPositions();
    }

    private void UpdateOverlayPositions()
    {
        var location = _updateManager.CurrentLocation;
        if (location == null || _nearbyTreasures.Count == 0)
            return;

        var width = (float)TreasureOverlay.Width;
        var height = (float)TreasureOverlay.Height;

        if (width <= 0 || height <= 0)
            return;

        foreach (var treasure in _nearbyTreasures)
        {
            var (x, y) = _arService.CalculateScreenPosition(
                treasure.Latitude,
                treasure.Longitude,
                location,
                width,
                height
            );

            treasure.IsVisible = x >= 0 && x <= width && y >= 0 && y <= height;
            treasure.ScreenX = x;
            treasure.ScreenY = y;
        }

        _overlayDrawable.UpdateTreasures(_nearbyTreasures);
        
        // Invalidate on main thread to trigger redraw
        MainThread.BeginInvokeOnMainThread(() =>
        {
            TreasureOverlay.Invalidate();
        });
    }

    private void OnProximityUpdate(List<ProximityData> proximityData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Update existing treasures or add new ones
            foreach (var data in proximityData)
            {
                var existing = _nearbyTreasures.FirstOrDefault(t => t.Id == data.Id);
                if (existing != null)
                {
                    // Update existing treasure data
                    existing.Distance = data.Distance;
                    existing.Bearing = data.Bearing;
                    existing.ProximityLevel = data.ProximityLevel;
                }
                else
                {
                    // Add new treasure
                    _nearbyTreasures.Add(new TreasureOverlay
                    {
                        Id = data.Id,
                        Name = data.Name,
                        Latitude = data.Latitude,
                        Longitude = data.Longitude,
                        Distance = data.Distance,
                        Bearing = data.Bearing,
                        IconUrl = data.IconUrl ?? string.Empty,
                        ProximityLevel = data.ProximityLevel
                    });
                }
            }

            // Remove treasures that are no longer nearby
            var currentIds = proximityData.Select(d => d.Id).ToHashSet();
            _nearbyTreasures.RemoveAll(t => !currentIds.Contains(t.Id));

            TreasureCountLabel.Text = $"{_nearbyTreasures.Count} nearby";
            UpdateOverlayPositions();
        });
    }

    private void OnTreasureDiscoverable(List<DiscoverableData> discoverableData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var data in discoverableData)
            {
                var treasure = _nearbyTreasures.FirstOrDefault(t => t.Id == data.Id);
                if (treasure != null)
                {
                    treasure.PointValue = data.PointValue;
                }
            }

            if (discoverableData.Any())
            {
                _selectedTreasure = _nearbyTreasures
                    .Where(t => t.PointValue > 0)
                    .OrderBy(t => t.Distance)
                    .FirstOrDefault();

                if (_selectedTreasure != null)
                {
                    CollectButton.IsVisible = true;
                    StatusLabel.Text = $"🎯 {_selectedTreasure.Name} - {FormatDistance(_selectedTreasure.Distance)}";
                }
            }
            else
            {
                CollectButton.IsVisible = false;
                _selectedTreasure = null;
                StatusLabel.Text = "Keep searching...";
            }

            UpdateOverlayPositions();
        });
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
    {
        var point = e.GetPosition(TreasureOverlay);
        if (point == null)
            return;

        const float tapRadius = 40;
        var tappedTreasure = _nearbyTreasures.FirstOrDefault(t =>
            t.IsVisible &&
            Math.Abs(t.ScreenX - point.Value.X) < tapRadius &&
            Math.Abs(t.ScreenY - point.Value.Y) < tapRadius
        );

        if (tappedTreasure != null && tappedTreasure.PointValue > 0)
        {
            _selectedTreasure = tappedTreasure;
            OnCollectClicked(sender, EventArgs.Empty);
        }
    }

    private async void OnCollectClicked(object? sender, EventArgs e)
    {
        if (_selectedTreasure == null)
            return; 

        try
        {
            CollectButton.IsEnabled = false;
            LoadingIndicator.IsRunning = true;

            var collectResponse = await _apiService.CollectItemAsync(new CollectItemRequest
            {
                ItemId = Guid.TryParse(_selectedTreasure.Id, out var guid) ? guid : Guid.Empty,
                // Optionally, you may want to pass user location if required by your API
                // UserLatitude = ...,
                // UserLongitude = ...,
            });
            var result = collectResponse != null && collectResponse.Success;
            
            if (result)
            {
                await DisplayAlert("Success!", 
                    $"You collected {_selectedTreasure.Name} for +{_selectedTreasure.PointValue} points!", 
                    "Awesome!");
                
                _nearbyTreasures.Remove(_selectedTreasure);
                _selectedTreasure = null;
                CollectButton.IsVisible = false;
                UpdateOverlayPositions();
                
                // Refresh points display
                await RefreshUserPoints();
            }
            else
            {
                await DisplayAlert("Failed", "Could not collect treasure. Try getting closer.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            CollectButton.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task RefreshUserPoints()
    {
        try
        {
            // var userProfile = await _apiService.GetUserProfileAsync();
            // if (userProfile != null)
            // {
            //     PointsLabel.Text = $"{userProfile.TotalPoints} pts";
            // }
            // Optionally, set PointsLabel.Text to a default or cached value, or leave it unchanged.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing points: {ex.Message}");
        }
    }

    private static string GetCompassDirection(double heading)
    {
        var directions = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        var index = (int)Math.Round(heading / 45.0) % 8;
        return directions[index];
    }

    private static string FormatDistance(double meters)
    {
        if (meters < 1)
            return $"{(int)(meters * 100)}cm";
        if (meters < 100)
            return $"{meters:F1}m";
        return $"{(meters / 1000):F2}km";
    }

    // Helper classes for SignalR data
    private class ProximityData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }
        public double Bearing { get; set; }
        public string ProximityLevel { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    private class DiscoverableData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public int PointValue { get; set; }
    }
}

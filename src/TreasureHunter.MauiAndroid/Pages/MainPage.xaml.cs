using System.Collections.ObjectModel;
using TreasureHunter.MauiAndroid.Services;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Pages;

public partial class MainPage : ContentPage
{
    private readonly ITreasureApiService _apiService;
    private readonly IGeolocationService _geoService;
    private GeolocationData? _currentLocation;
    private ObservableCollection<TreasureViewModel> _treasures = new();
    private TreasureItemDto? _closestTreasure;
    private int _totalPoints = 0;
    private readonly int[] _typeIds = { 1, 2, 3, 4 }; // Map picker indices to type IDs

    public MainPage(ITreasureApiService apiService, IGeolocationService geoService)
    {
        InitializeComponent();
        _apiService = apiService;
        _geoService = geoService;
        
        TreasureCollection.ItemsSource = _treasures;
        TypePicker.SelectedIndex = 0;
        
        // Subscribe to location changes
        _geoService.LocationChanged += OnLocationChanged;
        
        // Start tracking when page loads
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var started = await _geoService.StartTrackingAsync();
        if (started)
        {
            TrackingLabel.Text = "📍 Tracking";
        }
        else
        {
            TrackingLabel.Text = "📍 Permission Denied";
        }
    }

    private async void OnLocationChanged(object? sender, GeolocationData location)
    {
        _currentLocation = location;
        
        // Update location label on creation mode
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LocationLabel.Text = $"Location: {location.Latitude:F6}, {location.Longitude:F6}";
        });

        // Refresh nearby treasures
        await RefreshNearbyTreasures();
    }

    private async Task RefreshNearbyTreasures()
    {
        if (_currentLocation == null) return;

        var items = await _apiService.GetNearbyItemsAsync(
            _currentLocation.Latitude, 
            _currentLocation.Longitude, 
            500); // 500m radius

        // Calculate distances and sort
        foreach (var item in items)
        {
            item.DistanceMeters = CalculateDistance(
                _currentLocation.Latitude, 
                _currentLocation.Longitude,
                item.Latitude,
                item.Longitude);
            
            item.BearingDegrees = CalculateBearing(
                _currentLocation.Latitude, 
                _currentLocation.Longitude,
                item.Latitude,
                item.Longitude);
        }

        var sortedItems = items.OrderBy(i => i.DistanceMeters).ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _treasures.Clear();
            foreach (var item in sortedItems)
            {
                _treasures.Add(new TreasureViewModel
                {
                    Name = item.Name,
                    TypeName = item.ItemTypeName,
                    PointsText = $"{item.PointValue} points",
                    DistanceText = $"{item.DistanceMeters:F0}m",
                    Emoji = GetEmojiForType(item.ItemTypeId)
                });
            }

            // Update proximity display
            UpdateProximityDisplay(sortedItems);
        });
    }

    private void UpdateProximityDisplay(List<TreasureItemDto> sortedItems)
    {
        _closestTreasure = sortedItems.FirstOrDefault();
        
        if (_closestTreasure != null && _closestTreasure.DistanceMeters.HasValue)
        {
            ProximityFrame.IsVisible = true;
            ClosestItemLabel.Text = _closestTreasure.Name;
            DistanceLabel.Text = $"{_closestTreasure.DistanceMeters:F1}m away";
            DirectionLabel.Text = $"Direction: {_closestTreasure.BearingDegrees:F0}°";

            var distance = _closestTreasure.DistanceMeters.Value;
            var radius = _closestTreasure.DiscoveryRadiusMeters;

            // Proximity levels
            if (distance <= radius)
            {
                ProximityLabel.Text = "🔥 BURNING HOT!";
                ProximityFrame.BackgroundColor = Colors.Red;
                CollectButton.IsVisible = true;
            }
            else if (distance <= radius * 2)
            {
                ProximityLabel.Text = "🔥 Hot!";
                ProximityFrame.BackgroundColor = Colors.Orange;
                CollectButton.IsVisible = false;
            }
            else if (distance <= radius * 5)
            {
                ProximityLabel.Text = "😊 Warm";
                ProximityFrame.BackgroundColor = Colors.Yellow;
                CollectButton.IsVisible = false;
            }
            else if (distance <= radius * 10)
            {
                ProximityLabel.Text = "❄️ Cool";
                ProximityFrame.BackgroundColor = Colors.LightBlue;
                CollectButton.IsVisible = false;
            }
            else
            {
                ProximityLabel.Text = "🧊 Cold";
                ProximityFrame.BackgroundColor = Colors.LightGray;
                CollectButton.IsVisible = false;
            }
        }
        else
        {
            ProximityFrame.IsVisible = false;
            CollectButton.IsVisible = false;
        }
    }

    private async void OnCollectClicked(object sender, EventArgs e)
    {
        if (_closestTreasure == null || _currentLocation == null) return;

        var request = new CollectItemRequest
        {
            ItemId = _closestTreasure.Id,
            UserLatitude = _currentLocation.Latitude,
            UserLongitude = _currentLocation.Longitude
        };

        var response = await _apiService.CollectItemAsync(request);

        if (response != null && response.Success)
        {
            _totalPoints += response.PointsEarned;
            PointsLabel.Text = $"Points: {_totalPoints}";
            await ShowStatusMessage($"🎉 Collected! +{response.PointsEarned} points");
            
            // Refresh treasures
            await RefreshNearbyTreasures();
        }
        else
        {
            await ShowStatusMessage(response?.Message ?? "Collection failed");
        }
    }

    private void OnDiscoveryModeClicked(object sender, EventArgs e)
    {
        DiscoveryLayout.IsVisible = true;
        CreationLayout.IsVisible = false;
        DiscoveryButton.BackgroundColor = Color.FromArgb("#512BD4"); // Primary
        CreationButton.BackgroundColor = Color.FromArgb("#DFD8F7"); // Secondary
    }

    private void OnCreationModeClicked(object sender, EventArgs e)
    {
        DiscoveryLayout.IsVisible = false;
        CreationLayout.IsVisible = true;
        DiscoveryButton.BackgroundColor = Color.FromArgb("#DFD8F7"); // Secondary
        CreationButton.BackgroundColor = Color.FromArgb("#512BD4"); // Primary
    }

    private async void OnPlaceClicked(object sender, EventArgs e)
    {
        if (_currentLocation == null)
        {
            await ShowStatusMessage("Location not available");
            return;
        }

        var name = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await ShowStatusMessage("Please enter a treasure name");
            return;
        }

        if (TypePicker.SelectedIndex < 0)
        {
            await ShowStatusMessage("Please select a treasure type");
            return;
        }

        var request = new PlaceItemRequest
        {
            Name = name,
            Description = DescriptionEditor.Text?.Trim(),
            ItemTypeId = _typeIds[TypePicker.SelectedIndex],
            Latitude = _currentLocation.Latitude,
            Longitude = _currentLocation.Longitude,
            Altitude = _currentLocation.Altitude,
            DiscoveryRadiusMeters = RadiusSlider.Value
        };

        var response = await _apiService.PlaceItemAsync(request);

        if (response != null && response.Success)
        {
            await ShowStatusMessage("✅ Treasure placed successfully!");
            
            // Clear form
            NameEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            TypePicker.SelectedIndex = 0;
            RadiusSlider.Value = 5;
            
            // Switch back to discovery mode
            OnDiscoveryModeClicked(this, EventArgs.Empty);
        }
        else
        {
            await ShowStatusMessage(response?.Message ?? "Failed to place treasure");
        }
    }

    private void OnTypePickerChanged(object sender, EventArgs e)
    {
        // Nothing to do, just for UI feedback
    }

    private void OnRadiusChanged(object sender, ValueChangedEventArgs e)
    {
        RadiusLabel.Text = $"{e.NewValue:F0} meters";
    }

    private async Task ShowStatusMessage(string message)
    {
        StatusMessageLabel.Text = message;
        StatusMessageFrame.IsVisible = true;
        
        await Task.Delay(3000);
        
        StatusMessageFrame.IsVisible = false;
    }

    private string GetEmojiForType(int typeId)
    {
        return typeId switch
        {
            1 => "🪙",
            2 => "💎",
            3 => "📦",
            4 => "👑",
            _ => "🎁"
        };
    }

    // Haversine formula for distance calculation
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    // Calculate bearing between two points
    private double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRadians(lon2 - lon1);
        var y = Math.Sin(dLon) * Math.Cos(ToRadians(lat2));
        var x = Math.Cos(ToRadians(lat1)) * Math.Sin(ToRadians(lat2)) -
                Math.Sin(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Cos(dLon);
        
        var bearing = Math.Atan2(y, x);
        return (ToDegrees(bearing) + 360) % 360;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;
    private double ToDegrees(double radians) => radians * 180 / Math.PI;

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _ = _geoService.StopTrackingAsync();
    }
}

public class TreasureViewModel
{
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string PointsText { get; set; } = string.Empty;
    public string DistanceText { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
}

using System.Diagnostics;

namespace TreasureHunter.MauiAndroid.Services;

public class GeolocationService : IGeolocationService
{
    private CancellationTokenSource? _cts;
    private bool _isTracking;

    public event EventHandler<GeolocationData>? LocationChanged;
    public bool IsTracking => _isTracking;

    public async Task<GeolocationData?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                return new GeolocationData
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Altitude = location.Altitude,
                    Accuracy = location.Accuracy,
                    Timestamp = DateTime.UtcNow
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCurrentLocationAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> StartTrackingAsync()
    {
        if (_isTracking)
        {
            return true;
        }

        try
        {
            // Request location permission
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                
                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Location permission denied");
                    return false;
                }
            }

            _isTracking = true;
            _cts = new CancellationTokenSource();

            // Start background tracking loop
            _ = Task.Run(async () =>
            {
                while (_isTracking && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var locationData = await GetCurrentLocationAsync();
                        if (locationData != null)
                        {
                            LocationChanged?.Invoke(this, locationData);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Tracking loop error: {ex.Message}");
                    }
                }
            }, _cts.Token);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"StartTrackingAsync error: {ex.Message}");
            _isTracking = false;
            return false;
        }
    }

    public Task StopTrackingAsync()
    {
        _isTracking = false;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        return Task.CompletedTask;
    }
}

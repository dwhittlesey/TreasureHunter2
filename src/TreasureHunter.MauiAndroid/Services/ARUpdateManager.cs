using System.Diagnostics;
using Microsoft.Maui.Devices.Sensors;

namespace TreasureHunter.MauiAndroid.Services;

public interface IARUpdateManager
{
    event EventHandler<Location>? LocationChanged;
    event EventHandler? OrientationChanged;
    Task StartAsync();
    Task StopAsync();
    Location? CurrentLocation { get; }
    bool IsRunning { get; }
}

public class ARUpdateManager : IARUpdateManager
{
    private readonly IARCameraService _arService;
    private PeriodicTimer? _locationTimer;
    private PeriodicTimer? _renderTimer;
    private CancellationTokenSource? _cancellationTokenSource;
    private Location? _currentLocation;
    private bool _isRunning;

    // Configuration
    private const int LocationUpdateIntervalMs = 1000; // 1 second
    private const int RenderUpdateIntervalMs = 33; // ~30 FPS

    public Location? CurrentLocation => _currentLocation;
    public bool IsRunning => _isRunning;

    public event EventHandler<Location>? LocationChanged;
    public event EventHandler? OrientationChanged;

    public ARUpdateManager(IARCameraService arService)
    {
        _arService = arService;
        _arService.SensorDataChanged += OnSensorDataChanged;
    }

    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();

        // Start location updates
        StartLocationUpdates(_cancellationTokenSource.Token);

        // Start render loop
        StartRenderLoop(_cancellationTokenSource.Token);

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        
        _locationTimer?.Dispose();
        _renderTimer?.Dispose();
        
        _locationTimer = null;
        _renderTimer = null;
        _cancellationTokenSource = null;

        await Task.CompletedTask;
    }

    private void StartLocationUpdates(CancellationToken cancellationToken)
    {
        _locationTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(LocationUpdateIntervalMs));

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _locationTimer.WaitForNextTickAsync(cancellationToken);
                    await UpdateLocationAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Location update error: {ex.Message}");
                }
            }
        }, cancellationToken);
    }

    private void StartRenderLoop(CancellationToken cancellationToken)
    {
        _renderTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(RenderUpdateIntervalMs));

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _renderTimer.WaitForNextTickAsync(cancellationToken);
                    
                    // Trigger render update (orientation changes trigger this via sensor event)
                    if (_currentLocation != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            OrientationChanged?.Invoke(this, EventArgs.Empty);
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Render loop error: {ex.Message}");
                }
            }
        }, cancellationToken);
    }

    private async Task UpdateLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                var previousLocation = _currentLocation;
                _currentLocation = location;

                // Only notify if location changed significantly (> 1 meter)
                if (previousLocation == null || 
                    CalculateDistance(previousLocation, location) > 1.0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LocationChanged?.Invoke(this, location);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting location: {ex.Message}");
        }
    }

    private void OnSensorDataChanged(object? sender, EventArgs e)
    {
        // Sensor data changed, trigger orientation update
        if (_currentLocation != null && _isRunning)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OrientationChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    private static double CalculateDistance(Location loc1, Location loc2)
    {
        const double earthRadius = 6371000; // meters

        var lat1Rad = loc1.Latitude * Math.PI / 180;
        var lat2Rad = loc2.Latitude * Math.PI / 180;
        var deltaLat = (loc2.Latitude - loc1.Latitude) * Math.PI / 180;
        var deltaLon = (loc2.Longitude - loc1.Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }
}
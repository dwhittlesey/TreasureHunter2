namespace TreasureHunter.MauiAndroid.Services;

public class GeolocationData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Accuracy { get; set; }
    public DateTime Timestamp { get; set; }
}

public interface IGeolocationService
{
    Task<GeolocationData?> GetCurrentLocationAsync();
    Task<bool> StartTrackingAsync();
    Task StopTrackingAsync();
    event EventHandler<GeolocationData>? LocationChanged;
    bool IsTracking { get; }
}
